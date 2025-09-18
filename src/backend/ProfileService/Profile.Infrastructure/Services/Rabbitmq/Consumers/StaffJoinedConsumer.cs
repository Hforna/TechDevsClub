using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Services.Rabbitmq.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Rabbitmq.Consumers
{
    public class StaffJoinedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<StaffJoinedConsumer> _logger;
        private readonly BaseRabbitMqConnectionDto _rabbitMqConnection;

        public StaffJoinedConsumer(IServiceProvider serviceProvider, 
            ILogger<StaffJoinedConsumer> logger, IOptions<BaseRabbitMqConnectionDto> rabbitMqConnection)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _rabbitMqConnection = rabbitMqConnection.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = await new ConnectionFactory()
            {
                HostName = _rabbitMqConnection.Host,
                Port = _rabbitMqConnection.Port,
                Password = _rabbitMqConnection.Password,
                UserName = _rabbitMqConnection.UserName,
            }.CreateConnectionAsync();

            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync("from-career", "direct", true, false);
            await _channel.QueueDeclareAsync("staff-joined", true, true, false);
            await _channel.QueueBindAsync("staff-joined", "from-career", "staff.joined");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());

                try
                {
                    var deserialize = JsonSerializer.Deserialize<StaffJoinedMessage>(message);
                    if (deserialize is null)
                    {
                        _logger.LogError(message, "Message received couldn't be deserialized");

                        throw new Exception("It was not possible deserializing the message received");
                    }

                    await ProcessMessage(deserialize!);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                } catch (ContextException ex)
                {
                    _logger.LogError(ex, $"An error occured while trying to use context");

                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }catch(Exception ex)
                {
                    _logger.LogError(ex, "Unexpectadly occured while trying to consume message from career service");

                    throw;
                }
                await _channel.BasicConsumeAsync("staff-joined", false, consumer);
            };
        }

        private async Task ProcessMessage(StaffJoinedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();

            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var sqids = scope.ServiceProvider.GetRequiredService<SqidsEncoder<long>>();

            var userId = sqids.Decode(message.UserId);
            var user = await uow.GenericRepository.GetById<User>(userId.Single());
            if (user is null)
            {
                _logger.LogError($"The user got by {userId} in staff joined class was not found in context");

                throw new ContextException("The by id user was not found", System.Net.HttpStatusCode.NotFound);
            }
            var addToRole = await userManager.AddToRoleAsync(user, "staff");
            if (!addToRole.Succeeded)
                throw new ContextException("Couldn't assign user to role staff", System.Net.HttpStatusCode.InternalServerError);
        }
    }
}

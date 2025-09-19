using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Dtos;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Infrastructure.Messaging.Rabbitmq.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Career.Infrastructure.Messaging.Rabbitmq.Consumers
{
    public class UsersMatchedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<UsersMatchedConsumer> _logger;
        private readonly BaseRabbitMqConnectionDto _baseRabbitMqConnection;

        public UsersMatchedConsumer(IServiceProvider serviceProvider, IConnection connection, 
            IChannel channel, ILogger<UsersMatchedConsumer> logger, IOptions<BaseRabbitMqConnectionDto> baseRabbitMqConnection)
        {
            _serviceProvider = serviceProvider;
            _connection = connection;
            _channel = channel;
            _logger = logger;
            _baseRabbitMqConnection = baseRabbitMqConnection.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = await new ConnectionFactory()
            {
                Port = _baseRabbitMqConnection.Port,
                HostName = _baseRabbitMqConnection.Host,
                Password = _baseRabbitMqConnection.Password,
                UserName = _baseRabbitMqConnection.UserName
            }.CreateConnectionAsync();

            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync("from-profile", "direct", true, false);
            await _channel.QueueDeclareAsync("user-matched", false, false);
            await _channel.QueueBindAsync("user-matched", "from-profile", "user.matched");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                var body = ea.Body;
                var bodyEncoded = Encoding.UTF8.GetString(body.ToArray());

                try
                {
                    var deserialize = JsonSerializer.Deserialize<UsersMatchedToJobMessage>(bodyEncoded);
                    if (deserialize is null)
                    {
                        _logger.LogError(bodyEncoded, "Message received couldn't be deserialized");

                        throw new Exception("It was not possible deserializing the message received");
                    }
                    await ConsumeMessage(deserialize);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }catch(Exception ex)
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync("user-matched", false, consumer);
        }

        private async Task ConsumeMessage(UsersMatchedToJobMessage dto)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var uof = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var company = await uof.CompanyRepository.CompanyById(dto.CompanyId);
                var job = await uof.GenericRepository.GetById<Job>(dto.JobId);

                var userEmails = dto.Users.Select(u => new SimpleEmailDto(u.Email, u.UserName));
                var message = new BatchEmailDto(userEmails.ToList(), 
                    company.Name, 
                    company.Location.ToString(), 
                    $"localhost:8080/api/jobs/{job.Id}", 
                    job.Title);

                await emailService.SendBatchEmails(message);
            }
        }

        public override void Dispose()
        {
            _channel.CloseAsync();
            _channel.Dispose();
            _connection.CloseAsync();
            _connection.Dispose();
            base.Dispose();
        }
    }
}

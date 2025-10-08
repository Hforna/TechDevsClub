using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Services.Rabbitmq.Messages;
using Profile.Infrastructure.Services.Rabbitmq.Producers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Rabbitmq.Consumers
{
    public class JobCreatedConsumer : BackgroundService
    {
        private readonly ILogger<JobCreatedConsumer> _logger;
        private IConnection _connection;
        private IChannel _channel;
        private IServiceProvider _serviceProvider;
        private BaseRabbitMqConnectionDto _rabbitMqConnection;

        public JobCreatedConsumer(ILogger<JobCreatedConsumer> logger, 
            IServiceProvider serviceProvider, IOptions<BaseRabbitMqConnectionDto> rabbitMqConnection)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
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
            await _channel.QueueDeclareAsync("job-created", true, true, false);
            await _channel.QueueBindAsync("job-created", "from-career", "job.created");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                var body = ea.Body;
                var bodyString = Encoding.UTF8.GetString(body.ToArray());

                try
                {
                    var message = JsonSerializer.Deserialize<JobCreatedMessage>(bodyString);

                    if(message is null)
                    {
                        _logger.LogError("It was not possible to deserialize message received");

                        throw new Exception($"Couldn't deserialize the message: {bodyString}");
                    }

                    await ProcessMessage(message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, true);
                }
                catch (ContextException ex)
                {
                    _logger.LogError(ex, $"An error occured while trying to use context");

                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpectadly occured while trying to consume message from career service");

                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };
            await _channel.BasicConsumeAsync("job-created", false, consumer);
        }

        private async Task ProcessMessage(JobCreatedMessage message)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var uof = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var sqids = scope.ServiceProvider.GetRequiredService<SqidsEncoder<long>>();
                var userMatchedProducer = scope.ServiceProvider.GetRequiredService<IUserMatchedProducer>();
                var skills = message.JobRequirements.ToDictionary(d => sqids.Decode(d.SkillId).Single(), f => (int)f.ExperienceTime);
                var users = await uof.UserRepository.GetUsersMatchedWithSkillsExperienceAndCountry(skills, message.Country);

                if (users is null)
                    await Task.CompletedTask;

                var batches = users.Chunk(50);

                foreach(var batch in batches)
                {
                    var userMatchedMessage = new UsersMatchedToJobMessage()
                    {
                        Users = batch.Select(user => new UserMessage(sqids.Encode(user.Id), user.UserName, user.Email)).ToList(),
                        CompanyId = message.CompanyId,
                        JobId = message.Id
                    };

                    await userMatchedProducer.SendMessage(userMatchedMessage);
                }
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

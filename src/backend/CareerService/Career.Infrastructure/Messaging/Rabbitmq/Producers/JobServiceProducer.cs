using Career.Domain.Dtos;
using Career.Domain.Services.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Career.Infrastructure.Messaging.Rabbitmq.Producers
{
    public class JobServiceProducer : IJobServiceProducer
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly BaseRabbitMqConnectionDto _connectionDto;
        private readonly ILogger<JobServiceProducer> _logger;

        public JobServiceProducer(IOptions<BaseRabbitMqConnectionDto> connectionDto, ILogger<JobServiceProducer> logger)
        {
            _connectionDto = connectionDto.Value;
            _logger = logger;
        }

        public async Task SendJobCreated(JobCreatedDto dto)
        {
            _connection = await new ConnectionFactory()
            {
                Port = _connectionDto.Port,
                HostName = _connectionDto.Host,
                Password = _connectionDto.Password,
                UserName = _connectionDto.UserName
            }.CreateConnectionAsync();

            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync("from-career", "direct", true, false);

            var body = JsonSerializer.Serialize(dto);
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            await _channel.BasicPublishAsync("from-career", "job.created", bodyBytes);
        }
    }
}

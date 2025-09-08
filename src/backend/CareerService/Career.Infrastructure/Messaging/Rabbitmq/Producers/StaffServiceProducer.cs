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
    public class StaffServiceProducer : IDisposable, IStaffServiceProducer
    { 
        private IConnection _connection;
        private readonly ILogger<StaffServiceProducer> _logger;
        private readonly BaseRabbitMqConnectionDto _baseConnection;
        private IChannel _channel;

        public StaffServiceProducer(ILogger<StaffServiceProducer> logger, 
            IOptions<BaseRabbitMqConnectionDto> baseConnection)
        {
            _logger = logger;
            _baseConnection = baseConnection.Value;
        }

        public async Task StaffAcceptJoinedCompany(StaffJoinedDto dto)
        {
            _connection = await new ConnectionFactory()
            {
                Port = _baseConnection.Port,
                HostName = _baseConnection.Host,
                Password = _baseConnection.Password,
                UserName = _baseConnection.UserName
            }.CreateConnectionAsync();

            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync("from-career", "direct", true, false);
            await _channel.QueueDeclareAsync("staff-joined", true, false, false);
            await _channel.QueueBindAsync("staff-joined", "from-career", "staff.joined");

            var serialize = JsonSerializer.Serialize(dto);
            var body = Encoding.UTF8.GetBytes(serialize);
            await _channel.BasicPublishAsync("from-career", "staff.joined", body);
        }

        public void Dispose()
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}

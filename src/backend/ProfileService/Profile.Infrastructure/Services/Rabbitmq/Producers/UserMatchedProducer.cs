using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profile.Infrastructure.Services.Rabbitmq.Consumers;
using Profile.Infrastructure.Services.Rabbitmq.Messages;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Rabbitmq.Producers
{
    public interface IUserMatchedProducer
    {
        public Task SendMessage(UsersMatchedToJobMessage message);
    }

    public class UserMatchedProducer : IUserMatchedProducer
    {
        private readonly ILogger<UserMatchedProducer> _logger;
        private IConnection _connection;
        private IChannel _channel;
        private IServiceProvider _serviceProvider;
        private BaseRabbitMqConnectionDto? _rabbitMqConnection;

        public UserMatchedProducer(ILogger<UserMatchedProducer> logger, IConnection connection,
            IChannel channel, IServiceProvider serviceProvider, IOptions<BaseRabbitMqConnectionDto> rabbitMqConnection)
        {
            _logger = logger;
            _connection = connection;
            _channel = channel;
            _serviceProvider = serviceProvider;
            _rabbitMqConnection = rabbitMqConnection.Value;
        }

        public async Task SendMessage(UsersMatchedToJobMessage message)
        {
            _connection = await new ConnectionFactory()
            {
                HostName = _rabbitMqConnection.Host,
                Port = _rabbitMqConnection.Port,
                Password = _rabbitMqConnection.Password,
                UserName = _rabbitMqConnection.UserName,
            }.CreateConnectionAsync();

            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync("from-profile", "direct", true, false);

            var body = JsonSerializer.Serialize(message);
            var getBytes = Encoding.UTF8.GetBytes(body);
            await _channel.BasicPublishAsync("from-profile", "user.matched", getBytes);
        }
    }
}

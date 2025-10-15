using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Career.Infrastructure.Messaging.Rabbitmq.Consumers
{
    public class AnalyzeJobProducer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<UsersMatchedConsumer> _logger;
        private readonly BaseRabbitMqConnectionDto _baseRabbitMqConnection;

        public AnalyzeJobProducer(IServiceProvider serviceProvider, ILogger<UsersMatchedConsumer> logger, 
            IOptions<BaseRabbitMqConnectionDto> baseRabbitMqConnection)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _baseRabbitMqConnection = baseRabbitMqConnection.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }
    }
}
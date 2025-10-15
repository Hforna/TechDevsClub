using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sqids;

namespace Profile.Infrastructure.Services.Rabbitmq.Consumers;

public class StaffRemovedFromCompany : BackgroundService
{
    private readonly ILogger<StaffRemovedFromCompany> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection _connection;
    private IChannel _channel;
    private readonly BaseRabbitMqConnectionDto _connectionDto;
    
    public StaffRemovedFromCompany(ILogger<StaffRemovedFromCompany> logger, IServiceProvider serviceProvider, IOptions<BaseRabbitMqConnectionDto>  connectionDto)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connectionDto = connectionDto.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await new ConnectionFactory()
        {
            HostName = _connectionDto.Host,
            Port = _connectionDto.Port,
            Password = _connectionDto.Password,
            UserName = _connectionDto.UserName,
        }.CreateConnectionAsync(stoppingToken);

        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("from-career", "direct", true, false, cancellationToken:stoppingToken);
        await _channel.QueueDeclareAsync("staff-removed", true, false, false, cancellationToken:stoppingToken);
        await _channel.QueueBindAsync("staff-removed", "from-career", "staff.removed", cancellationToken:stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (ModuleHandle, ea) =>
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());

            try
            {
                using var scope = _serviceProvider.CreateScope();
                
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var uow =  scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var sqids = scope.ServiceProvider.GetRequiredService<SqidsEncoder<long>>();

                var userIds = JsonSerializer.Deserialize<List<string>>(message);
                foreach (var userId in userIds)
                {
                    long decodeId = sqids.Decode(userId).Single();
                    var user = await uow.GenericRepository.GetById<User>(decodeId);
                    if (user is null)
                    {
                        _logger.LogError("User with id {userId} was not found while trying to remove from staff role", userId);
                        throw new Exception("User was not found");
                    }
                    await userManager.RemoveFromRoleAsync(user, "staff");
                }
                await uow.Commit(stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while removing staff");

                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync("staff-removed", false, consumer, cancellationToken:stoppingToken);
    }
}
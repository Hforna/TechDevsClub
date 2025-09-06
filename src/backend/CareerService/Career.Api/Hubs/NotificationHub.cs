using Career.Api.Hubs.Responses;
using Career.Domain.Dtos;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Career.Api.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }
    }

    public class NotificationHubService : IRealTimeNotifier
    {
        private readonly IHubContext<NotificationHub> _context;
        private readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(IHubContext<NotificationHub> context, ILogger<NotificationHubService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendNotification(SendNotificationDto dto)
        {
            var response = new NotificationSentResponse(dto.Id, dto.SenderId, dto.Title, dto.CreatedAt);

            _logger.LogInformation($"Message sent to user with id: {dto.UserId}");
            await _context.Clients.User(dto.UserId).SendAsync("ReceiveMessagee", response);
        }
    }
}

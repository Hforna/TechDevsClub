using Career.Api.Hubs.Responses;
using Career.Domain.Dtos.Notifications;
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

        public async Task SendInformationNotificationManyUsers(InformationNotificationManyUsersDto dto)
        {
            var response = dto.Notifications.Select(d => new InformationNotificationSentResponse(d.Id, d.Title, d.CreatedAt)).ToList();
            var users = dto.Notifications.Select(d => d.UserId).ToList();

            await _context.Clients.Users(users).SendAsync("ReceiveInformationNotifications", response);
            _logger.LogInformation($"Message sent to users with ids: {users}");
        }

        public async Task SendNotification(SendNotificationDto dto)
        {
            var response = new NotificationSentResponse(dto.Id, dto.SenderId, dto.Title, dto.CreatedAt);

            await _context.Clients.User(dto.UserId).SendAsync("ReceiveMessage", response);
            _logger.LogInformation($"Message sent to user with id: {dto.UserId}");
        }
    }
}

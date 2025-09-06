using Career.Api.Hubs.Responses;
using Career.Domain.Dtos;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Career.Api.Hubs
{
    public class NotificationHub : Hub, IRealTimeNotifier
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task SendNotification(SendNotificationDto dto)
        {
            var response = new NotificationSentResponse(dto.Id, dto.SenderId, dto.Title, dto.CreatedAt);

            _logger.LogInformation($"Notification sent to user: {dto.UserId}");

            await Clients.User(dto.UserId).SendAsync("ReciveNotification", response);    
        }

    }
}

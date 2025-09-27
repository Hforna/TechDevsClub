using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos.Notifications
{
    public sealed record SendNotificationDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string? SenderId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }

    public sealed record InformationNotificationManyUsersDto
    {
        public InformationNotificationManyUsersDto(List<SendNotificationDto> notifications)
        {
            Notifications = notifications;
        }

        public List<SendNotificationDto> Notifications { get; set; }
    }
}

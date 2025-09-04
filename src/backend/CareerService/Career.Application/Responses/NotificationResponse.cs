using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string SenderId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public ENotificationType Type { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }

    public class ShortNotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public ENotificationType Type { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }

    public class NotificationsPaginatedResponse
    {
        public int Count { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public List<ShortNotificationResponse> Notifications { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
    }
}

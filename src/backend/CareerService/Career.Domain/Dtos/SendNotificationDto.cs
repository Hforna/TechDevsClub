using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public class SendNotificationDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string SenderId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}

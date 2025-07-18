using Career.Domain.Enums;
using Career.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Entities
{
    [Table("notifications")]
    public class Notification
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public ENotificationType Type { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

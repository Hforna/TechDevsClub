using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Rabbitmq.Messages
{
    public sealed record StaffJoinedMessage
    {
        public Guid StaffId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public Guid CompanyId { get; set; }
    }
}

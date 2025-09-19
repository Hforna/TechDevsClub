using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Messaging.Rabbitmq.Messages
{
    public class UsersMatchedToJobMessage
    {
        public Guid JobId { get; set; }
        public Guid CompanyId { get; set; }
        public List<UserMessage> Users { get; set; }
    }

    public sealed record UserMessage
    {
        public UserMessage(string id, string userName, string email)
        {
            Id = id;
            UserName = userName;
            Email = email;
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}

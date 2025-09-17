using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Rabbitmq.Messages
{
    public class UsersMatchedToJobMessage
    {
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

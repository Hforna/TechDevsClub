using Profile.Domain.Aggregates;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class UserInfosResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserSkills> Skills { get; set; }
    }

    public class UserInfosWithRolesResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserRolesResponse UserRoles { get; set; }
        public ICollection<UserSkills> Skills { get; set; }
    }
}

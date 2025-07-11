using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class UserRolesResponse
    {
        public string UserId { get; set; }
        public List<UserRoleResponse> Roles { get; set; }
    }
}

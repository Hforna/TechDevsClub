using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record UserInfosDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public List<UserRoleDto>? UserRoles { get; set; }
    }
}

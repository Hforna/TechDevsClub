using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record UserRoleDto
    {
        public string Id { get; set; }
        public string Role { get; set; }
    }
}

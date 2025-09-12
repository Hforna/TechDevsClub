using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record StaffJoinedDto
    {
        public Guid StaffId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public Guid CompanyId { get; set; }
    }
}

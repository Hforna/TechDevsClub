using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests
{
    public class PutStaffOnCompanyRequest
    {
        public string UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string Role { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class StaffResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string Role { get; set; }
    }

    public class StaffsResponse
    {
        public Guid CompanyId { get; set; }
        public List<StaffResponse> Staffs { get; set; }
    }
}

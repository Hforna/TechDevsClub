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
    }
}

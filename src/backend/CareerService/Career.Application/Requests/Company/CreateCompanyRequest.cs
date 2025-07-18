using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests.Company
{
    public class CreateCompanyRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public CompanyLocationRequest CompanyLocation { get; set; }
    }
}

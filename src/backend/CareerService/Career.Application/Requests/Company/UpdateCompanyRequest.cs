using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests.Company
{
    public class UpdateCompanyRequest
    {
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CompanyLocationRequest CompanyLocation { get; set; }
        public IFormFile Logo { get; set; }
        public string Website { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests
{
    public class CompanyLocationRequest
    {
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
    }
}

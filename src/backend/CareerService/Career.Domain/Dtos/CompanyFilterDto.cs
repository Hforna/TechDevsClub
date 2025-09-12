using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Dtos
{
    public sealed record CompanyFilterDto
    {
        public string? Name { get; set; }
        public bool? Verified { get; set; }
        public ECompanyRate? Rate { get; set; }
        public string? Country { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests.Company
{
    public class CompanyConfigurationRequest
    {
        public bool IsPrivate { get; set; }
        public bool ShowStaffs { get; set; }
        public bool HighlightVerifiedStatus { get; set; } = true;
        public bool NotifyStaffsOnNewReview { get; set; } = false;
        public bool NotifyStaffsOnNewJobApplication { get; set; } = false;
        public bool NotifyStaffsOnJobApplicationUpdate { get; set; } = false;
    }
}

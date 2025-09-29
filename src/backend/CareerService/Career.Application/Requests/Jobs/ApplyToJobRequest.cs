using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests.Jobs
{
    public class ApplyToJobRequest
    {
        public IFormFile Resume { get; set; }
        public decimal? DesiredSalary { get; set; }
    }
}

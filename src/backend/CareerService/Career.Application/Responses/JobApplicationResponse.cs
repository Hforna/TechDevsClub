using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class JobApplicationResponse
    {
        public Guid Id { get; set; }
        public DateTime AppliedAt { get; private set; } = DateTime.UtcNow;
        public EApplicationStatus Status { get; set; }
        public Guid JobId { get; set; }
        public string ProfileId { get; set; }
        public decimal? DesiredSalary { get; set; }
    }

    public class JobApplicationPaginatedResponse
    {
        public List<JobApplicationResponse> JobApplications { get; set; }
        public int TotalApplied { get; set; }
        public int TotalInterview { get; set; }
        public int TotalRejected { get; set; }
        public int Count { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
    }
}

using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Enums;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Aggregates.JobRoot
{
    [Table("jobs")]
    public class Job
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PostedAt { get; set; }
        public bool IsActive { get; set; }
        public Guid CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public Salary Salary { get; set; }
        public ICollection<JobRequirement> JobRequirements { get; set; }
    }
}

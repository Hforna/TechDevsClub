using Career.Domain.Entities;
using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Aggregates.JobRoot
{
    [Table("job_applications")]
    public class JobApplication: Entity, IEntity
    {
        public Guid Id { get; set; }
        public DateTime AppliedAt { get; private set; } = DateTime.UtcNow;
        public EApplicationStatus Status { get; set; }
        public Guid JobId { get; set; }
        [ForeignKey("JobId")]
        public Job Job { get; set; }
        public string ProfileId { get; set; }
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public decimal? DesiredSalary { get; set; }
        public string ResumeName { get; set; }
    }
}

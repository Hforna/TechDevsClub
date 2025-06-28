using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Aggregates.JobRoot
{
    [Table("job_requirements")]
    public class JobRequirement
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Job Job { get; set; }
        public string SkillId { get; set; }
        public bool IsMandatory { get; set; }
        public string ExperienceTime { get; set; }
    }
}

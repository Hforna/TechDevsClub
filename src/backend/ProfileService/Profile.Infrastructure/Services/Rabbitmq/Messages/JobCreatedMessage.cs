using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Rabbitmq.Messages
{
    public sealed record JobCreatedMessage
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PostedAt { get; set; }
        public bool IsActive { get; set; }
        public Guid CompanyId { get; set; }
        public string Country { get; set; }
        public ICollection<JobRequirementMessage> JobRequirements { get; set; }
    }

    public sealed record JobRequirementMessage
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string SkillId { get; set; }
        public bool IsMandatory { get; set; }
        public ExperienceLevelMessage ExperienceTime { get; set; }
    }

    public enum ExperienceLevelMessage
    {
        [Display(Name = "No experience")]
        NoExperience,

        [Display(Name = "Less than 1 year")]
        LessThanOneYear,

        [Display(Name = "1-2 years")]
        OneToTwoYears,

        [Display(Name = "2-5 years")]
        TwoToFiveYears,

        [Display(Name = "5+ years")]
        FivePlusYears,

        [Display(Name = "10+ years")]
        TenPlusYears
    }
}

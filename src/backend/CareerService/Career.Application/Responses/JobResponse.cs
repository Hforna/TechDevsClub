using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Enums;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class JobResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PostedAt { get; set; }
        public bool IsActive { get; set; }
        public Guid CompanyId { get; set; }
        public SalaryResponse Salary { get; set; }
        public List<JobRequirementResponse> Requirements { get; set; }
    }

    public class JobRequirementResponse
    {
        public Guid JobId { get; set; }
        public string SkillId { get; set; }
        public bool IsMandatory { get; set; }
        public ExperienceLevel ExperienceTime { get; set; }
    }

    public class SalaryResponse
    {
        public decimal MinSalary { get; private set; }
        public decimal MaxSalary { get; private set; }
        public ECurrency Currency { get; private set; }
    }
}

using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Requests.Jobs
{
    public class CreateJobRequest
    {
        public Guid CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public SalaryRequest? Salary { get; set; }
        public List<JobRequirementRequest> JobRequirements { get; set; }
        public ExperienceLevel Experience { get; set; }
    }

    public class JobRequirementRequest
    {
        public string SkillId { get; set; }
        public bool IsMandatory { get; set; }
        public ExperienceLevel ExperienceTime { get; set; }
    }

    public class SalaryRequest
    {
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public ECurrency Currency { get; set; }
    }
}

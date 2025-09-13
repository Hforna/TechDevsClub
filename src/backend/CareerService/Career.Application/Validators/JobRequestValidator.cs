using Career.Application.Requests.Jobs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Validators
{
    public class JobRequestValidator : AbstractValidator<CreateJobRequest>
    {
        public JobRequestValidator()
        {
            RuleFor(d => d.JobRequirements.Select(d => d.SkillId)).Must(IsSkillsUnique).WithMessage("Skill ids must be unique in job requirements");
        }

        public bool IsSkillsUnique(IEnumerable<string> skillIds) => skillIds.Count() == skillIds.Distinct().Count();
    }
}

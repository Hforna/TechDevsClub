using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Enums;
using Profile.Domain.Exceptions;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Aggregates
{
    [Table("users")]
    public class User : IdentityUser<long>
    {
        public bool Active { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserSkills> Skills { get; set; } = [];
        public Address? Address { get; set; }

        public void AddSkill(long skillId, LevelsEnum level)
        {
            if (Skills.Select(d => d.SkillId).Contains(skillId))
                throw new DomainException(ResourceExceptMessages.USER_HAS_SKILL, System.Net.HttpStatusCode.BadRequest);

            Skills.Add(new UserSkills(Id, skillId, level));
        }
    }

    [Table("users_skills")]
    public class UserSkills
    {
        public long UserId { get; set; }
        public long SkillId { get; set; }
        public LevelsEnum Level { get; set; }

        public UserSkills(long userId, long skillId, LevelsEnum level)
        {
            UserId = userId;
            SkillId = skillId;
            Level = level;
        }

        public void PromoteLevel()
        {
            Level = Level switch
            {
                LevelsEnum.NEWBIE => LevelsEnum.JUNIOR,
                LevelsEnum.JUNIOR => LevelsEnum.MID,
                LevelsEnum.MID => LevelsEnum.SENIOR,
                LevelsEnum.SENIOR => LevelsEnum.TECHLEAD,
                _ => Level
            };
        }

        public class Mapping : IEntityTypeConfiguration<UserSkills>
        {
            public void Configure(EntityTypeBuilder<UserSkills> builder)
            {
                builder.HasKey(us => new { us.UserId, us.SkillId });
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Aggregates
{
    [Table("profiles")]
    public class ProfileEntity : IEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public required long UserId { get; set; }
        public required User User { get; set; }
        public string Description { get; set; }
        public GithubMetadata GithubMeta { get; set; }
        public EmploymentStatusEnum EmploymentStatus { get; set; } = EmploymentStatusEnum.NotLooking;
        public ICollection<SocialLink> SocialLinks { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsPrivate { get; set; } = false;

        public class Mapping : IEntityTypeConfiguration<ProfileEntity>
        {
            public void Configure(EntityTypeBuilder<ProfileEntity> builder)
            {
                builder.HasOne(d => d.User);

                builder.HasMany(d => d.SocialLinks).WithOne(d => d.Profile);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profile.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Profile.Domain.Aggregates
{
    public class SocialLink
    {
        public long Id { get; set; }
        public long ProfileId { get; set; }
        public ProfileEntity Profile { get; set; }
        public string Name { get; set; }

        private string _link;
        public string Link
        {
            get => _link;
            set
            {
                var isValid = Regex.IsMatch(value,
                    @"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w- .\/?%&=]*)?$");

                if (!isValid)
                    throw new DomainException(ResourceExceptMessages.INVALID_URL_FORMAT,
                        System.Net.HttpStatusCode.BadRequest);

                _link = value;
            }
        }

        public class Mapping : IEntityTypeConfiguration<SocialLink>
        {
            public void Configure(EntityTypeBuilder<SocialLink> builder)
            {
                builder.HasOne(d => d.Profile)
                       .WithMany(d => d.SocialLinks)
                       .HasForeignKey(d => d.ProfileId);
            }
        }
    }
}

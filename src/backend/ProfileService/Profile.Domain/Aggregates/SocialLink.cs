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
        public string Link {
            get { return this.Link; }
            set {
                var isValid = Regex.Match(value, 
                    "/[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)/gi");

                if (!isValid.Success)
                    throw new DomainException(ResourceExceptMessages.INVALID_URL_FORMAT, System.Net.HttpStatusCode.BadRequest);

                Link = value;
            } 
        }

        public class Mapping : IEntityTypeConfiguration<SocialLink>
        {
            public void Configure(EntityTypeBuilder<SocialLink> builder)
            {
                builder.HasOne(d => d.Profile).WithMany(d => d.SocialLinks);
            }
        }
    }
}

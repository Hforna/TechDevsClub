using Org.BouncyCastle.Asn1.Mozilla;
using Profile.Domain.Aggregates;
using Profile.Domain.Enums;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class UpdateProfileRequest
    {
        public string Description { get; set; }
        public string? GitHubUserName { get; set; }
        public EmploymentStatusEnum EmploymentStatus { get; set; }
        public List<SocialLinkRequest> SocialLinks { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsPrivate { get; set; }
    }
}

using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class ProfileResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public EmploymentStatusEnum EmploymentStatus { get; set; }
        public List<SocialLinksResponse> SocialLinks { get; set; }
        public GithubMetadataResponse GithubMetadata { get; set; }
        public List<UserSkillsResponse> UserSkills { get; set; }
        public bool IsPrivate { get; set; }
        public string ProfilePicture { get; set; }
    }
}

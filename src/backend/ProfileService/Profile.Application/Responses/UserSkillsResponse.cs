using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Responses
{
    public class UserSkillsResponse
    {
        public List<SkillUserResponse> Skills { get; set; }
    }
}

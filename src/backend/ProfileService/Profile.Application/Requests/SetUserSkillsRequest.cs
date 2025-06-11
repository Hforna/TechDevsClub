using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class SetUserSkillsRequest
    {
        public List<CreateUserSkillRequest> Skills { get; set; }
    }
}

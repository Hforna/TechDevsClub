using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class RemoveUserSkillsRequest
    {
        public List<RemoveUserSkillRequest> Skills { get; set; }
    }
}

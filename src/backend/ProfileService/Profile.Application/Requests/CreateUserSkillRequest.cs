using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Requests
{
    public class CreateUserSkillRequest
    {
        public string Name { get; set; }
        public LevelsEnum Level { get; set; }
    }
}

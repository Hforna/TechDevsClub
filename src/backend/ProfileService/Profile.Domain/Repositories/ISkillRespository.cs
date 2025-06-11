using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface ISkillRespository
    {
        public Task<bool> SkillExists(string name);
        public Task<Skill?> GetSkillByName(string name);
        public Task<UserSkills?> GetUserSkills(User user);
        public Task<List<Skill>?> GetSkillsByNames(string[] names);
    }
}

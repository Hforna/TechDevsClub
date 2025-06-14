using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface ISessionService
    {
        public Dictionary<long, List<string>>? GetProfilesVisitedByUser();
        public void SetProfileVisitedByUser(long profileId, List<Skill> skills);
    }
}

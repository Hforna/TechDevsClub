using Profile.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Profile.Domain.Repositories
{
    public interface IProfileRepository
    {
        public Task<ProfileEntity?> ProfileById(long id, bool asNoTracking = false);
        public Task<ProfileEntity?> ProfileByUser(User user);
        public Task<List<ProfileEntity>?> GetProfilesWithGithub();
        public Task<IPagedList<ProfileEntity>?> ProfilesBySkills(List<string> skills, int page, int perPage);
        public Task<ProfileEntity?> ProfileByUsername(string name);
    }
}

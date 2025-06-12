using Profile.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface IProfileRepository
    {
        public Task<ProfileEntity?> ProfileById(long id);
        public Task<ProfileEntity?> ProfileByUser(User user);
        public Task<List<ProfileEntity>?> GetProfilesWithGithub();
        public Task<ProfileEntity?> ProfileByUsername(string name);
    }
}

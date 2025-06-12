using Microsoft.EntityFrameworkCore;
using Profile.Domain.Aggregates;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly DataContext _context;

        public ProfileRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<ProfileEntity>?> GetProfilesWithGithub()
        {
            return await _context.Profiles
                .Where(d => string.IsNullOrEmpty(d.GithubMeta.Username) == false)
                .ToListAsync();
        }

        public async Task<ProfileEntity?> ProfileById(long id)
        {
            var profile = await _context.Profiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.Id == id && d.User.Active);

            return profile;
        }

        public async Task<ProfileEntity?> ProfileByUser(User user)
        {
            return await _context.Profiles.SingleOrDefaultAsync(d => d.User == user);
        }

        public async Task<ProfileEntity?> ProfileByUsername(string name)
        {
            return await _context.Profiles.SingleOrDefaultAsync(d => d.User.UserName == name);
        }
    }
}

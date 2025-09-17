using Microsoft.EntityFrameworkCore;
using Profile.Domain.Aggregates;
using Profile.Domain.Enums;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories.Relational
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context) => _context = context;

        public async Task<List<User>?> GetUsersMatchedWithSkillsAndExperience(Dictionary<long, int> skills, string country)
        {
            var minExp = Math.Ceiling(skills.Count() * 0.3);

            return await _context.Users
                .AsNoTracking()
                .Where(d => d.Skills
                    .Where(skill => skills.ContainsKey(skill.UserId))
                    .Count(d => (int)d.Experience > skills[d.SkillId]) >= minExp && d.Address!.Country == country)
                .ToListAsync();
        }

        public async Task<User?> UserByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(d => d.Email == email && d.EmailConfirmed);
        }

        public async Task<User?> UserByEmailNotConfirmed(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(d => d.Email == email);
        }

        public async Task<User?> UserByIdentifier(Guid uid)
        {
            return await _context.Users
                .Include(d => d.Skills)
                .ThenInclude(d => d.Skill)
                .SingleOrDefaultAsync(d => d.UserIdentifier == uid && d.Active);
        }
    }
}

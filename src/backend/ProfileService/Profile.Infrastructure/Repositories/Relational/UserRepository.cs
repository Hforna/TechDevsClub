using Microsoft.EntityFrameworkCore;
using Profile.Domain.Aggregates;
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
            return await _context.Users.Include(d => d.Skills).ThenInclude(d => d.Skill).SingleOrDefaultAsync(d => d.UserIdentifier == uid && d.Active);
        }
    }
}

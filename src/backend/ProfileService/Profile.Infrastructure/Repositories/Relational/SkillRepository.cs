using Microsoft.EntityFrameworkCore;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories.Relational
{
    public class SkillRepository : ISkillRespository
    {
        private readonly DataContext _context;

        public SkillRepository(DataContext context) => _context = context;

        public async Task<Skill?> GetSkillByName(string name)
        {
            return await _context.Skills.SingleOrDefaultAsync(d => d.Name == name);
        }

        public async Task<List<Skill>?> GetSkillsByNames(string[] names)
        {
            return await _context.Skills.Where(d => names.Contains(d.Name)).ToListAsync();
        }

        public async Task<UserSkills?> GetUserSkills(User user)
        {
            return await _context.UserSkills.SingleOrDefaultAsync(d => d.UserId == user.Id);
        }

        public async Task<bool> SkillExists(string name)
        {
            return await _context.Skills.AnyAsync(d => d.Name == name);
        }
    }
}

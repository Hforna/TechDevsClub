using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace Profile.Infrastructure.Repositories.Relational
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

        public async Task<ProfileEntity?> ProfileById(long id, bool asNoTracking = false)
        {
            var profile = _context.Profiles
                .Include(d => d.User);

            if (asNoTracking)
                profile.AsNoTracking();

            return await profile.SingleOrDefaultAsync(d => d.Id == id && d.User.Active);
        }

        public async Task<ProfileEntity?> ProfileByUser(User user)
        {
            return await _context.Profiles.SingleOrDefaultAsync(d => d.User == user);
        }

        public async Task<ProfileEntity?> ProfileByUsername(string name)
        {
            return await _context.Profiles.SingleOrDefaultAsync(d => d.User.UserName == name);
        }

        public async Task<IPagedList<ProfileEntity>?> ProfilesBySkills(List<string> skills, int page, int perPage)
        {
            var profileTableName = _context.Model.FindEntityType(typeof(ProfileEntity))!.GetTableName();
            var userTableName = _context.Model.FindEntityType(typeof(User))!.GetTableName();
            var userSkillsTableName = _context.Model.FindEntityType(typeof(UserSkills))!.GetTableName();
            var skillsTableName = _context.Model.FindEntityType(typeof(Skill))!.GetTableName();

            var query = $@"
            SELECT DISTINCT P.* FROM {profileTableName} AS P 
            JOIN {userTableName} AS U ON U.Id = P.UserId
            JOIN {userSkillsTableName} AS US ON U.Id = US.UserId
            JOIN {skillsTableName} AS S ON US.SkillId = S.Id";

            var parameters = new List<SqlParameter>();
            var paramNames = new List<string>();
            
            for (int i = 0; i < skills.Count; i++)
            {
                paramNames.Add($"@p{i}");
                parameters.Add(new SqlParameter($"@p{i}", skills[i]));
            }
            
            query += $" WHERE S.Name IN ({string.Join(", ", paramNames)})";
            
            var profiles = await _context.Profiles
                .FromSqlRaw(query, parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();
            
            return profiles.ToPagedList(page, perPage);
        }
    }
}

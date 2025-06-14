using Microsoft.AspNetCore.Http;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public SessionService(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
            _session = _httpContext.HttpContext.Session;
        }

        public Dictionary<long, List<string>>? GetProfilesVisitedByUser()
        {
            if(_session.TryGetValue("profiles_visited", out var value))
            {
                var deserialize = JsonSerializer.Deserialize<Dictionary<long, List<string>>>(value);

                return deserialize;
            }
            return null;
        }

        public void SetProfileVisitedByUser(long profileId, List<Skill> skills)
        {
            var profiles = new Dictionary<long, List<string>>();

            if(_session.TryGetValue($"profiles_visited", out var value))
            {
                var deserialize = JsonSerializer.Deserialize<Dictionary<long, List<string>>>(value);

                profiles = deserialize;
            }
            profiles.Add(profileId, skills.Select(d => d.Name).ToList());

            var serializeList = JsonSerializer.Serialize(profiles);
            
            _session.SetString($"profiles_visited", serializeList);
        }
    }
}

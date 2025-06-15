using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<SessionService> _logger;

        public SessionService(IHttpContextAccessor httpContext, ILogger<SessionService> logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        public Dictionary<long, List<string>>? GetProfilesVisitedByUser()
        {
            var session = _httpContext.HttpContext.Session;

            if (session.TryGetValue("profiles_visited", out var value))
            {
                var deserialize = JsonSerializer.Deserialize<Dictionary<long, List<string>>>(value);

                return deserialize;
            }
            return null;
        }

        public void SetProfileVisitedByUser(long profileId, List<Skill> skills)
        {
            var profiles = new Dictionary<long, List<string>>();

            var session = _httpContext.HttpContext!.Session;
            _logger.LogInformation($"Session got: {session.Id}");

            if (session.TryGetValue("profiles_visited", out var value))
            {
                var deserialize = JsonSerializer.Deserialize<Dictionary<long, List<string>>>(value);

                profiles = deserialize;
            }
            profiles![profileId] = profiles.ContainsKey(profileId) == false 
                ?  skills.Select(d => d.Name).ToList() 
                : profiles[profileId];

            var serializeList = JsonSerializer.Serialize(profiles);

            _logger.LogInformation($"New list after add profile visited: {serializeList}");
            
            session.SetString("profiles_visited", serializeList);
        }
    }
}

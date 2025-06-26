using Profile.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Services.Security
{
    public interface ITokenService
    {
        public string GenerateToken(List<Claim> claims, Guid userIdentifier);
        public Guid GetUserIdentifierByToken();
        public long GetDeviceId(string token);
        public Guid ValidateToken(string token);
        public DateTime GetRefreshTokenExpiration();
        public List<Claim> GetTokenClaims(string token);
        public Task<User> GetUserByToken();
        public string GenerateRefreshToken();
    }
}

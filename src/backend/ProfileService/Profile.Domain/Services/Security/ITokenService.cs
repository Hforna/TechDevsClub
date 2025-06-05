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
        public Guid GetUserIdentifierByToken(string token);
        public DateTime GetRefreshTokenExpiration();
        public string GenerateRefreshToken();
    }
}

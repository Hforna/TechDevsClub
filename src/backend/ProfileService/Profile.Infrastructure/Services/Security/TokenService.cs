using Microsoft.IdentityModel.Tokens;
using Profile.Domain.Services.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.Security
{
    public class TokenService : ITokenService
    {
        private readonly int _expiresOn;
        private readonly string _signKey;
        private readonly int _refreshHoursExpiration;

        public TokenService(int expiresOn, string signKey, int refreshHoursExpiration)
        {
            _expiresOn = expiresOn;
            _signKey = signKey;
            _refreshHoursExpiration = refreshHoursExpiration;
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public string GenerateToken(List<Claim> claims, Guid userIdentifier)
        {
            claims.Add(new Claim(ClaimTypes.Sid, userIdentifier.ToString()));

            var descriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow.AddMinutes(_expiresOn),
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(GetSecurityKey(), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();

            var create = handler.CreateToken(descriptor);

            return handler.WriteToken(create);
        }

        public DateTime GetRefreshTokenExpiration()
        {
            return DateTime.UtcNow.AddHours(_refreshHoursExpiration);
        }

        SymmetricSecurityKey GetSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey));
    }
}

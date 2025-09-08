using Microsoft.IdentityModel.Tokens;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
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
        private readonly IRequestService _requestService;
        private readonly IUnitOfWork _uof;

        public TokenService(int expiresOn, string signKey, int refreshHoursExpiration, 
            IRequestService requestService, IUnitOfWork uof)
        {
            _expiresOn = expiresOn;
            _uof = uof;
            _signKey = signKey;
            _refreshHoursExpiration = refreshHoursExpiration;
            _requestService = requestService;
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public DateTime GenerateTimeForAccessTokenExpires() => DateTime.UtcNow.AddMinutes(_expiresOn);

        public string GenerateToken(List<Claim> claims, Guid userIdentifier, string userId)
        {
            claims.Add(new Claim(ClaimTypes.Sid, userIdentifier.ToString()));
            claims.Add(new Claim("userId", userId));

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

        public long GetDeviceId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var read = handler.ReadJwtToken(token);
            var id = read.Claims.FirstOrDefault(d => d.Type == "device-id")!.Value;

            return long.Parse(id);
        }

        public DateTime GetRefreshTokenExpiration()
        {
            return DateTime.UtcNow.AddHours(_refreshHoursExpiration);
        }

        public Guid GetUserIdentifierByToken()
        {
            var token = _requestService.GetAccessToken();

            if (string.IsNullOrEmpty(token))
                throw new AuthenticationException(ResourceExceptMessages.TOKEN_IS_NULL, System.Net.HttpStatusCode.BadRequest);

            var handler = new JwtSecurityTokenHandler();
            var read = handler.ReadJwtToken(token);
            var uid = Guid.Parse(read.Claims.FirstOrDefault(d => d.Type == ClaimTypes.Sid).Value);

            return uid;
        }

        public List<Claim> GetTokenClaims(string token)
        {
            var @params = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSecurityKey(),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, @params, out SecurityToken validated);

            return result.Claims.ToList();
        }

        public Guid ValidateToken(string token)
        {
            var @params = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSecurityKey(),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, @params, out SecurityToken validated);
            var uid = Guid.Parse(result.Claims.FirstOrDefault(d => d.Type == ClaimTypes.Sid)!.Value);

            return uid;
        }

        SymmetricSecurityKey GetSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey));

        public async Task<User> GetUserByToken()
        {
            var uid = GetUserIdentifierByToken();

            var user = await _uof.UserRepository.UserByIdentifier(uid);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_DOESNT_EXISTS, System.Net.HttpStatusCode.NotFound);

            return user!;
        }
    }
}

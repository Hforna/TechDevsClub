using Career.Domain.Services.Clients;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Career.Api.Hubs
{
    public class CustomUserProvider : IUserIdProvider
    {
        private readonly IConfiguration _configuration;

        public CustomUserProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? GetUserId(HubConnectionContext connection)
        {
            var context = connection.GetHttpContext();

            if (context is null)
                return null;

            string? token = context.Request.Query["access_token"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("services:jwt:signKey")!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            var userId = principal.Claims.First(c => c.Type == "userId").Value;

            return userId;
        }
    }
}

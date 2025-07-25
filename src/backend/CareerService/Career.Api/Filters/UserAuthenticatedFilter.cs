using Career.Domain.Exceptions;
using Career.Domain.Services.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Career.Api.Filters
{
    public class UserAuthenticatedAttribute : TypeFilterAttribute
    {
        public UserAuthenticatedAttribute() : base(typeof(UserAuthenticatedFilter))
        {
        }
    }

    public class UserAuthenticatedFilter : IAsyncAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAuthenticatedFilter> _logger;


        public UserAuthenticatedFilter(IConfiguration configuration, 
            ILogger<UserAuthenticatedFilter> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers.Authorization.ToString();
            var jwtSettings = _configuration.GetSection("jwt");

            if (string.IsNullOrEmpty(token))
                throw new RequestException(ResourceExceptMessages.USER_NOT_AUTHENTICATED);

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SignKey"]!));

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = key,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogError(ex, $"Request bearer token expired: {ex.Message}");

                throw new RequestException(ResourceExceptMessages.TOKEN_EXPIRED);
            }
            catch (SecurityTokenInvalidTypeException ex)
            {
                _logger.LogError(ex, $"Invalid format token: {token}");

                throw new RequestException(ResourceExceptMessages.INVALID_TOKEN_FORMAT);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpectadly error occured: {ex.Message}");

                throw;
            }
        }
    }
}

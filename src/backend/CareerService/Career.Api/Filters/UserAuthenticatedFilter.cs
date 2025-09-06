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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserAuthenticatedFilter> _logger;

        public UserAuthenticatedFilter(IServiceProvider serviceProvider, 
            ILogger<UserAuthenticatedFilter> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(token))
                throw new RequestException(ResourceExceptMessages.USER_NOT_AUTHENTICATED);

            try
            {
                using(var scope = _serviceProvider.CreateScope())
                {
                    var profileService = scope.ServiceProvider.GetRequiredService<IProfileServiceClient>();

                    var userInfos = await profileService.GetUserInfos(token["Bearer ".Length..].Trim());
                }
            }
            catch(ClientException ex)
            {
                _logger.LogError(ex, $"Error while trying to request profile service: {ex.Message}");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpectadly error occured: {ex.Message}");

                throw;
            }
        }
    }
}

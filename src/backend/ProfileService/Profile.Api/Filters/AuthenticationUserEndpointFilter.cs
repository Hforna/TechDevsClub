using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services.Security;

namespace Profile.Api.Filters
{
    public class AuthenticationUserEndpointFilter : IEndpointFilter
    {
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uof;

        public AuthenticationUserEndpointFilter(ITokenService tokenService, IUnitOfWork uof)
        {
            _tokenService = tokenService;
            _uof = uof;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var token = context.HttpContext.Request.Headers.Authorization.ToString();

            var exception = new AuthenticationException(ResourceExceptMessages.USER_NOT_AUTHENTICATED, System.Net.HttpStatusCode.Unauthorized);

            if (string.IsNullOrEmpty(token))
                throw exception;

            token = token["Bearer ".Length..].Trim();

            try
            {
                var validate = _tokenService.ValidateToken(token);

                var user = await _uof.UserRepository.UserByIdentifier(validate);

                if (user is null)
                    throw exception;

                return await next(context);
            }
            catch (SecurityTokenExpiredException stee)
            {
                return Results.Problem(
                    detail: stee.Message,
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        }
    }
}

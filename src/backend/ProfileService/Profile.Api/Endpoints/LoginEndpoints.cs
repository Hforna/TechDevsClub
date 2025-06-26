using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;
using Profile.Application.Services;
using Profile.Domain.Exceptions;
using System.Security.Claims;

namespace Profile.Api.Endpoints
{
    public static class LoginEndpoints
    {
        const string LoginGroup = $"{BaseEndpointConsts.rootEndpoint}login";

        public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(LoginGroup);


            app.MapPost("application", LoginByApplication)
                .WithName("LoginByApplication")
                .WithSummary("User login on their account by email and password")
                .WithDescription("User login on account by email and password with an access token for authenticate on application");

            app.MapGet("github", LoginByGitHub);

            return app;
        }

        static async Task<IResult> LoginByApplication([FromBody] LoginRequest request, [FromServices] ILoginService service)
        {
            var result = await service.LoginByApplication(request);

            return Results.Ok(result);
        }

        static async Task<IResult> LoginByGitHub(HttpContext context, [FromServices]ILoginService service)
        {
            var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (IsNotAuthenticated(result))
                return Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                {
                    RedirectUri = $"https://localhost:56075/api/login/github"
                },
                authenticationSchemes: new List<string>() { "GitHub" });

            var email = result.Principal.Claims.FirstOrDefault(d => ClaimTypes.Email == d.Type);
            if(email is null)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.BadRequest("E-mail not provided");
            }

            try
            {
                var user = await service.AuthenticateUserByOAuth(email.Value, result.Principal.Claims.ToList());

                return Results.Ok(user);
            } catch(ContextException cx)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                throw;
            }
        }

        static bool IsNotAuthenticated(AuthenticateResult result)
        {
            return result.Principal is null
                || result.Principal.Identities.Any(d => d.IsAuthenticated == false)
                || result.Succeeded.Equals(false);
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Profile.Api.Filters;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;
using Profile.Application.Services;
using Profile.Domain.Exceptions;
using System.Security.Claims;

namespace Profile.Api.Endpoints
{
    public static class UserEndpoints
    {
        const string UserGroup = $"{BaseEndpointConsts.rootEndpoint}users";

        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(UserGroup);

            app.MapPost("create", CreateUser)
                .WithName("CreateUser")
                .WithSummary("Create a user request and confirm e-mail after");

            app.MapGet("confirm-email", ConfirmEmail)
                .WithName("ConfirmUserEmail")
                .WithSummary("Verify an user e-mail after them create an account");

            app.MapPut("update-address", UpdateUserAddress)
                .WithName("UpdateUserAddress")
                .WithSummary("Update the address of an user")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>();

            app.MapPut("update-password", UpdatePassword)
                .AddEndpointFilter<AuthenticationUserEndpointFilter>();

            app.MapGet("forgot-password", ForgotPassword)
                .WithName("ForgotAccountPassword")
                .WithSummary("Request a redefinition password by user email");

            app.MapPost("reset-password", ResetPassword)
                .WithName("ResetUserPassword")
                .WithSummary("Reset a password with token that was sent on e-mail");

            app.MapGet("create-github", CreateUserByGitHub)
                .WithName("CreateUserByGitHubOAuth")
                .WithSummary("Create an user by github authentication, user will be redirected to their github accoun for application authorization");

            return app;
        }

        [ProducesResponseType(typeof(ValidationException), StatusCodes.Status400BadRequest)]
        static async Task<IResult> CreateUser([FromServices] IUserService service, [FromBody] CreateUserRequest request)
        {
            var result = await service.CreateUser(request);

            return Results.Created(string.Empty, result);
        }

        static async Task<IResult> CreateUserByGitHub([FromServices] IUserService service, HttpContext context)
        {
            var result = await context.AuthenticateAsync("GitHub");

            if (IsNotAuthenticated(result))
                return Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                {
                    RedirectUri = "localhost:51334/api/users/create-github"
                }, authenticationSchemes: new List<string>() { "GitHub" });


            var email = result.Principal.Claims.FirstOrDefault(d => ClaimTypes.Email == d.Type)!;
            var name = result.Principal.Claims.FirstOrDefault(d => ClaimTypes.Name == d.Type)!.Value;
            if (email is null)
            {
                await context.SignOutAsync();
                return Results.BadRequest("E-mail not provided by github");
            }
            await service.CreateUserByOauth(email.Value, name);
            await context.SignOutAsync();

            return Results.Redirect("localhost:51334/api/login/github");
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AuthenticationException), StatusCodes.Status400BadRequest)]
        static async Task<IResult> ConfirmEmail([FromServices] IUserService service, [FromQuery] string email, [FromQuery] string token)
        {
            await service.ConfirmEmail(email, token);

            return Results.Ok();
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        static async Task<IResult> UpdateUserAddress([FromServices] IUserService service, [FromBody] UpdateAddressRequest request)
        {
            var result = await service.UpdateUserAddress(request);

            return Results.Ok(result);
        }

        static async Task<IResult> UpdatePassword([FromBody] UpdatePasswordRequest request, [FromServices] IUserService service)
        {
            await service.UpdatePassword(request);

            return Results.NoContent();
        }

        static async Task<IResult> ForgotPassword([FromQuery] string email, [FromServices] IUserService service)
        {
            await service.ForgotPassword(email);

            return Results.Ok();
        }

        static async Task<IResult> ResetPassword([FromQuery] string email, [FromQuery] string token, 
            [FromBody]ResetPasswordRequest request, [FromServices]IUserService service)
        {
            await service.ResetPassword(email, token, request);

            return Results.Ok();
        }

        static bool IsNotAuthenticated(AuthenticateResult result)
        {
            return result.Principal is null
                || result.Principal.Identities.Any(d => d.IsAuthenticated == false)
                || result.Succeeded.Equals(false);
        }
    }
}

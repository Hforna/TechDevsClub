using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Profile.Api.Filters;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;
using Profile.Application.Services;
using Profile.Domain.Exceptions;

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

            return app;
        }

        [ProducesResponseType(typeof(ValidationException), StatusCodes.Status400BadRequest)]
        static async Task<IResult> CreateUser([FromServices] IUserService service, [FromBody] CreateUserRequest request)
        {
            var result = await service.CreateUser(request);

            return Results.Created(string.Empty, result);
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
    }
}

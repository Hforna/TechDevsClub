using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Profile.Api.Filters;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;
using Profile.Domain.Exceptions;
using System.Security.Authentication;

namespace Profile.Api.Endpoints
{
    public static class UserEndpoints
    {
        const string UserGroup = $"{BaseEndpointConsts.rootEndpoint}user";

        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(UserGroup);

            app.MapPost("create", CreateUser)
                .WithName("CreateUser")
                .WithSummary("Create a user request and confirm e-mail after");

            app.MapGet("confirm-email", ConfirmEmail)
                .WithName("ConfirmUserEmail")
                .WithSummary("Verify an user e-mail after them create an account");

            app.MapPost("update-address", UpdateUserAddress)
                .WithName("UpdateUserAddress")
                .WithSummary("Update the address of an user")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>();

            return app;
        }

        [ProducesResponseType(typeof(ValidationException), StatusCodes.Status400BadRequest)]
        static async Task<IResult> CreateUser([FromServices]IUserService service, [FromBody]CreateUserRequest request)
        {
            var result = await service.CreateUser(request);

            return Results.Created(string.Empty, result);
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AuthenticationException), StatusCodes.Status400BadRequest)]
        static async Task<IResult> ConfirmEmail([FromServices]IUserService service, [FromQuery]string email, [FromQuery]string token)
        {
            await service.ConfirmEmail(email, token);

            return Results.Ok();
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        static async Task<IResult> UpdateUserAddress([FromServices]IUserService service, [FromBody]UpdateAddressRequest request)
        {
            var result = await service.UpdateUserAddress(request);

            return Results.Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Profile.Api.Binders;
using Profile.Api.Filters;
using Profile.Application.ApplicationServices;
using Profile.Domain.Exceptions;

namespace Profile.Api.Endpoints
{
    public static class ConnectionEndpoints
    {
        public const string MapGroup = $"{BaseEndpointConsts.rootEndpoint}connections";

        public static IEndpointRouteBuilder MapConnectionEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(MapGroup);

            app.MapPut("{profileId}", CreateConnectionWithProfile)
                .WithName("CreateConnectionWithProfile")
                .WithDescription("User create a connection with a profile by it id")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>()
                .RequireAuthorization("NormalUser");

            app.MapPut("{id}/accept", AcceptConnection)
                .WithName("AcceptConnectionRequest")
                .WithDescription("Accept a connection request by connection id")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>()
                .RequireAuthorization("NormalUser");

            app.MapPut("{id}/reject", AcceptConnection)
                .WithName("RejectConnectionRequest")
                .WithDescription("Reject a connection request by connection id")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>()
                .RequireAuthorization("NormalUser");

            return app;
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DomainException), StatusCodes.Status401Unauthorized)]
        static async Task<IResult> CreateConnectionWithProfile([FromServices]IConnectionService service, HttpContext context)
        {
            var profileId = await BinderIdValidatorExtension.Validate(context, "profileId");

            var result = await service.CreateConnection(profileId);

            return Results.Ok(result);
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DomainException), StatusCodes.Status401Unauthorized)]
        static async Task<IResult> AcceptConnection([FromServices]IConnectionService service, HttpContext context)
        {
            var id = await BinderIdValidatorExtension.Validate(context, "id");

            var result = await service.AcceptConnection(id);

            return Results.Accepted(string.Empty, result);
        }

        static async Task<IResult> RejectConnection([FromServices]IConnectionService service, HttpContext context)
        {
            var id = await BinderIdValidatorExtension.Validate(context, "id");

            await service.RejectConnection(id);

            return Results.NoContent();
        }
    }
}

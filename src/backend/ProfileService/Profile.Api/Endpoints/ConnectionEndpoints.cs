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

        public static IEndpointRouteBuilder AddConnectionEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(MapGroup);

            app.MapGet("create-connection/{profileId}", CreateConnectionWithProfile)
                .WithName("CreateConnectionWithProfile")
                .WithDescription("User create a connection with a profile by it id")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>();

            app.MapGet("{id}/accept", AcceptConnection)
                .WithName("AcceptConnectionRequest")
                .WithDescription("Accept a connection request by connection id")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>();

            return app;
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DomainException), StatusCodes.Status401Unauthorized)]
        static async Task<IResult> CreateConnectionWithProfile([FromRoute][ModelBinder(typeof(BinderId))]long profileId, [FromServices]IConnectionService service)
        {
            var result = await service.CreateConnection(profileId);

            return Results.Ok(result);
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DomainException), StatusCodes.Status401Unauthorized)]
        static async Task<IResult> AcceptConnection([FromRoute][ModelBinder(typeof(BinderId))]long id, [FromServices]IConnectionService service)
        {
            var result = await service.AcceptConnection(id);

            return Results.Accepted(string.Empty, result);
        }
    }
}

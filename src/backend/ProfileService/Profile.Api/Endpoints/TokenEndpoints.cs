using Microsoft.AspNetCore.Mvc;
using Profile.Api.Filters;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;

namespace Profile.Api.Endpoints
{
    public static class TokenEndpoints
    {
        public const string MapGroup = $"{BaseEndpointConsts.rootEndpoint}tokens";

        public static IEndpointRouteBuilder MapTokenEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(MapGroup);

            app.MapPost("refresh", RefreshToken)
                .WithName("GenerateNewAccessTokenWithRefresh")
                .WithSummary("Generate a new access token using user refresh token, access token on header authorization field must not be expired")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>();

            return app;
        }

        static async Task<IResult> RefreshToken([FromBody]RefreshTokenRequest request, [FromServices]ITokenAppService service)
        {
            var result = await service.RefreshToken(request);

            return Results.Ok(result);
        }
    }
}

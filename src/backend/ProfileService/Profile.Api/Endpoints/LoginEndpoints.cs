using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;

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

            return app;
        }

        static async Task<IResult> LoginByApplication([FromBody]LoginRequest request, [FromServices]ILoginService service)
        {
            var result = await service.LoginByApplication(request);

            return Results.Ok(result);
        }
    }
}

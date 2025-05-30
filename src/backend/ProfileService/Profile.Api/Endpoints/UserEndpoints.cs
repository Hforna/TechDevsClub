using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;

namespace Profile.Api.Endpoints
{
    public static class UserEndpoints
    {
        private const string UserGroup = "/api/users";

        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(UserGroup);

            app.MapPost("create", CreateUser)
                .WithName("CreateUser")
                .WithSummary("Create a user request and confirm e-mail after");

            return app;
        }


        static async Task<IResult> CreateUser([FromServices]IUserService service, [FromBody]CreateUserRequest request)
        {
            var result = await service.CreateUser(request);

            return Results.Created(string.Empty, result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Identity.Client;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;

namespace Profile.Api.Endpoints
{
    public static class ProfileEndpoints
    {
        const string ProfileGroup = $"{BaseEndpointConsts.rootEndpoint}profiles";

        public static IEndpointRouteBuilder MapProfileEndpoint(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(ProfileGroup);

            app.MapPut("update", UpdateProfile)
                .WithName("UpdateProfile")
                .WithSummary("Update an user profile and consult their github profile to get github infos for profile");

            return app;
        }

        static async Task<IResult> UpdateProfile([FromBody]UpdateProfileRequest request, [FromServices]IProfileService service)
        {
            var result = await service.UpdateProfile(request);

            return Results.Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Identity.Client;
using MimeKit.Tnef;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;
using Profile.Domain.Exceptions;

namespace Profile.Api.Endpoints
{
    public static class ProfileEndpoints
    {
        const string ProfileGroup = $"{BaseEndpointConsts.rootEndpoint}profiles";

        public static IEndpointRouteBuilder MapProfileEndpoint(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(ProfileGroup);

            app.MapPut("", UpdateProfile)
                .WithName("UpdateProfile")
                .WithSummary("Update an user profile and consult their github profile to get github infos for profile");

            app.MapGet("{name}", GetProfile)
                .WithName("GetProfileById")
                .WithSummary("Get a profile by id if it is not private")
                .RequireRateLimiting("PerfilPolicy");

            return app;
        }

        static async Task<IResult> UpdateProfile([FromBody]UpdateProfileRequest request, [FromServices]IProfileService service)
        {
            var result = await service.UpdateProfile(request);

            return Results.Ok(result);
        }

        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AuthenticationException), StatusCodes.Status401Unauthorized)]
        static async Task<IResult> GetProfile([FromRoute]string name, [FromServices]IProfileService service)
        {
            var result = await service.GetProfile(name);

            return Results.Ok(result);
        }

        static async Task<IResult> GetProfilesRecommendedByUserSkills([FromQuery]int page, [FromQuery]int perPage, [FromServices]IProfileService service)
        {
            if (perPage > 100) throw new ValidationException(ResourceExceptMessages.OUT_OF_RANGE_PER_PAGE_MAX_100, System.Net.HttpStatusCode.BadRequest);

            var result = service.
        }
    }
}

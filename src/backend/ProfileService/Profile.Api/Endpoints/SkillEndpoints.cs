using Microsoft.AspNetCore.Mvc;
using Profile.Api.Filters;
using Profile.Application.ApplicationServices;
using Profile.Application.Requests;
using Profile.Application.Services;
using Profile.Domain.Exceptions;

namespace Profile.Api.Endpoints
{
    public static class SkillEndpoints
    {
        public const string MapGroup = $"{BaseEndpointConsts.rootEndpoint}skills";

        public static IEndpointRouteBuilder MapSkillEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(MapGroup);

            app.MapPost("add-skills", SetUserSkills)
                .WithName("SetUserSkills")
                .WithDescription("Add user skills by the request list")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>()
                .RequireAuthorization("NormalUser");

            app.MapGet("catalog", GetSkills)
                .WithName("GetAllSkills")
                .WithDescription("Get all skills that user can set");

            app.MapDelete("", RemoveUserSkills)
                .WithName("RemoveUserSkills")
                .WithDescription("Remove skills that user has, by its names")
                .AddEndpointFilter<AuthenticationUserEndpointFilter>()
                .RequireAuthorization("NormalUser");

            return app;
        }


        [ProducesResponseType(typeof(ContextException), StatusCodes.Status404NotFound)]
        static async Task<IResult> SetUserSkills([FromBody] SetUserSkillsRequest request, [FromServices] IUserService service)
        {
            var result = await service.SetUserSkills(request);

            return Results.Ok(result);
        }

        static async Task<IResult> RemoveUserSkills([FromBody]RemoveUserSkillsRequest request, [FromServices]ISkillService service)
        {
            await service.RemoveUserSkills(request);

            return Results.NoContent();
        }

        static async Task<IResult> GetSkills([FromServices]ISkillService service)
        {
            var result = await service.GetSkills();

            if (result.Any() == false)
                return Results.NoContent();

            return Results.Ok(result);
        }
    }
}

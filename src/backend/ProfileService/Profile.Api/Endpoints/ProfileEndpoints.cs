using Microsoft.Identity.Client;

namespace Profile.Api.Endpoints
{
    public static class ProfileEndpoints
    {
        public const string ProfileGroup = $"{BaseEndpointConsts.rootEndpoint}profile";

        public static IEndpointRouteBuilder MapProfileEndpoint(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup()
        }
    }
}

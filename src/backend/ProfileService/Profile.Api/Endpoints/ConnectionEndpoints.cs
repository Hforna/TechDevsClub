namespace Profile.Api.Endpoints
{
    public static class ConnectionEndpoints
    {
        public const string MapGroup = $"{BaseEndpointConsts.rootEndpoint}connections";

        public static IEndpointRouteBuilder AddConnectionEndpoints(this IEndpointRouteBuilder builder)
        {
            var app = builder.MapGroup(MapGroup);



            return app;
        }
    }
}

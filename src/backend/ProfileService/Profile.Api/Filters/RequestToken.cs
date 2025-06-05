using Profile.Domain.Services.Security;

namespace Profile.Api.Filters
{
    public class RequestToken : IRequestToken
    {
        public IHttpContextAccessor? HttpAccessor { get; set; }

        public RequestToken(IHttpContextAccessor httpContext) => HttpAccessor = httpContext;

        public string GetToken()
        {
            var token = HttpAccessor!.HttpContext!.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(token))
                return string.Empty;

            return token["Bearer ".Length..].Trim();
        }
    }
}

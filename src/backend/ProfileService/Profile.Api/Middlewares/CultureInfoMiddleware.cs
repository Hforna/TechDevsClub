
using System.Globalization;

namespace Profile.Api.Middlewares
{
    public class CultureInfoMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestLanguage = context.Request.Headers.AcceptLanguage.ToString();
            var acceptCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var currentCulture = new CultureInfo("en-us");

            if(string.IsNullOrEmpty(requestLanguage) == false && acceptCultures.Any(d => d.Equals(requestLanguage)))
            {
                currentCulture = new CultureInfo(requestLanguage);
            }
            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentCulture;

            await next(context);
        }
    }
}

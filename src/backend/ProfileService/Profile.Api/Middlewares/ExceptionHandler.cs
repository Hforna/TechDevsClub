using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Profile.Domain.Exceptions;

namespace Profile.Api.Middlewares
{
    public class ExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = exception is BaseException bse
                ? (int)bse.GetStatusCode()
                : (int)System.Net.HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                Message = exception is BaseException d
                ? d.GetMessage()
                : new List<string>() { exception.Message }
            });

            return true;
        }
    }
}

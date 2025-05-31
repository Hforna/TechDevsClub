using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Profile.Domain.Exceptions;

namespace Profile.Api.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if(context.Exception is BaseException expb)
            {
                context.Result = new BadRequestObjectResult(new JsonErrorResponse(expb.GetMessage(), expb.GetStatusCode()));
                context.HttpContext.Response.StatusCode = (int)expb.GetStatusCode();
            }
        }
    }
}

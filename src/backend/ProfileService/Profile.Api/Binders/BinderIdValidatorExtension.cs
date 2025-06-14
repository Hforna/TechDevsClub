using Microsoft.AspNetCore.Mvc.ModelBinding;
using Profile.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Profile.Api.Binders
{
    public static class BinderIdValidatorExtension
    {
        public static async Task<long> Validate(HttpContext context, string Idname)
        {
            var binder = context.RequestServices.GetRequiredService<BinderId>();
            var bindingContext = new DefaultModelBindingContext
            {
                ModelName = Idname,
                ValueProvider = new RouteValueProvider(BindingSource.Path, context.Request.RouteValues),
                ModelState = new ModelStateDictionary()
            };

            await binder.BindModelAsync(bindingContext);

            if (!bindingContext.Result.IsModelSet)
                throw new Domain.Exceptions.ValidationException(ResourceExceptMessages.INVALID_ID_FORMAT, System.Net.HttpStatusCode.BadRequest);

            return (long)bindingContext.Result.Model!;
        }
    }
}

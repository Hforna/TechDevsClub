using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Commons
{
    public static class RequestValidatorCommons
    {
        public static void Validate<Request, Validator>(Request request) where Validator : AbstractValidator<Request>
        {
            var validator = Activator.CreateInstance<Validator>();
            var validate = validator.Validate(request);

            if(!validate.IsValid)
            {
                var errorMessages = validate.Errors.Select(d => d.ErrorMessage).ToList();
                throw new Profile.Domain.Exceptions.ValidationException(errorMessages, HttpStatusCode.BadRequest);
            }
        }
    }
}

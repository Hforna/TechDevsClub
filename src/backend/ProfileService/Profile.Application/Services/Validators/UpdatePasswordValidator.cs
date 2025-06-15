using FluentValidation;
using Profile.Application.Requests;
using Profile.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Services.Validators
{
    public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordRequest>
    {
        public UpdatePasswordValidator()
        {
            RuleFor(d => d.NewPassword.Length).GreaterThanOrEqualTo(8).WithMessage(ResourceExceptMessages.PASSWORD_LENGTH);
            RuleFor(d => d.RepeatPassword).Equal(d => d.NewPassword);
        }
    }
}

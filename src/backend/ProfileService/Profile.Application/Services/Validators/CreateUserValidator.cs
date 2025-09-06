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
    public class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(d => d.Email).EmailAddress().WithMessage(ResourceExceptMessages.EMAIL_FORMAT);
            RuleFor(d => d.Password.Length).GreaterThanOrEqualTo(8).WithMessage(ResourceExceptMessages.PASSWORD_LENGTH);
            RuleFor(d => d.UserName).Must(d => d.Contains(" ") == false).WithMessage(ResourceExceptMessages.USERNAME_NOT_SPACES);
        }
    }
}

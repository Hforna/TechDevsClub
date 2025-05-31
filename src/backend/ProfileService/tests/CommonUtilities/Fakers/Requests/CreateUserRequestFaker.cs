using Bogus;
using Profile.Application.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilities.Fakers.Requests
{
    public static class CreateUserRequestFaker
    {
        public static CreateUserRequest Build()
        {
            return new Faker<CreateUserRequest>()
                .RuleFor(d => d.UserName, f => f.Name.FindName())
                .RuleFor(d => d.Email, f => f.Internet.Email())
                .RuleFor(d => d.Password, f => f.Internet.Password(10));
        }
    }
}

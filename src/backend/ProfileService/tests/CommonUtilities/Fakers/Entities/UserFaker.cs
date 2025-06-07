using Bogus;
using Profile.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilities.Fakers.Entities
{
    public static class UserFaker
    {
        public static User Build(bool active = true, bool emailConfirmed = true)
        {
            return new Faker<User>()
                .RuleFor(d => d.UserName, f => f.Name.FindName())
                .RuleFor(d => d.Email, f => f.Internet.Email())
                .RuleFor(d => d.Active, active)
                .RuleFor(d => d.EmailConfirmed, emailConfirmed)
                .RuleFor(d => d.PasswordHash, f => f.Internet.Password(1));
        }
    }
}

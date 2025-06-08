using Bogus;
using Profile.Application.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilities.Fakers.Requests
{
    public static class UpdateAddressRequestFaker
    {
        public static UpdateAddressRequest Build()
        {
            return new Faker<UpdateAddressRequest>()
                .RuleFor(d => d.ZipCode, f => f.Address.ZipCode())
                .RuleFor(d => d.City, f => f.Address.City())
                .RuleFor(d => d.Country, f => f.Address.Country())
                .RuleFor(d => d.Street, f => f.Address.StreetAddress())
                .RuleFor(d => d.State, f => f.Address.State());
        }
    }
}

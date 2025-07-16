using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Career.Domain.ValueObjects
{
    public sealed record Location
    {
        public Location(string country, string state, string? zipCode)
        {
            Country = country;
            State = state;
            ZipCode = zipCode;
        }

        public string Country { get; private set; }
        public string State { get; private set; }
        public string? ZipCode { get; private set; }
    }
}

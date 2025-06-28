using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string Country { get; set; }
        public string State { get; set; }
        public string? ZipCode { get; set; }
    }
}

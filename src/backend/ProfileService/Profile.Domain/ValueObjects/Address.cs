﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.ValueObjects
{
    [Owned]
    public sealed record Address
    {
        public Address(string country, string street, string city, string state, string zipCode)
        {
            Country = country;
            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
        }

        public string Country { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.ValueObjects
{
    [Owned]
    public sealed record DeviceLocation
    {
        public DeviceLocation(string country, string city, decimal latitude, decimal longitude)
        {
            Country = country;
            City = city;
            Latitude = latitude;
            Longitude = longitude;
        }

        public string Country { get; set; }
        public string City { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}

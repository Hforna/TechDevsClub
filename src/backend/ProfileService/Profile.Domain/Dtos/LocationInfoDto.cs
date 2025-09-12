using Profile.Domain.Services.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Dtos
{
    public sealed record LocationInfoDto
    {
        public CountryDto Country { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }


    public sealed record CountryDto
    {
        public string IsoCode { get; set; }
        public string Name { get; set; }
    }
}

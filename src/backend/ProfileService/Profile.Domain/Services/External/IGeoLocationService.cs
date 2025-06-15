using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Services.External
{
    public interface IGeoLocationService
    {
        public LocationInfoDto GetLocationInfosByIp(string ip);
    }

    public class LocationInfoDto
    {
        public CountryDto Country { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class CountryDto
    {
        public string IsoCode { get; set; }
        public string Name { get; set; }
    }
}

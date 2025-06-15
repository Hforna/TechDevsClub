using MaxMind.GeoIP2;
using Microsoft.Extensions.Options;
using Profile.Domain.Exceptions;
using Profile.Domain.Services.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services.External
{
    public class MaxMindGeoIpAdapter : IGeoLocationService
    {
        private readonly DatabaseReader _reader;

        public MaxMindGeoIpAdapter(string dbPath)
        {
            _reader = new DatabaseReader(
                file: dbPath, 
                mode: MaxMind.Db.FileAccessMode.MemoryMapped);
        }

        public LocationInfoDto GetLocationInfosByIp(string ip)
        {
            if(_reader.TryCity(ip, out var infos))
            {
                return new LocationInfoDto()
                {
                    City = infos.City.Name,
                    Latitude = (decimal)infos.Location.Latitude,
                    Longitude = (decimal)infos.Location.Longitude,
                    PostalCode = infos.Postal.Code,
                    Country = new CountryDto()
                    {
                        IsoCode = infos.Country.IsoCode,
                        Name = infos.Country.Name
                    }
                };
            }
            throw new ExternalServiceException(ResourceExceptMessages.ERROR_ON_LOCATION, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public class GeoIPSettings
    {
        public string DatabasePath { get; set; }
    }
}

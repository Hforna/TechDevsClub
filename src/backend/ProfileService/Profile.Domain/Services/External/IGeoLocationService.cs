using Profile.Domain.Dtos;
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
}

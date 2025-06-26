using Profile.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Services
{
    public interface IRequestService
    {
        public string GetRequestIp();
        public DeviceDto? GetDeviceInfos();
        public string? GetAccessToken();
    }
}

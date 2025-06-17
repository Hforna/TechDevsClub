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
    }

    public sealed record DeviceDto
    {
        public DeviceDto(string name, string model, string osType, string type)
        {
            Name = name;
            Model = model;
            OsType = osType;
            Type = type;
        }

        public string Name { get; set; }
        public string Model { get; set; }
        public string OsType { get; set; }
        public string Type { get; set; }
    }
}

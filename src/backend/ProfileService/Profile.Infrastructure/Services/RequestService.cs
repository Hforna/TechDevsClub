using Microsoft.AspNetCore.Http;
using Profile.Domain.Dtos;
using Profile.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace Profile.Infrastructure.Services
{
    public class RequestService : IRequestService
    {
        private readonly IHttpContextAccessor _httpContext;

        public RequestService(IHttpContextAccessor httpContext) => _httpContext = httpContext;

        public string GetRequestIp()
        {
            //If application is running in containers on development
            return "187.7.70.182";

            var ipAddress = _httpContext.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if(string.IsNullOrEmpty(ipAddress))
                return _httpContext.HttpContext.Connection.RemoteIpAddress!
                    .MapToIPv4()
                    .ToString();

            return ipAddress.Split(",")[0].Trim();
        }

        public DeviceDto? GetDeviceInfos()
        {
            var userAgent = _httpContext.HttpContext.Request.Headers.UserAgent.ToString();

            if (string.IsNullOrEmpty(userAgent))
                return null;

            var parser = Parser.GetDefault(); 

            var parse = parser.Parse(userAgent);

            return new DeviceDto(parse.Device.Brand, parse.Device.Model, parse.OS.Family, parse.Device.Family);
        }
    }
}

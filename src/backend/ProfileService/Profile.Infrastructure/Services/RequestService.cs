using Microsoft.AspNetCore.Http;
using Profile.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Services
{
    public class RequestService : IRequestService
    {
        private readonly IHttpContextAccessor _httpContext;

        public RequestService(IHttpContextAccessor httpContext) => _httpContext = httpContext;

        public string? GetRequestIp()
        {
            var ipAddress = _httpContext.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if(string.IsNullOrEmpty(ipAddress))
                return _httpContext.HttpContext.Connection.RemoteIpAddress!
                    .MapToIPv4()
                    .ToString();

            return ipAddress.Split(",")[0].Trim();
        }
    }
}

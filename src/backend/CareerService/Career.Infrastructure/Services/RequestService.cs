using Career.Domain.Exceptions;
using Career.Domain.Services.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Services
{
    public class RequestService : IRequestService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<RequestService> _logger;

        public RequestService(IHttpContextAccessor httpContext, ILogger<RequestService> logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        public string? GetBearerToken()
        {
            var token = _httpContext.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(token))
                return null;

            return token.ToString()["Bearer ".Length..].Trim();
        }
    }
}

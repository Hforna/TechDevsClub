
using Profile.Domain.Aggregates;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.Security;

namespace Profile.Api.Middlewares
{
    public class DeviceTrackerMiddleware : IMiddleware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeviceTrackerMiddleware> _logger;

        public DeviceTrackerMiddleware(IServiceProvider serviceProvider, ILogger<DeviceTrackerMiddleware> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var token = context.Request.Headers.Authorization.ToString();
                if(!string.IsNullOrEmpty(token))
                {
                    token = token["Bearer ".Length..].Trim();
                    var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                    var uof = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var deviceId = tokenService.GetDeviceId(token);
                    var device = await uof.GenericRepository.GetById<Device>(deviceId);
                    if(device is null)
                    {
                        _logger.LogWarning($"Device is null, id: {deviceId}");
                        await next(context);
                    }
                    device.LastAccess = DateTime.UtcNow;
                    uof.GenericRepository.Update<Device>(device);
                    await uof.Commit();
                }
            }

            await next(context);
        }
    }
}

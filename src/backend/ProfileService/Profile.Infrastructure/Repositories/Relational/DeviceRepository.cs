using Microsoft.EntityFrameworkCore;
using Profile.Domain.Aggregates;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories.Relational
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly DataContext _context;

        public DeviceRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Device?> DeviceByUserIdAndIp(long userId, string ip)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.UserId == userId && d.Ip == ip);
        }

        public async Task<List<Device>?> DevicesByUserIdentifier(Guid userIdentifier)
        {
            return await _context.Devices.Where(d => d.User.Active && d.User.UserIdentifier == userIdentifier).ToListAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenByDeviceAndUser(long userId, long deviceId)
        {
            return await _context.RefreshTokens.SingleOrDefaultAsync(d => d.UserId == userId 
            && d.DeviceId == deviceId 
            && DateTime.UtcNow < d.RefreshTokenExpiration);
        }

    }
}

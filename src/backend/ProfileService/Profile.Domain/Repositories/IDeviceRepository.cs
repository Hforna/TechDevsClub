﻿using Profile.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface IDeviceRepository
    {
        public Task<List<Device>?> DevicesByUserIdentifier(Guid userIdentifier);
        public Task<Device?> DeviceByUserIdAndIp(long userId, string ip);
        public Task<RefreshToken?> GetRefreshTokenByDeviceAndUser(long userId, long deviceId);
    }
}

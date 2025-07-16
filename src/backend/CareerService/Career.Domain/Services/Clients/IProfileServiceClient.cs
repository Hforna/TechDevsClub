using Career.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services.Clients
{
    public interface IProfileServiceClient
    {
        public Task<UserInfosDto> GetUserInfos(string accessToken);
        public Task<UserRolesDto> GetUserRoles(string accessToken);
        public Task<UserInfosWithRolesDto> GetUserInfosById(string userId);
    }
}

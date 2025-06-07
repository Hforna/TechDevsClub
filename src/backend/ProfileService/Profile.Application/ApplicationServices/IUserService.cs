using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface IUserService
    {
        public Task<UserResponse> CreateUser(CreateUserRequest request);
        public Task ConfirmEmail(string email, string token);
        public Task<Address> CreateUserAddress(UpdateAddressRequest request);
    }
}

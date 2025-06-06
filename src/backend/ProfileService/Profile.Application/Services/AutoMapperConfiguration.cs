using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.ValueObjects;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Services
{
    public class AutoMapperConfiguration : AutoMapper.Profile
    {

        public AutoMapperConfiguration(SqidsEncoder<long> sqids)
        {
            CreateMap<CreateUserRequest, User>()
                .ForMember(d => d.PasswordHash, f => f.Ignore());

            CreateMap<User, UserResponse>()
                .ForMember(d => d.Id, f => f.MapFrom(d => sqids.Encode(d.Id)));

            CreateMap<UpdateAddressRequest, Address>();
        }
    }
}

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

            CreateMap<ProfileEntity, ProfileResponse>()
                .ForMember(d => d.UserId, f => f.MapFrom(d => sqids.Encode(d.UserId)))
                .ForMember(d => d.Id, f => f.MapFrom(d => sqids.Encode(d.Id)))
                .ForMember(d => d.SocialLinks, f => f.MapFrom(d => d.SocialLinks));

            CreateMap<SocialLink, SocialLinksResponse>();

            CreateMap<SocialLinkRequest, SocialLink>();

            CreateMap<UpdateProfileRequest, ProfileEntity>()
                .ForMember(dest => dest.SocialLinks, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var linkRequest in src.SocialLinks)
                    {
                        dest.SocialLinks = new List<SocialLink>();

                        dest.SocialLinks.Add(new SocialLink
                        {
                            Name = linkRequest.Name,
                            Link = linkRequest.Link
                        });
                    }
                });
        }
    }
}

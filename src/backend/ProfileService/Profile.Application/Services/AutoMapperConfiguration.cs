using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Dtos;
using Profile.Domain.Entities;
using Profile.Domain.Services;
using Profile.Domain.Services.External;
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
                .ForMember(d => d.UserSkills, f => f.Ignore())
                .ForMember(d => d.GithubMetadata, f => f.MapFrom(d => d.GithubMeta))
                .ForMember(d => d.SocialLinks, f => f.MapFrom(d => d.SocialLinks));

            CreateMap<Skill, SkillResponse>();

            CreateMap<SocialLink, SocialLinksResponse>();

            CreateMap<SocialLinkRequest, SocialLink>();

            CreateMap<ProfileEntity, ShortProfileResponse>()
                .ForMember(d => d.Id, f => f.MapFrom(d => sqids.Encode(d.Id)));

            CreateMap<LocationInfoDto, DeviceLocation>()
                .ForMember(d => d.Country, f => f.MapFrom(d => d.Country.Name))
                .ForMember(d => d.City, f => f.MapFrom(d => d.City));

            CreateMap<DeviceDto, Device>()
                .ForMember(d => d.Location, f => f.Ignore());

            CreateMap<UserSkills, SkillUserResponse>()
                .ForMember(d => d.Name, f => f.MapFrom(d => d.Skill.Name))
                .ForMember(d => d.Level, f => f.MapFrom(d => d.Level));

            CreateMap<Connection, ConnectionResponse>()
                .ForMember(d => d.ConnectorId, f => f.MapFrom(d => d.ConnectorId))
                .ForMember(d => d.ConnectedId, f => f.MapFrom(d => d.ConnectedId))
                .ForMember(d => d.Id, f => f.MapFrom(d => d.Id));

            CreateMap<GithubMetadata, GithubMetadataResponse>();

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

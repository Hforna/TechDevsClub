﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.External;
using Profile.Domain.Services.Security;
using Profile.Domain.ValueObjects;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface IProfileService
    {
        public Task<ProfileResponse> UpdateProfile(UpdateProfileRequest request);
        public Task<ProfileResponse> GetProfile(long id);
        public Task<PaginationProfilesResponse?> GetProfileRecommendedByProfileVisits(int page, int perPage);
    }

    public class ProfileService : IProfileService
    {
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly SqidsEncoder<long> _sqids;
        private readonly IGitHubService _gitHubService;
        private readonly ILogger<ProfileService> _logger;
        private readonly Guid _userUid;
        private readonly ISessionService _sessionService;
        private readonly IRequestService _requestService;

        public ProfileService(ITokenService tokenService, 
            IUnitOfWork uof, IMapper mapper,
            IGitHubService gitHubService, SqidsEncoder<long> sqids,
            ILogger<ProfileService> logger, ISessionService sessionService, IRequestService requestService)
        {
            _tokenService = tokenService;
            _requestService = requestService;
            _uof = uof;
            _logger = logger;
            _mapper = mapper;
            _sqids = sqids;
            _gitHubService = gitHubService;
            _sessionService = sessionService;
        }

        public async Task<ProfileResponse> GetProfile(long id)
        {
            var profile = await _uof.ProfileRepository.ProfileById(id, true);

            if (profile is null)
            {
                _logger.LogError($"Profile doesn't exists: profile id: {profile.Id}, is user active: {profile.User.Active}");
                throw new ContextException(ResourceExceptMessages.PROFILE_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);
            }
            var token = _requestService.GetAccessToken();
            User? user = null;

            if(!string.IsNullOrEmpty(token))
            {
                user = await _tokenService.GetUserByToken();
            }

            if (profile.IsPrivate && 
                ((user is null || profile.User.Id != user.Id))) 
                throw new DomainException(ResourceExceptMessages.PROFILE_PRIVATE, System.Net.HttpStatusCode.Unauthorized);

            var response = _mapper.Map<ProfileResponse>(profile);
            if(user is not null && user.Skills is not null)
                response.UserSkills = new UserSkillsResponse() { Skills = _mapper.Map<List<SkillUserResponse>>(user.Skills.ToList()) };

            var userGot = await _uof.UserRepository.UserByIdentifier(profile.User.UserIdentifier);

            if(userGot.Skills.Any())
                _sessionService.SetProfileVisitedByUser(profile.Id, userGot.Skills.Select(d => d.Skill).ToList());

            return response;
        }

        public async Task<PaginationProfilesResponse?> GetProfileRecommendedByProfileVisits(int page, int perPage)
        {
            if (perPage > 100) throw new ValidationException(ResourceExceptMessages.OUT_OF_RANGE_PER_PAGE_MAX_100, System.Net.HttpStatusCode.BadRequest);

            var profilesVisited = _sessionService.GetProfilesVisitedByUser();

            if (profilesVisited is null)
                return null;

            var skillsList = profilesVisited
                .Select(d => d.Value)
                .SelectMany(d => d)
                .ToList();
            var skillsCount = skillsList
                .GroupBy(d => d)
                .ToDictionary(d => d, f => f.Count())
                .OrderByDescending(d => d.Value)
                .Select(d => d.Key)
                .Select(d => d.Key)
                .ToList();

            var profiles = await _uof.ProfileRepository.ProfilesBySkills(skillsCount, page, perPage);

            var response = new PaginationProfilesResponse()
            {
                HasPreviousPage = profiles.HasPreviousPage,
                HasNextPage = profiles.HasNextPage,
                Count = profiles.Count,
                IsFirstPage = profiles.IsFirstPage,
                IsLastPage = profiles.IsLastPage,
                PageNumber = profiles.PageNumber
            };
            response.Profiles = profiles.Select(profile => _mapper.Map<ShortProfileResponse>(profile)).ToList();

            return response;
        }

        public async Task<ProfileResponse> UpdateProfile(UpdateProfileRequest request)
        {
            var user = await _tokenService.GetUserByToken();
            var profile = await _uof.ProfileRepository.ProfileByUser(user);
            _mapper.Map(request, profile);

            try
            {
                var gitHubRepositories = await _gitHubService.GetPublicProfileRepositories(request.GitHubUserName);
                profile.GithubMeta = new GithubMetadata(request.GitHubUserName, gitHubRepositories, 0);

            } catch(ExternalServiceException ee) when(ee.GetStatusCode() == System.Net.HttpStatusCode.NotFound)
            {
                throw new ExternalServiceException(ResourceExceptMessages.INVALID_GITHUB_PROFILE, System.Net.HttpStatusCode.NotFound);
            }catch(ExternalServiceException ee)
            {
                throw new ExternalServiceException(ResourceExceptMessages.REQUEST_GITHUB_ERROR, ee.GetStatusCode());
            }

            _uof.GenericRepository.Update<ProfileEntity>(profile);
            await _uof.Commit();

            var skillsList = user.Skills.ToList();

            var response = _mapper.Map<ProfileResponse>(profile);
            response.UserSkills = new UserSkillsResponse() { Skills = _mapper.Map<List<SkillUserResponse>>(skillsList) };

            return response;
        }
    }
}

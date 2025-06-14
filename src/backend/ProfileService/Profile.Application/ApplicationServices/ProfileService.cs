using AutoMapper;
using Microsoft.Extensions.Logging;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
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
        private readonly IRequestToken _requestToken;
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly SqidsEncoder<long> _sqids;
        private readonly IGitHubService _gitHubService;
        private readonly ILogger<ProfileService> _logger;
        private readonly Guid _userUid;
        private readonly ISessionService _sessionService;

        public ProfileService(ITokenService tokenService, IRequestToken requestToken, 
            IUnitOfWork uof, IMapper mapper,
            IGitHubService gitHubService, SqidsEncoder<long> sqids,
            ILogger<ProfileService> logger, ISessionService sessionService)
        {
            _tokenService = tokenService;
            _requestToken = requestToken;
            _uof = uof;
            _logger = logger;
            _mapper = mapper;
            _sqids = sqids;
            _gitHubService = gitHubService;
            _sessionService = sessionService;

            _userUid = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());
        }

        public async Task<ProfileResponse> GetProfile(long id)
        {
            var profile = await _uof.ProfileRepository.ProfileById(id, true);

            if (profile is null)
            {
                _logger.LogError($"Profile doesn't exists: profile id: {profile.Id}, is user active: {profile.User.Active}");
                throw new ContextException(ResourceExceptMessages.PROFILE_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);
            }
            var token = _requestToken.GetToken();
            User? user = null;

            if(!string.IsNullOrEmpty(token))
            {
                var userUid = _tokenService.GetUserIdentifierByToken(token);
                user = await _uof.UserRepository.UserByIdentifier(userUid);
            }

            if (profile.IsPrivate && 
                (user is not null && profile.User.Id != user.Id)) 
                throw new DomainException(ResourceExceptMessages.PROFILE_PRIVATE, System.Net.HttpStatusCode.Unauthorized);

            var response = _mapper.Map<ProfileResponse>(profile);
            if(user is not null && user.Skills is not null)
                response.UserSkills = new UserSkillsResponse() { Skills = _mapper.Map<List<SkillUserResponse>>(user.Skills.ToList()) };

            return response;
        }

        public async Task<PaginationProfilesResponse?> GetProfileRecommendedByProfileVisits(int page, int perPage)
        {
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
            var userUid = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());

            var user = await _uof.UserRepository.UserByIdentifier(userUid);
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

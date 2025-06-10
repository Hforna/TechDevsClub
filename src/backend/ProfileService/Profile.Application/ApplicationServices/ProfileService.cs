using AutoMapper;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services.External;
using Profile.Domain.Services.Security;
using Profile.Domain.ValueObjects;
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
    }

    public class ProfileService : IProfileService
    {
        private readonly ITokenService _tokenService;
        private readonly IRequestToken _requestToken;
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly IGitHubService _gitHubService;

        public ProfileService(ITokenService tokenService, IRequestToken requestToken, 
            IUnitOfWork uof, IMapper mapper, IGitHubService gitHubService)
        {
            _tokenService = tokenService;
            _requestToken = requestToken;
            _uof = uof;
            _mapper = mapper;
            _gitHubService = gitHubService;
        }

        public async Task<ProfileResponse> UpdateProfile(UpdateProfileRequest request)
        {
            var userUid = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());

            var user = await _uof.UserRepository.UserByIdentifier(userUid);

            var profile = _mapper.Map<ProfileEntity>(request);

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

            await _uof.GenericRepository.Add<ProfileEntity>(profile);
            await _uof.Commit();

            var response = _mapper.Map<ProfileResponse>(profile);

            return response;
        }
    }
}

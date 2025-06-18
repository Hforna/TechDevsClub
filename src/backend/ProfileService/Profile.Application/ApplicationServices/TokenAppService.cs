using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface ITokenAppService
    {
        public Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
    }

    public class TokenAppService : ITokenAppService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IRequestToken _requestToken;
        private readonly IUnitOfWork _uof;
        private readonly IRequestService _requestService;
        private readonly ILogger<TokenAppService> _logger;

        public TokenAppService(ITokenService tokenService, UserManager<User> userManager, 
            IRequestToken requestToken, IUnitOfWork uof, 
            IRequestService requestService, ILogger<TokenAppService> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _requestToken = requestToken;
            _uof = uof;
            _requestService = requestService;
            _logger = logger;
        }

        public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
        {
            var userUid = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());
            var user = await _uof.UserRepository.UserByIdentifier(userUid);

            var requestIp = _requestService.GetRequestIp();

            var device = await _uof.DeviceRepository.DeviceByUserIdAndIp(user.Id, requestIp);

            if (device is null)
                throw new ContextException(ResourceExceptMessages.DEVICE_NOT_AUTHENTICATED, System.Net.HttpStatusCode.Unauthorized);

            var userRefreshToken = await _uof.DeviceRepository.GetRefreshTokenByDeviceAndUser(user.Id, device.Id);

            if (userRefreshToken is null || request.RefreshToken != userRefreshToken.Value)
                throw new ContextException(ResourceExceptMessages.REFRESH_TOKEN_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            userRefreshToken.Value = _tokenService.GenerateRefreshToken();
            userRefreshToken.RefreshTokenExpiration = _tokenService.GetRefreshTokenExpiration();

            _uof.GenericRepository.Update<RefreshToken>(userRefreshToken);
            await _uof.Commit();

            _logger.LogInformation("New refresh token generated for user: {user.Id} for device: {device.Id}", user.Id, device.Id);

            var claims = _tokenService.GetTokenClaims(_requestToken.GetToken());

            var accessToken = _tokenService.GenerateToken(claims, userUid);

            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = userRefreshToken.Value
            };
        }
    }
}

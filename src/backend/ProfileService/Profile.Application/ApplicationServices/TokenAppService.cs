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
        private readonly IUnitOfWork _uof;
        private readonly IRequestService _requestService;
        private readonly ILogger<TokenAppService> _logger;

        public TokenAppService(ITokenService tokenService, UserManager<User> userManager, 
            IUnitOfWork uof, 
            IRequestService requestService, ILogger<TokenAppService> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _uof = uof;
            _requestService = requestService;
            _logger = logger;
        }

        public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
        {
            var user = await _tokenService.GetUserByToken();

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

            var requestToken = _requestService.GetAccessToken();

            var claims = _tokenService.GetTokenClaims(requestToken!);

            var accessToken = _tokenService.GenerateToken(claims, user.UserIdentifier);

            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = userRefreshToken.Value
            };
        }
    }
}

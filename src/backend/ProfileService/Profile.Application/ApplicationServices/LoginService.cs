using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.External;
using Profile.Domain.Services.Security;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface ILoginService
    {
        public Task<LoginResponse> LoginByApplication(LoginRequest request);
        public Task<LoginResponse> AuthenticateUserByOAuth(string email, List<Claim> providerClaims);
    }

    public class LoginService : ILoginService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _uof;
        private readonly IPasswordEncrypt _passwordEncrypt;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly IGeoLocationService _geoLocation;

        public LoginService(ITokenService tokenService, UserManager<User> userManager, IUnitOfWork uof, 
            IPasswordEncrypt passwordEncrypt, IRequestService requestService, 
            IMapper mapper, IGeoLocationService geoLocation)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _uof = uof;
            _passwordEncrypt = passwordEncrypt;
            _requestService = requestService;
            _mapper = mapper;
            _geoLocation = geoLocation;
        }

        public async Task<LoginResponse> AuthenticateUserByOAuth(string email, List<Claim> providerClaims)
        {
            var user = await _uof.UserRepository.UserByEmail(email);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_DOESNT_EXISTS, HttpStatusCode.NotFound);

            var ip = _requestService.GetRequestIp();

            var device = await _uof.DeviceRepository.DeviceByUserIdAndIp(user.Id, ip);

            if (device is null)
            {
                var deviceInfos = _requestService.GetDeviceInfos();
                var deviceLocation = _geoLocation.GetLocationInfosByIp(ip);

                device = _mapper.Map<Device>(deviceInfos);
                device.Ip = ip;
                device.Location = _mapper.Map<DeviceLocation>(deviceLocation);
                device.Location.Country = deviceLocation.Country.Name;
                await _uof.GenericRepository.Add<Device>(device);
            } else
            {
                var deviceRefreshToken = await _uof.DeviceRepository.GetRefreshTokenByDeviceAndUser(user.Id, device.Id);

                if (deviceRefreshToken is not null)
                {
                    _uof.GenericRepository.Delete<RefreshToken>(deviceRefreshToken);
                }
            }
            device.LastAccess = DateTime.UtcNow;
            await _uof.Commit();

            var refreshToken = new RefreshToken()
            {
                DeviceId = device.Id,
                UserId = user.Id,
                Value = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiration = _tokenService.GetRefreshTokenExpiration()
            };
            await _uof.GenericRepository.Add<RefreshToken>(refreshToken);

            await _uof.Commit();

            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = providerClaims;

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim("device-id", device.Id.ToString()));

            var accessToken = _tokenService.GenerateToken(claims, user.UserIdentifier);
            var accessExpiresAt = _tokenService.GenerateTimeForAccessTokenExpires();

            return new LoginResponse()
            {
                AccessToken = accessToken,
                ExpiresAt = accessExpiresAt,
                RefreshToken = refreshToken.Value,
                RefreshExpiresAt = refreshToken.RefreshTokenExpiration
            };
        }

        public async Task<LoginResponse> LoginByApplication(LoginRequest request)
        {
            var user = await _uof.UserRepository.UserByEmail(request.Email);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.EMAIL_OR_PASSWORD_INVALID, System.Net.HttpStatusCode.BadRequest);

            var isValidPassword = _passwordEncrypt.IsValidPassword(request.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new ContextException(ResourceExceptMessages.EMAIL_OR_PASSWORD_INVALID, System.Net.HttpStatusCode.BadRequest);

            await _userManager.UpdateAsync(user);

            var ip = _requestService.GetRequestIp();
            var location = _geoLocation.GetLocationInfosByIp(ip);

            var device = await _uof.DeviceRepository.DeviceByUserIdAndIp(user.Id, ip);

            if(device is null)
            {
                var deviceInfos = _requestService.GetDeviceInfos();
                device = _mapper.Map<Device>(deviceInfos);
                device.UserId = user.Id;
                device.Ip = ip;
                device.Location = _mapper.Map<DeviceLocation>(location);
                device.Location.Country = location.Country.Name;
                await _uof.GenericRepository.Add<Device>(device);

                await _uof.Commit();
            } else
            {
                device.Location = _mapper.Map<DeviceLocation>(location);
                device.LastAccess = DateTime.UtcNow;
                _uof.GenericRepository.Update<Device>(device);
            }

            var refreshToken = new RefreshToken()
            {
                DeviceId = device.Id,
                UserId = user.Id,
                Value = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiration = _tokenService.GetRefreshTokenExpiration()
            };
            await _uof.GenericRepository.Add<RefreshToken>(refreshToken);

            await _uof.Commit();

            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>();

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim("device-id", device.Id.ToString()));

            var accessToken = _tokenService.GenerateToken(claims, user.UserIdentifier);
            var accessExpiresAt = _tokenService.GenerateTimeForAccessTokenExpires();

            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Value,
                RefreshExpiresAt = refreshToken.RefreshTokenExpiration,
                ExpiresAt = accessExpiresAt
            };
        }
    }
}

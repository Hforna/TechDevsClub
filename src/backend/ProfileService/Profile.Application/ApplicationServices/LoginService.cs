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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface ILoginService
    {
        public Task<LoginResponse> LoginByApplication(LoginRequest request);
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

        public async Task<LoginResponse> LoginByApplication(LoginRequest request)
        {
            var user = await _uof.UserRepository.UserByEmail(request.Email);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.EMAIL_OR_PASSWORD_INVALID, System.Net.HttpStatusCode.BadRequest);

            var isValidPassword = _passwordEncrypt.IsValidPassword(request.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new ContextException(ResourceExceptMessages.EMAIL_OR_PASSWORD_INVALID, System.Net.HttpStatusCode.BadRequest);

            user.RefreshTokenExpiration = _tokenService.GetRefreshTokenExpiration();
            user.RefreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.UpdateAsync(user);

            var location = _geoLocation.GetLocationInfosByIp(_requestService.GetRequestIp()!);

            var deviceInfos = _requestService.GetDeviceInfos();
            var device = _mapper.Map<Device>(deviceInfos);
            device.Location = _mapper.Map<DeviceLocation>(location);

            await _uof.GenericRepository.Add<Device>(device);
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

            return new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken
            };
        }
    }
}

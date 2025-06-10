using Microsoft.AspNetCore.Identity;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services.Security;
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

        public LoginService(ITokenService tokenService, UserManager<User> userManager, 
            IUnitOfWork uof, IPasswordEncrypt passwordEncrypt)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _uof = uof;
            _passwordEncrypt = passwordEncrypt;
        }

        public async Task<LoginResponse> LoginByApplication(LoginRequest request)
        {
            var user = await _uof.UserRepository.UserByEmail(request.Email);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.EMAIL_OR_PASSWORD_INVALID, System.Net.HttpStatusCode.BadRequest);

            var isValidPassword = _passwordEncrypt.IsValidPassword(request.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new ContextException(ResourceExceptMessages.EMAIL_OR_PASSWORD_INVALID, System.Net.HttpStatusCode.BadRequest);

            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>();

            foreach(var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

            user.RefreshTokenExpiration = _tokenService.GetRefreshTokenExpiration();
            user.RefreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.UpdateAsync(user);

            var accessToken = _tokenService.GenerateToken(claims, user.UserIdentifier);

            var response = new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken
            };

            return response;
        }
    }
}

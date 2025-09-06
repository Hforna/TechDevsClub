using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Profile.Application.ApplicationServices;
using Profile.Application.Commons;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Application.Services.Validators;
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
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Profile.Application.Services
{
    public interface IUserService
    {
        public Task<UserResponse> CreateUser(CreateUserRequest request, string uri);
        public Task ConfirmEmail(string email, string token);
        public Task<Address> UpdateUserAddress(UpdateAddressRequest request);
        public Task<UserSkillsResponse> SetUserSkills(SetUserSkillsRequest request);
        public Task UpdatePassword(UpdatePasswordRequest request);
        public Task ForgotPassword(string email, string uri);
        public Task CreateUserByOauth(string email, string userName);
        public Task<UserInfosResponse> GetUserInfos();
        public Task<UserRolesResponse> GetUserRoles();
        public Task<UserInfosWithRolesResponse> UserInfosById(long userId);
        public Task ResetPassword(string email, string token, ResetPasswordRequest request);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordEncrypt _passwordEncrypt;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IGeoLocationService _geoLocation;
        private readonly IRequestService _requestService;
        private readonly SqidsEncoder<long> _sqids;

        public UserService(IUnitOfWork uof, IMapper mapper, 
            UserManager<User> userManager, IPasswordEncrypt passwordEncrypt,
            IEmailService emailService, ILogger<UserService> logger, 
            ITokenService tokenService,
            IGeoLocationService geoLocation, IRequestService requestService, SqidsEncoder<long> sqids)
        {
            _uof = uof;
            _sqids = sqids;
            _mapper = mapper;
            _requestService = requestService;
            _logger = logger;
            _geoLocation = geoLocation;
            _userManager = userManager;
            _passwordEncrypt = passwordEncrypt;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        public async Task<UserResponse> CreateUser(CreateUserRequest request, string uri)
        {
            RequestValidatorCommons.Validate<CreateUserRequest, CreateUserValidator>(request);

            var userEmail = await _uof.UserRepository.UserByEmail(request.Email);

            if (userEmail is not null)
            {
                var exception = new ValidationException(ResourceExceptMessages.EMAIL_EXISTS, System.Net.HttpStatusCode.BadRequest);
                _logger.LogError(message: $"An user with this e-mail already exists: {request.Email}", exception: exception);
                throw exception;
            }

            var user = _mapper.Map<User>(request);
            user.NormalizedUserName = request.UserName.ToUpper();
            user.PasswordHash = _passwordEncrypt.Encrypt(request.Password);
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _uof.GenericRepository.Add<User>(user);
            await _uof.Commit();

            var userRole = request.CompanyOwner ? "company_owner" : "normal";
            await _userManager.AddToRoleAsync(user, userRole);

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
            await _emailService.SendEmail(user.Email, user.UserName,
                $"Hello {user.UserName} confirm your email clicking on this link"
                , _emailService.EmailConfirmation($"{uri}api/users/confirm-email?email={user.Email}&token={encodedToken}"));
            _logger.LogDebug($"Message sent to e-mail {user.Email}");

            var response = _mapper.Map<UserResponse>(user);

            return response;
        }

        public async Task ConfirmEmail(string email, string token)
        {
            var userByEmail = await _uof.UserRepository.UserByEmailNotConfirmed(email);

            if (userByEmail is null)
                throw new ContextException(ResourceExceptMessages.EMAIL_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var tokenIsValid = await _userManager.ConfirmEmailAsync(userByEmail, decodedToken);

            if (!tokenIsValid.Succeeded)
            {
                var exception = new AuthenticationException(ResourceExceptMessages.INVALID_EMAIL_TOKEN, System.Net.HttpStatusCode.BadRequest);
                _logger.LogError(message: $"Token {token} is not valid to email {email}", exception: exception);
                throw exception;
            }

            userByEmail.EmailConfirmed = true;
            userByEmail.Active = true;

            var profile = new ProfileEntity()
            {
                UserId = userByEmail.Id,
                User = userByEmail,
                Description = ""
            };

            await _uof.GenericRepository.Add<ProfileEntity>(profile);
            _uof.GenericRepository.Update<User>(userByEmail);
            await _uof.Commit();
        }

        public async Task<Address> UpdateUserAddress(UpdateAddressRequest request)
        {
            var user = await _tokenService.GetUserByToken();

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_DOESNT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var address = _mapper.Map<Address>(request);
            user.Address = address;

            _uof.GenericRepository.Update<User>(user);
            await _uof.Commit();

            return user.Address;
        }

        public async Task<UserSkillsResponse> SetUserSkills(SetUserSkillsRequest request)
        {
            var skillsRequest = request.Skills;

            var user = await _tokenService.GetUserByToken();

            var skillNames = skillsRequest.Select(d => d.Name).Distinct().ToArray();
            var skills = await _uof.SkillRepository.GetSkillsByNames(skillNames);

            if (skills.Count != skillsRequest.Count)
            {
                var missingSkills = skillNames.Except(skills.Select(s => s.Name));
                throw new ContextException($"{ResourceExceptMessages.SKILLS_NOT_EXISTS}: {missingSkills}", System.Net.HttpStatusCode.NotFound);
            }

            var skillsDict = skills.ToDictionary(d => d.Name, f => f.Id);
            foreach(var skill in skillsRequest)
            {
                user.AddSkill(skillsDict[skill.Name], skill.Level);
            }

            await _uof.GenericRepository.AddRange<UserSkills>(user.Skills);
            await _uof.Commit();

            var response = new UserSkillsResponse();
            response.Skills = _mapper.Map<List<SkillUserResponse>>(user.Skills);

            return response;
        }

        public async Task UpdatePassword(UpdatePasswordRequest request)
        {
            RequestValidatorCommons.Validate<UpdatePasswordRequest, UpdatePasswordValidator>(request);

            var user = await _tokenService.GetUserByToken();

            if (_passwordEncrypt.IsValidPassword(request.Password, user.PasswordHash) == false)
                throw new ValidationException(ResourceExceptMessages.PASSWORD_INVALID, HttpStatusCode.BadRequest);

            if (request.NewPassword == request.Password)
                throw new ValidationException(ResourceExceptMessages.UPDATE_WITH_THE_SAME_PASSWORD, HttpStatusCode.BadRequest);

            user.PasswordHash = _passwordEncrypt.Encrypt(request.NewPassword);

            _uof.GenericRepository.Update<User>(user);

            var requestIp = _requestService.GetRequestIp();

            if(!string.IsNullOrEmpty(requestIp))
            {
                var locationInfos = _geoLocation.GetLocationInfosByIp(requestIp);

                await _emailService.SendEmail(user.Email, user.UserName, "You updated your password", @$"Your password was updated, 
                location infos: Country: {locationInfos.Country.Name}, City: {locationInfos.City}");
            }

            await _uof.Commit();
        }

        public async Task ForgotPassword(string email, string uri)
        {
            var user = await _uof.UserRepository.UserByEmail(email);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_WITH_EMAIL_NOT_EXISTS, HttpStatusCode.NotFound);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _emailService.SendEmail(user.Email, user.UserName, "You requested a password redefinition", 
                $"Click on this link for reset you password {uri}users/reset-password?email={user.Email}&token={token}");
        }

        public async Task ResetPassword(string email, string token, ResetPasswordRequest request)
        {
            if (request.NewPassword.Length < 8)
                throw new ValidationException(ResourceExceptMessages.PASSWORD_LENGTH, HttpStatusCode.BadRequest);

            if (request.NewPassword != request.RepeatPassword)
                throw new ValidationException(ResourceExceptMessages.UPDATE_WITH_THE_SAME_PASSWORD, HttpStatusCode.BadRequest);

            var user = await _uof.UserRepository.UserByEmail(email);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_WITH_EMAIL_NOT_EXISTS, HttpStatusCode.NotFound);

            var isSamePassword = _passwordEncrypt.IsValidPassword(request.NewPassword, user.PasswordHash);

            if (isSamePassword)
                throw new ValidationException(ResourceExceptMessages.PASSWORD_CANT_BE_LIKE_OLD, HttpStatusCode.BadRequest);

            var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);

            if (!isValidToken)
                throw new ValidationException(ResourceExceptMessages.INVALID_EMAIL_TOKEN, HttpStatusCode.BadRequest);

            user.PasswordHash = _passwordEncrypt.Encrypt(request.NewPassword);
            _uof.GenericRepository.Update<User>(user);

            await _uof.Commit();
        }

        public async Task CreateUserByOauth(string email, string userName)
        {
            var user = await _uof.UserRepository.UserByEmail(email);

            if (user is null)
            {
                user = new User()
                {
                    Email = email,
                    Active = true,
                    UserName = userName,
                    PasswordHash = "--------",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true
                };

                await _uof.GenericRepository.Add<User>(user);
                await _uof.Commit();

                var profile = new ProfileEntity()
                {
                    UserId = user.Id,
                    User = user,
                    Description = ""
                };

                await _uof.Commit();

                await _userManager.AddToRoleAsync(user, "normal");
            }
        }

        public async Task<UserInfosResponse> GetUserInfos()
        {
            var user = await _tokenService.GetUserByToken();

            var response = _mapper.Map<UserInfosResponse>(user);

            return response;
        }

        public async Task<UserRolesResponse> GetUserRoles()
        {
            var user = await _tokenService.GetUserByToken();

            var roles = await _userManager.GetRolesAsync(user);

            var encodeUserId = _sqids.Encode(user.Id);
            var response = new UserRolesResponse()
            {
                UserId = encodeUserId,
                Roles = roles.Select(role =>
                {
                    var response = new UserRoleResponse()
                    {
                        UserId = encodeUserId,
                        RoleName = role
                    };

                    return response;
                }).ToList()
            };

            return response;
        }

        public async Task<UserInfosWithRolesResponse> UserInfosById(long userId)
        {
            var user = await _uof.GenericRepository.GetById<User>(userId);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_DOESNT_EXISTS, HttpStatusCode.NotFound);

            var roles = await _userManager.GetRolesAsync(user);

            var encodeUserId = _sqids.Encode(user.Id);

            var response = _mapper.Map<UserInfosWithRolesResponse>(user);
            response.UserRoles = new UserRolesResponse()
            {
                UserId = encodeUserId,
                Roles = roles.Select(role =>
            {
                return new UserRoleResponse()
                {
                    RoleName = role,
                    UserId = encodeUserId
                };
            }).ToList()};

            return response;
        }
    }
}

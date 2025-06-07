using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Profile.Application.ApplicationServices;
using Profile.Application.Commons;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Application.Services.Validators;
using Profile.Domain.Aggregates;
using Profile.Domain.Consts;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.Security;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordEncrypt _passwordEncrypt;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IRequestToken _requestToken;

        public UserService(IUnitOfWork uof, IMapper mapper, 
            UserManager<User> userManager, IPasswordEncrypt passwordEncrypt,
            IEmailService emailService, ILogger<UserService> logger, ITokenService tokenService, IRequestToken requestToken)
        {
            _uof = uof;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _passwordEncrypt = passwordEncrypt;
            _emailService = emailService;
            _tokenService = tokenService;
            _requestToken = requestToken;
        }

        public async Task<UserResponse> CreateUser(CreateUserRequest request)
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
            user.PasswordHash = _passwordEncrypt.Encrypt(request.Password);
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _uof.GenericRepository.Add<User>(user);
            await _uof.Commit();

            await _userManager.AddToRoleAsync(user, "normal");

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailService.SendEmail(user.Email, user.UserName,
                $"Hello {user.UserName} confirm your email clicking on this link"
                , _emailService.EmailConfirmation($"{AppConfigs.AppUrl}api/users/confirm-email?email={user.Email}&token={confirmationToken}"));
            _logger.LogDebug($"Message sent to e-mail {user.Email}");

            var response = _mapper.Map<UserResponse>(user);

            return response;
        }

        public async Task ConfirmEmail(string email, string token)
        {
            var userByEmail = await _uof.UserRepository.UserByEmail(email);

            if (userByEmail is null)
                throw new ContextException(ResourceExceptMessages.EMAIL_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var tokenIsValid = await _userManager.ConfirmEmailAsync(userByEmail, token.Replace(" ", "+"));

            if (!tokenIsValid.Succeeded)
            {
                var exception = new AuthorizationException(ResourceExceptMessages.INVALID_EMAIL_TOKEN, System.Net.HttpStatusCode.BadRequest);
                _logger.LogError(message: $"Token {token} is not valid to email {email}", exception: exception);
                throw exception;
            }

            userByEmail.EmailConfirmed = true;
            userByEmail.Active = true;

            await _userManager.UpdateAsync(userByEmail);
        }

        public async Task<Address> CreateUserAddress(UpdateAddressRequest request)
        {
            var userId = _tokenService.GetUserIdentifierByToken(_requestToken.GetToken());

            var user = await _uof.UserRepository.UserByIdentifier(userId);

            if (user is null)
                throw new ContextException(ResourceExceptMessages.USER_DOESNT_EXISTS, System.Net.HttpStatusCode.NotFound);

            var address = _mapper.Map<Address>(request);
            user.Address = address;

            await _uof.GenericRepository.Add<User>(user);
            await _uof.Commit();

            return user.Address;
        }
    }
}

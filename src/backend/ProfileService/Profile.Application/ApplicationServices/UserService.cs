using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Profile.Application.ApplicationServices;
using Profile.Application.Commons;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Application.Services.Validators;
using Profile.Domain.Aggregates;
using Profile.Domain.Consts;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using Profile.Domain.Security;
using Profile.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UserService(IUnitOfWork uof, IMapper mapper, 
            UserManager<User> userManager, IPasswordEncrypt passwordEncrypt, 
            IEmailService emailService)
        {
            _uof = uof;
            _mapper = mapper;
            _userManager = userManager;
            _passwordEncrypt = passwordEncrypt;
            _emailService = emailService;
        }

        public async Task<UserResponse> CreateUser(CreateUserRequest request)
        {
            RequestValidatorCommons.Validate<CreateUserRequest, CreateUserValidator>(request);

            var userEmail = await _userManager.FindByEmailAsync(request.Email);

            if (userEmail is not null)
                throw new ValidationException(ResourceExceptMessages.EMAIL_EXISTS, System.Net.HttpStatusCode.BadRequest);

            var user = _mapper.Map<User>(request);
            user.PasswordHash = _passwordEncrypt.Encrypt(request.Password);
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _uof.GenericRepository.Add<User>(user);
            await _uof.Commit();

            await _userManager.AddToRoleAsync(user, "normal");

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailService.SendEmail(user.Email, user.UserName,
                $"Hello {user.UserName} confirm your email clicking on this link"
                , _emailService.EmailConfirmation($"{AppConfigs.AppUrl}/api/user/confirm-email?email={user.Email}&token={confirmationToken}"));

            var response = _mapper.Map<UserResponse>(user);

            return response;
        }
    }
}

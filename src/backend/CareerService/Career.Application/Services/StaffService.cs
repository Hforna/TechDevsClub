using AutoMapper;
using Career.Application.Requests;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.DomainServices;
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Services
{
    public interface IStaffService
    {
        public Task<RequestStaffResponse> RequestStaffToCompany(PutStaffOnCompanyRequest request);
    }

    public class StaffService : IStaffService
    {
        private readonly IProfileServiceClient _profileService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<StaffService> _logger;
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly ICompanyDomainService _companyDomain;
        private readonly IEmailService _emailService;

        public StaffService(IProfileServiceClient profileService, IUnitOfWork uow, 
            ILogger<StaffService> logger, IMapper mapper, IRequestService requestService, ICompanyDomainService companyDomain, IEmailService emailService)
        {
            _profileService = profileService;
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
            _mapper = mapper;
            _requestService = requestService;
            _companyDomain = companyDomain;
        }

        public async Task<RequestStaffResponse> RequestStaffToCompany(PutStaffOnCompanyRequest request)
        {
            var accessToken = _requestService.GetBearerToken();
            var userInfos = await _profileService.GetUserInfos(accessToken!);

            var company = await _uow.CompanyRepository.CompanyById(request.CompanyId);

            if (company is null)
                throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            var userInCompany = await _uow.CompanyRepository.CompanyContainsStaff(company.Id, request.UserId);

            if (userInCompany)
                throw new DomainException(ResourceExceptMessages.USER_ALREADY_IN_COMPANY);

            if(company.OwnerId != userInfos.id)
            {
                var userStaff = await _uow.StaffRepository.GetStaffByUserIdAndCompany(userInfos.id, company.Id);

                if (userStaff is null)
                    throw new DomainException(ResourceExceptMessages.USER_NOT_BINDED_TO_COMPANY);

                var staffRoles = await _uow.StaffRepository.GetStaffRolesInCompany(company.Id, userStaff.Id);

                _logger.LogInformation($"User requester roles: {staffRoles!.Select(d => d.Role)}");

                if (staffRoles!.Select(d => d.Role).Contains("hiring_manager") == false)
                    throw new DomainException(ResourceExceptMessages.STAFF_DOESNT_HAVE_PERMISSION_FOR_HIRE);
            }

            var userToStaff = await _profileService.GetUserInfosById(request.UserId);

            try
            {
                await _emailService.SendEmailToUserBeStaff(userToStaff.userName, userToStaff.email, company.Name);
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"Unexpectadly error ocurred while trying to send message for user be staff: {ex.Message}");

                throw new ExternalServiceException(ResourceExceptMessages.ERROR_SENDING_EMAIL);
            }
   
            var requestStaff = new RequestStaff() { 
                CompanyId = company.Id, RequesterId = userInfos.id, 
                Role = request.Role, UserId = request.UserId };

            await _uow.GenericRepository.Add<RequestStaff>(requestStaff);
            await _uow.Commit();

            return _mapper.Map<RequestStaffResponse>(requestStaff);
        }
    }
}

using AutoMapper;
using Career.Application.Requests.Company;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.DomainServices;
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using Career.Domain.Services.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Services
{
    public interface ICompanyService
    {
        public Task<CompanyResponse> CreateCompany(CreateCompanyRequest request);
        public Task<CompanyResponse> UpdateCompany(UpdateCompanyRequest request);
        public Task<StaffsResponse> GetCompanyStaffs(Guid companyId);
        public Task<CompanyConfigurationResponse> UpdateCompanyConfiguration(
            Guid companyId, 
            CompanyConfigurationRequest request);
    }

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CompanyService> _logger;
        private readonly IProfileServiceClient _profileService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly string? _accessToken;

        public CompanyService(IUnitOfWork uow, ILogger<CompanyService> logger, 
            IProfileServiceClient profileService, IRequestService requestService, IMapper mapper)
        {
            _uow = uow;
            _logger = logger;
            _profileService = profileService;
            _requestService = requestService;
            _mapper = mapper;

            _accessToken = _requestService.GetBearerToken()!;
        }

        public async Task<CompanyResponse> CreateCompany(CreateCompanyRequest request)
        {
            var token = _requestService.GetBearerToken();

            var userInfos = await _profileService.GetUserInfos(token!);
            var userRoles = await _profileService.GetUserRoles(token!);

            if (userRoles.roles.Select(d => d.roleName).Contains("company_owner") == false)
                throw new DomainException(ResourceExceptMessages.USER_ROLE_OWNER_FOR_CREATE_COMPANY);

            var company = _mapper.Map<Company>(request);
            company.OwnerId = userInfos.id;

            await _uow.GenericRepository.Add<Company>(company);
            await _uow.Commit();

            _logger.LogInformation($"New company created by user: {userInfos.userName}, " +
                $"company details: id: {company.Id}, name: {company.Name}, website: {company.Website}");

            return _mapper.Map<CompanyResponse>(company);
        }

        public async Task<StaffsResponse> GetCompanyStaffs(Guid companyId)
        {
            var company = await _uow.CompanyRepository.CompanyById(companyId);

            if(company is null)
            {
                _logger.LogError($"Company with id {companyId} was not found");
                throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);
            }

            if (!company.CompanyConfiguration.ShowStaffs)
            {
                if (string.IsNullOrEmpty(_accessToken))
                    throw new DomainException(ResourceExceptMessages.SHOW_STAFFS_PRIVATE);

                var userInfos = await _profileService.GetUserInfos(_accessToken);

                var isStaff = await _uow.CompanyRepository.CompanyContainsStaff(companyId, userInfos.id);

                if (company.OwnerId != userInfos.id && !isStaff)
                    throw new DomainException(ResourceExceptMessages.SHOW_STAFFS_PRIVATE);
            }

            var staffs = await _uow.StaffRepository.GetAllStaffsFromACompany(companyId);
            var staffsIds = staffs.Select(d => d.Id).ToList();
            var staffsRoles = await _uow.StaffRepository.GetStaffsRole(staffsIds);

            var response = new StaffsResponse()
            {
                CompanyId = companyId,
                Staffs = staffs.Select(staff =>
                {
                    var response = _mapper.Map<StaffResponse>(staff);
                    response.Role = staffsRoles
                    .Where(d => d.StaffId == staff.Id)
                    .SingleOrDefault()!.Role;

                    return response;
                }).ToList()
            };

            return response;
        }

        public async Task<CompanyResponse> UpdateCompany(UpdateCompanyRequest request)
        {
            var accessToken = _requestService.GetBearerToken();

            var user = await _profileService.GetUserInfos(accessToken!);

            var company = await _uow.CompanyRepository.CompanyById(request.CompanyId) 
                ?? throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            if (company.OwnerId != user.id)
                throw new DomainException(ResourceExceptMessages.USER_NOT_COMPANY_OWNER);

            _mapper.Map(request, company);

            _uow.GenericRepository.Update<Company>(company);
            await _uow.Commit();

            return _mapper.Map<CompanyResponse>(company);
        }

        public async Task<CompanyConfigurationResponse> UpdateCompanyConfiguration(
            Guid companyId, 
            CompanyConfigurationRequest request)
        {
            var company = await _uow.CompanyRepository.CompanyById(companyId)
                ?? throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            var userInfos = await _profileService.GetUserInfos(_accessToken!);

            if (company.OwnerId != userInfos.id)
                throw new DomainException(ResourceExceptMessages.USER_NOT_COMPANY_OWNER);

            _mapper.Map(request, company);

            _uow.GenericRepository.Update<Company>(company);
            await _uow.Commit();

            return _mapper.Map<CompanyConfigurationResponse>(company);
        }
    }
}

using AutoMapper;
using Career.Application.Requests.Company;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.DomainServices;
using Career.Domain.Dtos;
using Career.Domain.Dtos.Notifications;
using Career.Domain.Entities;
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
    public interface ICompanyService
    {
        public Task<CompanyResponse> CreateCompany(CreateCompanyRequest request);
        public Task<CompanyResponse> UpdateCompany(UpdateCompanyRequest request);
        public Task<CompanyResponse> GetCompany(Guid id);
        public Task<CompanyPaginatedResponse> GetCompanyFiltered(CompaniesFilterRequest request);
        public Task FireStaffFromCompany(Guid companyId, Guid staffId, string reason);
        public Task<StaffsResponse> GetCompanyStaffs(Guid companyId);
        public Task<CompanyConfigurationResponse> GetCompanyConfigurationInfos(Guid companyId);
        public Task<CompanyConfigurationResponse> UpdateCompanyConfiguration(
            Guid companyId, 
            CompanyConfigurationRequest request);
        public Task DeleteCompany(Guid companyId, string reactivateUrl);
    }

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CompanyService> _logger;
        private readonly IProfileServiceClient _profileService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly string? _accessToken;
        private readonly ICompanyDomainService _companyDomain;
        private readonly IRealTimeNotifier _realTimeNotifier;
        private readonly IEmailService _emailService;

        public CompanyService(IUnitOfWork uow, ILogger<CompanyService> logger, 
            IProfileServiceClient profileService, 
            IRequestService requestService, IMapper mapper, 
            ICompanyDomainService companyDomain, IRealTimeNotifier realTimeNotifier, IEmailService emailService)
        {
            _uow = uow;
            _realTimeNotifier = realTimeNotifier;
            _companyDomain = companyDomain;
            _logger = logger;
            _profileService = profileService;
            _requestService = requestService;
            _emailService = emailService;
            _mapper = mapper;

            _accessToken = _requestService.GetBearerToken()!;
        }

        public async Task<CompanyResponse> CreateCompany(CreateCompanyRequest request)
        {
            var userInfos = await _profileService.GetUserInfos(_accessToken!);
            var userRoles = await _profileService.GetUserRoles(_accessToken!);

            if (userRoles.roles.Select(d => d.roleName).Contains("company_owner") == false)
                throw new DomainException(ResourceExceptMessages.USER_ROLE_OWNER_FOR_CREATE_COMPANY);

            var company = _mapper.Map<Company>(request);
            company.OwnerId = userInfos.id;

            await _uow.GenericRepository.Add<Company>(company);
            await _uow.Commit();

            var companyConfiguration = new CompanyConfiguration()
            {
                CompanyId = company.Id,
            };

            await _uow.GenericRepository.Add<CompanyConfiguration>(companyConfiguration);
            await _uow.Commit();
            _logger.LogInformation($"New company created by user: {userInfos.userName}, " +
                $"company details: id: {company.Id}, name: {company.Name}, website: {company.Website}");

            return _mapper.Map<CompanyResponse>(company);
        }

        public async Task DeleteCompany(Guid companyId, string reactivateUrl)
        {
            var company = await _uow.CompanyRepository.CompanyByIdWithJobs(companyId)
                ?? throw new NullEntityException("Company was not found");

            var user = await _profileService.GetUserInfos(_accessToken!);

            if (user.id != company.OwnerId)
                throw new UnauthorizedException("User must be the company owner to delete this company");

            company.Jobs = company.Jobs.Select(job =>
            {
                job.IsActive = false;

                return job;
            }).ToList();
            company.IsActive = false;
            _uow.GenericRepository.Update<Company>(company);
            await _uow.Commit();

            await _emailService.SendEmailCompanyDeleted(company.Name, user.userName, user.email, reactivateUrl, DateTime.UtcNow);
        }

        public async Task FireStaffFromCompany(Guid companyId, Guid staffId, string reason)
        {
            var company = await _uow.CompanyRepository.CompanyById(companyId) 
                ?? throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            var userInfos = await _profileService.GetUserInfos(_accessToken!);

            if (company.OwnerId != userInfos.id)
                throw new DomainException(ResourceExceptMessages.USER_NOT_COMPANY_OWNER);

            var staff = await _uow.StaffRepository.GetStaffByIdAndCompany(staffId, companyId)
                ?? throw new NullEntityException(ResourceExceptMessages.STAFF_NOT_IN_COMPANY);

            _uow.GenericRepository.Remove<Staff>(staff);

            var notification = new Notification()
            {
                UserId = staff.UserId,
                Type = Domain.Enums.ENotificationType.Information,
                IsRead = false,
                Title = ResourceNotificationMessages.USER_WERE_FIRED,
                Message = reason,
                SenderId = userInfos.id
            };

            await _realTimeNotifier.SendNotification(_mapper.Map<SendNotificationDto>(notification));

            await _uow.GenericRepository.Add<Notification>(notification);
            await _uow.Commit();
        }

        public async Task<CompanyResponse> GetCompany(Guid id)
        {
            var company = await _uow.CompanyRepository.CompanyById(id)
                ?? throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            var isOwner = false;

            if (company.CompanyConfiguration.IsPrivate)
            {
                if(string.IsNullOrEmpty(_accessToken))
                {
                    _logger.LogError($"Company with id {id} is private and user is not authenticated");
                    throw new DomainException(ResourceExceptMessages.COMPANY_PRIVATE);
                }

                var userInfos = await _profileService.GetUserInfos(_accessToken);

                if (userInfos.id != company.OwnerId)
                {
                    var containsStaff = await _uow.CompanyRepository
                    .CompanyContainsStaff(company.Id, userInfos.id);

                    if (!containsStaff) throw new DomainException(ResourceExceptMessages.STAFF_IN_COMPANY);
                }
                else
                {
                    isOwner = true;
                }
            }
            var response = _mapper.Map<CompanyResponse>(company);

            if(!isOwner)
            {
                var responseByconfiguration = _companyDomain
                    .GetCompanyResponseByConfigurations(company.CompanyConfiguration, company);

                response = _mapper.Map<CompanyResponse>(responseByconfiguration);
            }
            return response;
        }

        public async Task<CompanyConfigurationResponse> GetCompanyConfigurationInfos(Guid companyId)
        {
            var company = await _uow.CompanyRepository.CompanyById(companyId) 
                ?? throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            var user = await _profileService.GetUserInfos(_accessToken!);

            if (user.id != company.OwnerId)
                throw new DomainException(ResourceExceptMessages.USER_NOT_COMPANY_OWNER);

            return _mapper.Map<CompanyConfigurationResponse>(company.CompanyConfiguration);
        }

        public async Task<CompanyPaginatedResponse> GetCompanyFiltered(CompaniesFilterRequest request)
        {
            if (request.PerPage > 100)
                throw new RequestException(ResourceExceptMessages.MAX_PERPAGE_100);

            var filterDto = _mapper.Map<CompanyFilterDto>(request);

            var companies = _uow.CompanyRepository.GetCompaniesPaginated(filterDto);

            var companiesResponse = companies.Select(company =>
            {
                var response = _companyDomain.GetCompanyResponseByConfigurations(company.CompanyConfiguration, company);

                //get all companies logo from storage service

                return _mapper.Map<CompanyShortResponse>(response);
            }).ToList();

            return new CompanyPaginatedResponse()
            {
                Companies = companiesResponse,
                Count = companies.Count,
                HasNextPage = companies.HasNextPage,
                HasPreviousPage = companies.HasPreviousPage,
                IsFirstPage = companies.IsFirstPage,
                IsLastPage = companies.IsLastPage
            };
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
                        .SingleOrDefault(d => d.StaffId == staff.Id)!.Role;

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

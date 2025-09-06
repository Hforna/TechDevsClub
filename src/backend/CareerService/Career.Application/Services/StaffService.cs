using AutoMapper;
using Career.Application.Requests;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.DomainServices;
using Career.Domain.Dtos;
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
    public interface IStaffService
    {
        public Task<StaffRequestResponse> StaffRequestToCompany(PutStaffOnCompanyRequest request);
        public Task<StaffRequestResponse> GetStaffRequestStatus(Guid requestId);
        public Task<StaffRequestsResponse> UserStaffRequests(int perPage, int page);
        public Task<StaffRequestResponse> AcceptStaffRequest(Guid requestId);
        public Task<StaffRequestResponse> RejectStaffRequest(Guid requestId);
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
        private readonly IRealTimeNotifier _realTimeNotifier;
        private string _accessToken;

        public StaffService(IProfileServiceClient profileService, IUnitOfWork uow, 
            ILogger<StaffService> logger, IMapper mapper, 
            IRequestService requestService, 
            ICompanyDomainService companyDomain, IEmailService emailService, IRealTimeNotifier realTimeNotifier)
        {
            _profileService = profileService;
            _realTimeNotifier = realTimeNotifier;
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
            _mapper = mapper;
            _requestService = requestService;
            _companyDomain = companyDomain;

            _accessToken = _requestService.GetBearerToken()!;
        }

        public async Task<StaffRequestResponse> AcceptStaffRequest(Guid requestId)
        {
            var staffRequest = await _uow.StaffRepository.GetRequestStaffById(requestId)
                ?? throw new NullEntityException(ResourceExceptMessages.STAFF_REQUEST_NOT_EXISTS);

            var userInfos = await _profileService.GetUserInfos(_accessToken);

            if (userInfos.id != staffRequest.UserId)
                throw new DomainException(ResourceExceptMessages.USER_NOT_REQUESTED_ON_STAFF_REQUEST);

            staffRequest.AcceptRequest();

            var company = await _uow.CompanyRepository.CompanyById(staffRequest.CompanyId);
            await _companyDomain.AddStaffToCompany(company!, userInfos.id);

            _uow.GenericRepository.Update<RequestStaff>(staffRequest);
            _uow.GenericRepository.Update<Company>(company!);

            await _uow.Commit();

            return _mapper.Map<StaffRequestResponse>(staffRequest);
        }

        public async Task<StaffRequestResponse> GetStaffRequestStatus(Guid requestId)
        {
            var userInfos = await _profileService.GetUserInfos(_accessToken);

            var request = await _uow.StaffRepository.GetRequestStaffById(requestId) ?? throw new NullEntityException(ResourceExceptMessages.STAFF_REQUEST_NOT_EXISTS);

            if (request.UserId != userInfos.id || request.RequesterId != userInfos.id)
                throw new DomainException(ResourceExceptMessages.USER_CANNOT_SEE_STAFF_REQUEST_STATUS);

            return _mapper.Map<StaffRequestResponse>(request);
        }

        public async Task<StaffRequestResponse> RejectStaffRequest(Guid requestId)
        {
            var request = await _uow.StaffRepository.GetRequestStaffById(requestId)
                ?? throw new NullEntityException(ResourceExceptMessages.STAFF_REQUEST_NOT_EXISTS);

            var userInfos = await _profileService.GetUserInfos(_accessToken);

            if (userInfos.id != request.UserId)
                throw new DomainException(ResourceExceptMessages.USER_NOT_REQUESTED_ON_STAFF_REQUEST);

            request.RejectRequest();

            _uow.GenericRepository.Update<RequestStaff>(request);
            await _uow.Commit();

            return _mapper.Map<StaffRequestResponse>(request);
        }

        public async Task<StaffRequestResponse> StaffRequestToCompany(PutStaffOnCompanyRequest request)
        {
            var userInfos = await _profileService.GetUserInfos(_accessToken);

            var company = await _uow.CompanyRepository.CompanyById(request.CompanyId);

            if (company is null)
                throw new NullEntityException(ResourceExceptMessages.COMPANY_NOT_EXISTS);

            var userInCompany = await _uow.CompanyRepository.CompanyContainsStaff(company.Id, request.UserId);

            if (userInCompany)
                throw new DomainException(ResourceExceptMessages.USER_ALREADY_IN_COMPANY);

            if (company.OwnerId != userInfos.id)
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
            } catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpectadly error ocurred while trying to send message for user be staff: {ex.Message}");

                throw new ExternalServiceException(ResourceExceptMessages.ERROR_SENDING_EMAIL);
            }

            var requestStaff = new RequestStaff() {
                CompanyId = company.Id, RequesterId = userInfos.id,
                Role = request.Role, UserId = request.UserId };

            var notification = new Notification()
            {
                Message = $"",
                Title = "",
                Type = Domain.Enums.ENotificationType.StaffRequest,
                UserId = userToStaff.id,
                SenderId = userInfos.id
            };
            await _uow.GenericRepository.Add<Notification>(notification);
            await _uow.GenericRepository.Add<RequestStaff>(requestStaff);
            await _uow.Commit();

            var sendNotificationDto = _mapper.Map<SendNotificationDto>(notification);
            await _realTimeNotifier.SendNotification(sendNotificationDto);

            return _mapper.Map<StaffRequestResponse>(requestStaff);
        }

        public async Task<StaffRequestsResponse> UserStaffRequests(int perPage, int page)
        {
            var userInfos = await _profileService.GetUserInfos(_accessToken);

            var requests = _uow.StaffRepository.GetUserStaffRequestsPaged(perPage, page, userInfos.id);

            var staffRequestsItems = requests.ToList();

            var response = new StaffRequestsResponse()
            {
                Requests = staffRequestsItems.Select(staffreq =>
                {
                    var response = _mapper.Map<StaffRequestResponse>(staffreq);

                    return response;
                }).ToList(),
                TotalAccepted = staffRequestsItems.Count(d => d.Status == Domain.Enums.ERequestStaffStatus.APPROVED),
                TotalRejected = staffRequestsItems.Count(d => d.Status == Domain.Enums.ERequestStaffStatus.REJECTED),
                TotalPending = staffRequestsItems.Count(d => d.Status == Domain.Enums.ERequestStaffStatus.PENDING),
                UserId = userInfos.id,
                Count = requests.Count,
                HasNextPage = requests.HasNextPage,
                HasPreviousPage = requests.HasPreviousPage,
                IsLastPage = requests.IsLastPage,
                IsFirstPage = requests.IsFirstPage     
            };

            return response;
        }
    }
}

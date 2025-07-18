﻿using AutoMapper;
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
    }

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CompanyService> _logger;
        private readonly IProfileServiceClient _profileService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;

        public CompanyService(IUnitOfWork uow, ILogger<CompanyService> logger, 
            IProfileServiceClient profileService, IRequestService requestService, IMapper mapper)
        {
            _uow = uow;
            _logger = logger;
            _profileService = profileService;
            _requestService = requestService;
            _mapper = mapper;
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
    }
}

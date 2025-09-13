using AutoMapper;
using Career.Application.Requests.Jobs;
using Career.Application.Responses;
using Career.Application.Validators;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Dtos;
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using Career.Domain.Services.Clients;
using Career.Domain.Services.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Services
{
    public interface IJobService
    {
        public Task<JobResponse> CreateJob(CreateJobRequest request);
    }

    public class JobService : IJobService
    {
        private readonly ILogger<IJobService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IProfileServiceClient _profileService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly IJobServiceProducer _jobProducer;

        public JobService(ILogger<IJobService> logger, IUnitOfWork uow, 
            IProfileServiceClient profileService, IRequestService requestService, 
            IMapper mapper, IJobServiceProducer jobProducer)
        {
            _logger = logger;
            _uow = uow;
            _profileService = profileService;
            _requestService = requestService;
            _mapper = mapper;
            _jobProducer = jobProducer;
        }

        public async Task<JobResponse> CreateJob(CreateJobRequest request)
        {
            var validator = new JobRequestValidator();
            var validate = validator.Validate(request);

            if(!validate.IsValid)
            {
                var errorMessages = validate.Errors.Select(d => d.ErrorMessage).ToList();
                throw new RequestException(errorMessages);
            }

            var company = await _uow.CompanyRepository.CompanyById(request.CompanyId) 
                ?? throw new NullEntityException("Company by id was not found");

            var userInfos = await _profileService.GetUserInfos(_requestService.GetBearerToken()!);

            if(userInfos.id != company.OwnerId)
            {
                var staff = await _uow.StaffRepository.GetStaffByUserIdAndCompany(userInfos.id, request.CompanyId)
                    ?? throw new DomainException("User is not assigned to this company");

                var staffRoles = await _uow.StaffRepository.GetStaffRolesInCompany(company.Id, staff.Id);

                if (staffRoles is null)
                {
                    _logger.LogError($"Occured an unexpectadly error while trying get roles of staff: {staff.Id}");

                    throw new NullEntityException("It was not possible verify staff roles");
                }
            }

            var job = _mapper.Map<Job>(request);
            await _uow.GenericRepository.Add<Job>(job);
            await _uow.Commit();

            var producerDto = _mapper.Map<JobCreatedDto>(job);
            await _jobProducer.SendJobCreated(producerDto);
            _logger.LogInformation($"Producer dto message sent to job producer service: {producerDto}");

            return _mapper.Map<JobResponse>(request);
        }
    }
}

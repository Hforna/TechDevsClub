using AutoMapper;
using Career.Application.Requests.Jobs;
using Career.Application.Responses;
using Career.Application.Validators;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.DomainServices;
using Career.Domain.Dtos;
using Career.Domain.Dtos.Notifications;
using Career.Domain.Entities;
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using Career.Domain.Services;
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
        public Task<JobApplicationResponse> ApplyToJob(ApplyToJobRequest request, Guid jobId);
        public Task<JobApplicationPaginatedResponse> GetJobApplications(Guid jobId, int perPage, int page);
        public Task AnalyzeJobApplications(AnalyzeJobsRequest request);
    }

    public class JobService : IJobService
    {
        private readonly ILogger<IJobService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IProfileServiceClient _profileService;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly IJobServiceProducer _jobProducer;
        private readonly IFileService _fileService;
        //private readonly IStorageService _storageService;
        private readonly IRealTimeNotifier _realTimeNotifier;
        private readonly ICompanyDomainService _companyDomain;

        public JobService(ILogger<IJobService> logger, IUnitOfWork uow, 
            IProfileServiceClient profileService, IRequestService requestService, 
            IMapper mapper, IJobServiceProducer jobProducer, 
            IFileService fileService, IRealTimeNotifier realTime, ICompanyDomainService companyDomain)
        {
            _logger = logger;
            _uow = uow;
            _companyDomain = companyDomain;
            _realTimeNotifier = realTime;
            //_storageService = storageService;
            _fileService = fileService;
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
            producerDto.Country = company.Location.Country;
            await _jobProducer.SendJobCreated(producerDto);
            _logger.LogInformation($"Producer dto message sent to job producer service: {producerDto}");

            return _mapper.Map<JobResponse>(job);
        }

        public async Task<JobApplicationResponse> ApplyToJob(ApplyToJobRequest request, Guid jobId)
        {
            using var file = request.Resume.OpenReadStream();

            var validateFile = _fileService.IsFileAsPdfOrTxt(file);
            if (!validateFile.isValid)
                throw new RequestException("Invalid file format, it must be txt or pdf type");

            var user = await _profileService.GetUserInfos(_requestService.GetBearerToken()!);
            var profile = await _profileService.GetProfilesInfoByUser(user.id);

            var job = await _uow.JobRepository.GetJobById(jobId) ?? throw new NullEntityException("The job was not found");

            var jobApplication = _mapper.Map<JobApplication>(request);
            jobApplication.Id = Guid.NewGuid();
            jobApplication.ProfileId = user.id;
            jobApplication.ResumeName = $"{Guid.NewGuid()}{validateFile.ext}";
            jobApplication.Status = Domain.Enums.EApplicationStatus.Applied;
            jobApplication.JobId = jobId;

            var hiringManagers = await _uow.StaffRepository.GetHiringManagersFromCompany(job.CompanyId);

            var notifications = hiringManagers.Select(hiringManager => new Notification()
                {
                    UserId = hiringManager.UserId,
                    Title = $"Company {job.Company.Name} received a new job application",
                    Message = $"New job application with id: {jobApplication.Id}",
                    Type = Domain.Enums.ENotificationType.Information,
                }
            ).ToList();
            notifications.Add(new Notification() {
                Title = $"Company {job.Company.Name} received a new job application",
                UserId = job.Company.OwnerId, 
                Message = $"New job application with id: {jobApplication.Id}",
                Type = Domain.Enums.ENotificationType.Information
            });

            await _uow.GenericRepository.Add<JobApplication>(jobApplication);
            await _uow.GenericRepository.AddRange<Notification>(notifications);
            await _uow.Commit();

            //await _storageService.UploadUserResumeFile(jobApplication.Id, jobApplication.ResumeName, file);

            var notifyDto = new InformationNotificationManyUsersDto(_mapper.Map<List<SendNotificationDto>>(notifications));
            await _realTimeNotifier.SendInformationNotificationManyUsers(notifyDto);

            return _mapper.Map<JobApplicationResponse>(jobApplication);
        }

        public async Task<JobApplicationPaginatedResponse> GetJobApplications(Guid jobId, int perPage, int page)
        {
            var user = await _profileService.GetUserInfos(_requestService.GetBearerToken()!);

            var job = await _uow.JobRepository.GetJobById(jobId) ?? throw new NullEntityException("Job was not found");

            var hasPermission = await _companyDomain.CanUserHandleHiringManagement(job.Company, user.id);
            if (!hasPermission)
                throw new DomainException("User doesn't has permission for get job applications");

            var applications = _uow.JobRepository.GetJobApplicationsPaginated(jobId, perPage, page);

            var response = new JobApplicationPaginatedResponse()
            {
                IsFirstPage = applications.IsFirstPage,
                IsLastPage = applications.IsLastPage,
                HasPreviousPage = applications.HasPreviousPage,
                HasNextPage = applications.HasNextPage,
                Count = applications.Count,
            };
            response.JobApplications = applications.Select(app =>
            {
                var appResponse = _mapper.Map<JobApplicationResponse>(app);
                switch(app.Status)
                {
                    case Domain.Enums.EApplicationStatus.Applied:
                        response.TotalApplied++;
                        break;
                    case Domain.Enums.EApplicationStatus.Interview:
                        response.TotalInterview++;
                        break;
                    case Domain.Enums.EApplicationStatus.Rejected:
                        response.TotalRejected++; 
                        break;
                }
                return appResponse;
            }).ToList();

            return response;
        }

        public async Task AnalyzeJobApplications(AnalyzeJobsRequest request)
        {
            var job = await _uow.JobRepository.GetJobById(request.JobId)
                      ?? throw new NullEntityException("Job was not found");

            var jobsApplications = await _uow.JobRepository.GetJobApplicationsByIds(request.JobApplicationsIds);

            if (jobsApplications is null || jobsApplications.Count != request.JobApplicationsIds.Count)
                throw new RequestException("Some job applications weren't found");
            
            var user = await _profileService.GetUserInfos(_requestService.GetBearerToken()!);
            var hasPermission = await _companyDomain.CanUserHandleHiringManagement(job.Company, user.id);
            
            if (!hasPermission)
                throw new UnauthorizedException("User doesn't have permission to analyze these jobs");
            
            await _jobProducer.SendAnalyzingJob(_mapper.Map<JobAnalyzingDto>(request));
        }
    }
}

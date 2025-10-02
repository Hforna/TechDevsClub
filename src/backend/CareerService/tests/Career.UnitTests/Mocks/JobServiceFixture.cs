using AutoMapper;
using Bogus;
using Career.Application.Requests.Jobs;
using Career.Application.Responses;
using Career.Application.Services;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.DomainServices;
using Career.Domain.Dtos;
using Career.Domain.Dtos.Notifications;
using Career.Domain.Entities;
using Career.Domain.Enums;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Career.Domain.Services.Messaging;
using Career.Domain.ValueObjects;
using Career.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using X.PagedList;

namespace Career.Application.Tests.Services
{
    public class JobServiceTestFixture
    {
        // Mocks
        public Mock<ILogger<IJobService>> MockLogger { get; }
        public Mock<IUnitOfWork> MockUnitOfWork { get; }
        public Mock<IProfileServiceClient> MockProfileService { get; }
        public Mock<IRequestService> MockRequestService { get; }
        public IMapper Mapper { get; }
        public Mock<IJobServiceProducer> MockJobProducer { get; }
        public Mock<IFileService> MockFileService { get; }
        public Mock<IRealTimeNotifier> MockRealTimeNotifier { get; }
        public Mock<ICompanyDomainService> MockCompanyDomainService { get; }

        // Repository Mocks
        public Mock<ICompanyRepository> MockCompanyRepository { get; }
        public Mock<IStaffRepository> MockStaffRepository { get; }
        public Mock<IJobRepository> MockJobRepository { get; }
        public Mock<IGenericRepository> MockGenericRepository { get; }

        // Fakers
        private readonly Faker _faker;

        public JobServiceTestFixture()
        {
            _faker = new Faker();

            // Initialize mocks
            MockLogger = new Mock<ILogger<IJobService>>();
            MockUnitOfWork = new Mock<IUnitOfWork>();
            MockProfileService = new Mock<IProfileServiceClient>();
            MockRequestService = new Mock<IRequestService>();
            MockJobProducer = new Mock<IJobServiceProducer>();
            MockFileService = new Mock<IFileService>();
            MockRealTimeNotifier = new Mock<IRealTimeNotifier>();
            MockCompanyDomainService = new Mock<ICompanyDomainService>();

            // Initialize repository mocks
            MockCompanyRepository = new Mock<ICompanyRepository>();
            MockStaffRepository = new Mock<IStaffRepository>();
            MockJobRepository = new Mock<IJobRepository>();
            MockGenericRepository = new Mock<IGenericRepository>();

            // Setup UnitOfWork to return repository mocks
            MockUnitOfWork.Setup(x => x.CompanyRepository).Returns(MockCompanyRepository.Object);
            MockUnitOfWork.Setup(x => x.StaffRepository).Returns(MockStaffRepository.Object);
            MockUnitOfWork.Setup(x => x.JobRepository).Returns(MockJobRepository.Object);
            MockUnitOfWork.Setup(x => x.GenericRepository).Returns(MockGenericRepository.Object);

            // Setup RequestService to return a bearer token
            MockRequestService.Setup(x => x.GetBearerToken()).Returns("Bearer test-token");

            // Initialize real AutoMapper with test mapping profile
            Mapper = MapperMock.GetMock();
        }

        public JobService CreateService()
        {
            return new JobService(
                MockLogger.Object,
                MockUnitOfWork.Object,
                MockProfileService.Object,
                MockRequestService.Object,
                Mapper,
                MockJobProducer.Object,
                MockFileService.Object,
                MockRealTimeNotifier.Object,
                MockCompanyDomainService.Object
            );
        }

        #region CreateJobRequest Helpers

        public CreateJobRequest CreateValidJobRequest()
        {
            return new CreateJobRequest
            {
                CompanyId = Guid.NewGuid(),
                Title = _faker.Name.JobTitle(),
                Description = _faker.Lorem.Paragraph(),
                Experience = _faker.PickRandom<ExperienceLevel>(),
                LocalType = _faker.PickRandom<ELocalType>(),
                Salary = new SalaryRequest
                {
                    MinSalary = _faker.Random.Decimal(30000, 50000),
                    MaxSalary = _faker.Random.Decimal(50001, 100000),
                    Currency = _faker.PickRandom<ECurrency>()
                },
                JobRequirements = new List<JobRequirementRequest>
                {
                    new JobRequirementRequest
                    {
                        SkillId = Guid.NewGuid().ToString(),
                        IsMandatory = _faker.Random.Bool(),
                        ExperienceTime = _faker.PickRandom<ExperienceLevel>()
                    }
                }
            };
        }

        public CreateJobRequest CreateInvalidJobRequest()
        {
            return new CreateJobRequest
            {
                CompanyId = Guid.Empty,
                Title = "", // Invalid: empty title
                Description = "",
                JobRequirements = new List<JobRequirementRequest>()
            };
        }

        #endregion

        #region Company Helpers

        public Company CreateCompany(bool isOwner = true, Guid? ownerId = null)
        {
            var companyId = Guid.NewGuid();
            var ownerIdValue = ownerId.ToString() ?? (isOwner ? Guid.NewGuid().ToString() : Guid.NewGuid().ToString());

            return new Company
            {
                Id = companyId,
                Name = _faker.Company.CompanyName(),
                OwnerId = ownerIdValue.ToString(),
                Logo = _faker.Image.PicsumUrl(),
                Location = new Location(
                    _faker.Address.Country(),
                    _faker.Address.State(),
                    _faker.Address.ZipCode()
                ),
                Description = _faker.Lorem.Paragraph(),
                Verified = _faker.Random.Bool(),
                Website = _faker.Internet.Url(),
                CreatedAt = DateTime.UtcNow,
                Rate = _faker.Random.Decimal(0, 5),
                CompanyConfigurationId = Guid.NewGuid(),
                CompanyConfiguration = new CompanyConfiguration
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    IsPrivate = false,
                    ShowStaffs = true,
                    HighlightVerifiedStatus = true,
                    NotifyStaffsOnNewReview = false,
                    NotifyStaffsOnNewJobApplication = false,
                    NotifyStaffsOnJobApplicationUpdate = false
                },
                Staffs = new List<Staff>()
            };
        }

        #endregion

        #region User & Profile Helpers

        public UserInfosDto CreateUserInfo(string userId)
        {
            return new UserInfosDto
            {
                id = userId,
                userName = _faker.Internet.UserName(),
                email = _faker.Internet.Email(),
                createdAt = DateTime.UtcNow
            };
        }

        public ProfileInfosDto CreateProfile()
        {
            return new ProfileInfosDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
                IsPrivate = false
            };
        }

        #endregion

        #region Staff Helpers

        public Staff CreateStaff(string userId, Guid companyId)
        {
            return new Staff
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyId = companyId,
                StaffRole = new StaffRole
                {
                    Id = Guid.NewGuid(),
                    StaffId = Guid.NewGuid(),
                    CompanyId = companyId,
                    Role = StaffRolesConsts.HiringManagers
                }
            };
        }

        public List<StaffRole> CreateStaffRoles()
        {
            return new List<StaffRole>
            {
                new StaffRole
                {
                    Id = Guid.NewGuid(),
                    StaffId = Guid.NewGuid(),
                    CompanyId = Guid.NewGuid(),
                    Role = StaffRolesConsts.HiringManagers
                }
            };
        }

        public List<Staff> CreateHiringManagers(int count)
        {
            var managers = new List<Staff>();
            for (int i = 0; i < count; i++)
            {
                managers.Add(new Staff
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid().ToString(),
                    CompanyId = Guid.NewGuid(),
                    StaffRole = new StaffRole
                    {
                        Id = Guid.NewGuid(),
                        Role = StaffRolesConsts.HiringManagers
                    }
                });
            }
            return managers;
        }

        #endregion

        #region Job Helpers

        public Job CreateJob(CreateJobRequest request)
        {
            return new Job
            {
                Id = Guid.NewGuid(),
                CompanyId = request.CompanyId,
                Title = request.Title,
                Description = request.Description,
                PostedAt = DateTime.UtcNow,
                IsActive = true,
                LocalType = request.LocalType,
                Salary = request.Salary != null
                    ? new Salary(request.Salary.MinSalary, request.Salary.MaxSalary, request.Salary.Currency)
                    : null,
                JobRequirements = request.JobRequirements?.Select(jr => new JobRequirement
                {
                    Id = Guid.NewGuid(),
                    SkillId = jr.SkillId,
                    IsMandatory = jr.IsMandatory,
                    ExperienceTime = jr.ExperienceTime
                }).ToList() ?? new List<JobRequirement>()
            };
        }

        public Job CreateJobWithCompany(Guid jobId)
        {
            var company = CreateCompany();
            return new Job
            {
                Id = jobId,
                CompanyId = company.Id,
                Company = company,
                Title = _faker.Name.JobTitle(),
                Description = _faker.Lorem.Paragraph(),
                PostedAt = DateTime.UtcNow,
                IsActive = true,
                LocalType = _faker.PickRandom<ELocalType>(),
                Salary = new Salary(
                    _faker.Random.Decimal(30000, 50000),
                    _faker.Random.Decimal(50001, 100000),
                    _faker.PickRandom<ECurrency>()
                ),
                JobRequirements = new List<JobRequirement>()
            };
        }

        #endregion

        #region JobApplication Helpers

        public ApplyToJobRequest CreateApplyToJobRequest(bool isPdf = true)
        {
            var fileMock = new Mock<IFormFile>();
            var content = "Test file content";
            var fileName = isPdf ? "resume.pdf" : "document.doc";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            return new ApplyToJobRequest
            {
                Resume = fileMock.Object,
                DesiredSalary = _faker.Random.Decimal(40000, 80000)
            };
        }

        public JobApplication CreateJobApplication()
        {
            return new JobApplication
            {
                Id = Guid.NewGuid(),
                JobId = Guid.NewGuid(),
                ProfileId = Guid.NewGuid().ToString(),
                ResumeName = $"{Guid.NewGuid()}.pdf",
                Status = EApplicationStatus.Applied,
                DesiredSalary = _faker.Random.Decimal(40000, 80000)
            };
        }

        public JobApplication CreateJobApplicationWithStatus(EApplicationStatus status)
        {
            var application = CreateJobApplication();
            application.Status = status;
            return application;
        }

        public List<JobApplication> CreateJobApplicationsList(int count)
        {
            var applications = new List<JobApplication>();
            for (int i = 0; i < count; i++)
            {
                applications.Add(CreateJobApplication());
            }
            return applications;
        }

        public IPagedList<JobApplication> CreatePaginatedApplications(List<JobApplication> applications)
        {
            return new StaticPagedList<JobApplication>(
                applications,
                1, // page number
                10, // page size
                applications.Count // total count
            );
        }

        #endregion
    }
}
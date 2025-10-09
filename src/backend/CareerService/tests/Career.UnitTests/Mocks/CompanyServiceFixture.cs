using AutoMapper;
using Bogus;
using Career.Application.Requests.Company;
using Career.Application.Responses;
using Career.Application.Services;
using Career.Application.Tests.Common;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.DomainServices;
using Career.Domain.Dtos;
using Career.Domain.Entities;
using Career.Domain.Enums;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Career.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Career.UnitTests.Mocks;
using X.PagedList;

namespace Career.Application.Tests.Services
{
    public class CompanyServiceTestFixture
    {
        // Mocks
        public Mock<ILogger<CompanyService>> MockLogger { get; }
        public Mock<IUnitOfWork> MockUnitOfWork { get; }
        public Mock<IProfileServiceClient> MockProfileService { get; }
        public Mock<IRequestService> MockRequestService { get; }
        public IMapper Mapper { get; }
        public Mock<ICompanyDomainService> MockCompanyDomainService { get; }
        public Mock<IRealTimeNotifier> MockRealTimeNotifier { get; }
        public Mock<IEmailService> MockEmailService { get; }

        // Repository Mocks
        public Mock<ICompanyRepository> MockCompanyRepository { get; }
        public Mock<IStaffRepository> MockStaffRepository { get; }
        public Mock<IGenericRepository> MockGenericRepository { get; }

        // Fakers
        private readonly Faker _faker;

        public CompanyServiceTestFixture()
        {
            _faker = new Faker();

            // Initialize mocks
            MockLogger = new Mock<ILogger<CompanyService>>();
            MockUnitOfWork = new Mock<IUnitOfWork>();
            MockProfileService = new Mock<IProfileServiceClient>();
            MockRequestService = new Mock<IRequestService>();
            MockCompanyDomainService = new Mock<ICompanyDomainService>();
            MockRealTimeNotifier = new Mock<IRealTimeNotifier>();
            MockEmailService = new Mock<IEmailService>();

            // Initialize repository mocks
            MockCompanyRepository = new Mock<ICompanyRepository>();
            MockStaffRepository = new Mock<IStaffRepository>();
            MockGenericRepository = new Mock<IGenericRepository>();

            // Setup UnitOfWork
            MockUnitOfWork.Setup(x => x.CompanyRepository).Returns(MockCompanyRepository.Object);
            MockUnitOfWork.Setup(x => x.StaffRepository).Returns(MockStaffRepository.Object);
            MockUnitOfWork.Setup(x => x.GenericRepository).Returns(MockGenericRepository.Object);

            // Setup RequestService
            MockRequestService.Setup(x => x.GetBearerToken()).Returns("Bearer test-token");

            // Initialize AutoMapper
            Mapper = MapperMock.GetMock();
        }

        public CompanyService CreateService()
        {
            return new CompanyService(
                MockUnitOfWork.Object,
                MockLogger.Object,
                MockProfileService.Object,
                MockRequestService.Object,
                Mapper,
                MockCompanyDomainService.Object,
                MockRealTimeNotifier.Object,
                MockEmailService.Object
            );
        }

        #region Request Helpers

        public CreateCompanyRequest CreateValidCompanyRequest()
        {
            return new CreateCompanyRequest
            {
                Name = _faker.Company.CompanyName(),
                Description = _faker.Lorem.Paragraph(),
                Website = _faker.Internet.Url(),
                CompanyLocation = new CompanyLocationRequest
                {
                    Country = _faker.Address.Country(),
                    State = _faker.Address.State(),
                    ZipCode = _faker.Address.ZipCode()
                }
            };
        }

        public CreateCompanyRequest CreateInvalidCompanyRequest()
        {
            return new CreateCompanyRequest
            {
                Name = "", // Invalid: empty name
                Description = "",
                Website = "",
                CompanyLocation = null
            };
        }

        public UpdateCompanyRequest CreateUpdateCompanyRequest(Guid companyId)
        {
            var fileMock = new Mock<IFormFile>();
            var content = "fake image content";
            var fileName = "logo.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            return new UpdateCompanyRequest
            {
                CompanyId = companyId,
                Name = _faker.Company.CompanyName(),
                Description = _faker.Lorem.Paragraph(),
                Website = _faker.Internet.Url(),
                Logo = fileMock.Object,
                CompanyLocation = new CompanyLocationRequest
                {
                    Country = _faker.Address.Country(),
                    State = _faker.Address.State(),
                    ZipCode = _faker.Address.ZipCode()
                }
            };
        }

        public CompaniesFilterRequest CreateFilterRequest()
        {
            return new CompaniesFilterRequest
            {
                Name = _faker.Company.CompanyName(),
                Verified = _faker.Random.Bool(),
                Rate = _faker.PickRandom<ECompanyRate>(),
                Country = _faker.Address.Country(),
                Page = 1,
                PerPage = 10
            };
        }

        public CompanyConfigurationRequest CreateCompanyConfigurationRequest()
        {
            return new CompanyConfigurationRequest
            {
                IsPrivate = _faker.Random.Bool(),
                ShowStaffs = _faker.Random.Bool(),
                HighlightVerifiedStatus = _faker.Random.Bool(),
                NotifyStaffsOnNewReview = _faker.Random.Bool(),
                NotifyStaffsOnNewJobApplication = _faker.Random.Bool(),
                NotifyStaffsOnJobApplicationUpdate = _faker.Random.Bool()
            };
        }

        #endregion

        #region Paginated Helpers

        public IPagedList<Company> CreatePaginatedCompanies(List<Company> companies)
        {
            return new StaticPagedList<Company>(
                companies,
                1,
                10,
                companies.Count
            );
        }

        #endregion

        #region Company With Jobs Helper

        public Company CreateCompanyWithJobs(string ownerId, int jobCount = 2)
        {
            var company = CommonTestFakers.CreateCompany(ownerId);
            var jobs = new List<Job>();

            for (int i = 0; i < jobCount; i++)
            {
                jobs.Add(new Job
                {
                    Id = Guid.NewGuid(),
                    CompanyId = company.Id,
                    Title = _faker.Name.JobTitle(),
                    Description = _faker.Lorem.Paragraph(),
                    PostedAt = DateTime.UtcNow,
                    IsActive = true,
                    LocalType = _faker.PickRandom<ELocalType>(),
                    Salary = CommonTestFakers.CreateSalary(),
                    JobRequirements = new List<JobRequirement>()
                });
            }

            company.Jobs = jobs;
            return company;
        }

        #endregion
    }
}
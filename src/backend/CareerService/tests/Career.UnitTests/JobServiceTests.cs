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
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Career.Domain.Services.Messaging;
using Career.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Career.Application.Tests.Services
{
    public class JobServiceTests
    {
        private readonly JobServiceTestFixture _fixture;

        public JobServiceTests()
        {
            _fixture = new JobServiceTestFixture();
        }

        #region CreateJob Tests

        [Fact]
        public async Task CreateJob_WithValidRequest_ShouldCreateJobSuccessfully()
        {
            // Arrange
            var request = _fixture.CreateValidJobRequest();
            var company = _fixture.CreateCompany(isOwner: true);
            var userInfo = _fixture.CreateUserInfo(company.OwnerId);
            var job = _fixture.CreateJob(request);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<Job>()))
                .Returns(Task.CompletedTask);

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            // AutoMapper will handle the mapping automatically

            _fixture.MockJobProducer
                .Setup(x => x.SendJobCreated(It.IsAny<JobCreatedDto>()))
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            var result = await service.CreateJob(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.CompanyId, result.CompanyId);


            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<Job>()), Times.Once);
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Once);
            _fixture.MockJobProducer.Verify(x => x.SendJobCreated(It.IsAny<JobCreatedDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateJob_WithInvalidRequest_ShouldThrowRequestException()
        {
            // Arrange
            var request = _fixture.CreateInvalidJobRequest();
            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<RequestException>(async () => await service.CreateJob(request));

            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<Job>()), Times.Never);
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public async Task CreateJob_WithNonExistentCompany_ShouldThrowNullEntityException()
        {
            // Arrange
            var request = _fixture.CreateValidJobRequest();

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync((Company)null!);

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullEntityException>(() => service.CreateJob(request));
            Assert.Equal("Company by id was not found", exception.Message);
        }

        [Fact]
        public async Task CreateJob_WithStaffMember_ShouldVerifyStaffRolesAndCreateJob()
        {
            // Arrange
            var request = _fixture.CreateValidJobRequest();
            var company = _fixture.CreateCompany(isOwner: false);
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var staff = _fixture.CreateStaff(userInfo.id, company.Id);
            var staffRoles = _fixture.CreateStaffRoles();
            var job = _fixture.CreateJob(request);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffByUserIdAndCompany(userInfo.id, request.CompanyId))
                .ReturnsAsync(staff);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffRolesInCompany(company.Id, staff.Id))
                .ReturnsAsync(staffRoles);

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<Job>()))
                .Returns(Task.CompletedTask);

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            // AutoMapper will handle mapping automatically

            _fixture.MockJobProducer
                .Setup(x => x.SendJobCreated(It.IsAny<JobCreatedDto>()))
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            var result = await service.CreateJob(request);

            // Assert
            Assert.NotNull(result);
            _fixture.MockStaffRepository.Verify(x => x.GetStaffByUserIdAndCompany(userInfo.id, request.CompanyId), Times.Once);
            _fixture.MockStaffRepository.Verify(x => x.GetStaffRolesInCompany(company.Id, staff.Id), Times.Once);
        }

        [Fact]
        public async Task CreateJob_WithStaffMemberNotAssignedToCompany_ShouldThrowDomainException()
        {
            // Arrange
            var request = _fixture.CreateValidJobRequest();
            var company = _fixture.CreateCompany(isOwner: false);
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffByUserIdAndCompany(userInfo.id, request.CompanyId))
                .ReturnsAsync((Staff)null!);

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateJob(request));
            Assert.Equal("User is not assigned to this company", exception.Message);
        }

        [Fact]
        public async Task CreateJob_WithStaffMemberWithoutRoles_ShouldThrowNullEntityException()
        {
            // Arrange
            var request = _fixture.CreateValidJobRequest();
            var company = _fixture.CreateCompany(isOwner: false);
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var staff = _fixture.CreateStaff(userInfo.id, company.Id);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffByUserIdAndCompany(userInfo.id, request.CompanyId))
                .ReturnsAsync(staff);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffRolesInCompany(company.Id, staff.Id))
                .ReturnsAsync((List<StaffRole>)null!);

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullEntityException>(() => service.CreateJob(request));
            Assert.Equal("It was not possible verify staff roles", exception.Message);
        }

        #endregion

        #region ApplyToJob Tests

        [Fact]
        public async Task ApplyToJob_WithValidPdfResume_ShouldCreateApplicationSuccessfully()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var request = _fixture.CreateApplyToJobRequest(isPdf: true);
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var profile = _fixture.CreateProfile();
            var job = _fixture.CreateJobWithCompany(jobId);
            var hiringManagers = _fixture.CreateHiringManagers(2);

            _fixture.MockFileService
                .Setup(x => x.IsFileAsPdfOrTxt(It.IsAny<Stream>()))
                .Returns((".pdf", true));

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockProfileService
                .Setup(x => x.GetProfilesInfoByUser(userInfo.id))
                .ReturnsAsync(profile);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync(job);

            _fixture.MockStaffRepository
                .Setup(x => x.GetHiringManagersFromCompany(job.CompanyId))
                .ReturnsAsync(hiringManagers);

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<JobApplication>()))
                .Returns(Task.CompletedTask);

            _fixture.MockGenericRepository
                .Setup(x => x.AddRange(It.IsAny<List<Notification>>()))
                .Returns(Task.CompletedTask);

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            // AutoMapper will handle mappings automatically

            _fixture.MockRealTimeNotifier
                .Setup(x => x.SendInformationNotificationManyUsers(It.IsAny<InformationNotificationManyUsersDto>()))
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            var result = await service.ApplyToJob(request, jobId);

            // Assert
            Assert.NotNull(result);
            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<JobApplication>()), Times.Once);
            _fixture.MockGenericRepository.Verify(x => x.AddRange(It.IsAny<List<Notification>>()), Times.Once);
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Once);
            _fixture.MockRealTimeNotifier.Verify(x => x.SendInformationNotificationManyUsers(It.IsAny<InformationNotificationManyUsersDto>()), Times.Once);
        }

        [Fact]
        public async Task ApplyToJob_WithInvalidFileFormat_ShouldThrowRequestException()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var request = _fixture.CreateApplyToJobRequest(isPdf: false);

            _fixture.MockFileService
                .Setup(x => x.IsFileAsPdfOrTxt(It.IsAny<Stream>()))
                .Returns((".doc", false));

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RequestException>(() => service.ApplyToJob(request, jobId));
            Assert.Equal("Invalid file format, it must be txt or pdf type", exception.Message);
        }

        [Fact]
        public async Task ApplyToJob_WithNonExistentJob_ShouldThrowNullEntityException()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var request = _fixture.CreateApplyToJobRequest(isPdf: true);
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var profile = _fixture.CreateProfile();

            _fixture.MockFileService
                .Setup(x => x.IsFileAsPdfOrTxt(It.IsAny<Stream>()))
                .Returns((".pdf", true));

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockProfileService
                .Setup(x => x.GetProfilesInfoByUser(userInfo.id))
                .ReturnsAsync(profile);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync((Job)null!);

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullEntityException>(() => service.ApplyToJob(request, jobId));
            Assert.Equal("The job was not found", exception.Message);
        }

        [Fact]
        public async Task ApplyToJob_ShouldCreateNotificationsForHiringManagersAndOwner()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var request = _fixture.CreateApplyToJobRequest(isPdf: true);
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var profile = _fixture.CreateProfile();
            var job = _fixture.CreateJobWithCompany(jobId);
            var hiringManagers = _fixture.CreateHiringManagers(3);
            List<Notification> capturedNotifications = null!;

            _fixture.MockFileService
                .Setup(x => x.IsFileAsPdfOrTxt(It.IsAny<Stream>()))
                .Returns((".pdf", true));

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockProfileService
                .Setup(x => x.GetProfilesInfoByUser(userInfo.id))
                .ReturnsAsync(profile);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync(job);

            _fixture.MockStaffRepository
                .Setup(x => x.GetHiringManagersFromCompany(job.CompanyId))
                .ReturnsAsync(hiringManagers);

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<JobApplication>()))
                .Returns(Task.CompletedTask);

            _fixture.MockGenericRepository
                .Setup(x => x.AddRange(It.IsAny<List<Notification>>()))
                .Callback<List<Notification>>(notifications => capturedNotifications = notifications)
                .Returns(Task.CompletedTask);

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            // AutoMapper will handle mappings automatically

            _fixture.MockRealTimeNotifier
                .Setup(x => x.SendInformationNotificationManyUsers(It.IsAny<InformationNotificationManyUsersDto>()))
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            await service.ApplyToJob(request, jobId);

            // Assert
            Assert.NotNull(capturedNotifications);
            Assert.Equal(4, capturedNotifications.Count); // 3 hiring managers + 1 owner
            Assert.Contains(capturedNotifications, n => n.UserId == job.Company.OwnerId);
        }

        #endregion

        #region GetJobApplications Tests

        [Fact]
        public async Task GetJobApplications_WithValidPermissions_ShouldReturnPaginatedApplications()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var perPage = 10;
            var page = 1;
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var job = _fixture.CreateJobWithCompany(jobId);
            var applications = _fixture.CreateJobApplicationsList(5);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync(job);

            _fixture.MockCompanyDomainService
                .Setup(x => x.CanUserHandleHiringManagement(job.Company, userInfo.id))
                .ReturnsAsync(true);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobApplicationsPaginated(jobId, perPage, page))
                .Returns(_fixture.CreatePaginatedApplications(applications));

            // AutoMapper will handle mappings automatically

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetJobApplications(jobId, perPage, page);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.JobApplications.Count);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task GetJobApplications_WithNonExistentJob_ShouldThrowNullEntityException()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var perPage = 10;
            var page = 1;
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync((Job)null!);

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullEntityException>(() => service.GetJobApplications(jobId, perPage, page));
            Assert.Equal("Job was not found", exception.Message);
        }

        [Fact]
        public async Task GetJobApplications_WithoutPermissions_ShouldThrowDomainException()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var perPage = 10;
            var page = 1;
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var job = _fixture.CreateJobWithCompany(jobId);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync(job);

            _fixture.MockCompanyDomainService
                .Setup(x => x.CanUserHandleHiringManagement(job.Company, userInfo.id))
                .ReturnsAsync(false);

            var service = _fixture.CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetJobApplications(jobId, perPage, page));
            Assert.Equal("User doesn't has permission for get job applications", exception.Message);
        }

        [Fact]
        public async Task GetJobApplications_ShouldCountApplicationStatusesCorrectly()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var perPage = 10;
            var page = 1;
            var userInfo = _fixture.CreateUserInfo(Guid.NewGuid().ToString());
            var job = _fixture.CreateJobWithCompany(jobId);

            var applications = new List<JobApplication>
            {
                _fixture.CreateJobApplicationWithStatus(EApplicationStatus.Applied),
                _fixture.CreateJobApplicationWithStatus(EApplicationStatus.Applied),
                _fixture.CreateJobApplicationWithStatus(EApplicationStatus.Interview),
                _fixture.CreateJobApplicationWithStatus(EApplicationStatus.Interview),
                _fixture.CreateJobApplicationWithStatus(EApplicationStatus.Rejected)
            };

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobById(jobId))
                .ReturnsAsync(job);

            _fixture.MockCompanyDomainService
                .Setup(x => x.CanUserHandleHiringManagement(job.Company, userInfo.id))
                .ReturnsAsync(true);

            _fixture.MockJobRepository
                .Setup(x => x.GetJobApplicationsPaginated(jobId, perPage, page))
                .Returns(_fixture.CreatePaginatedApplications(applications));

            // AutoMapper will handle mappings automatically

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetJobApplications(jobId, perPage, page);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalInterview);
            Assert.Equal(1, result.TotalRejected);
            Assert.Equal(2, result.TotalApplied);
        }

        #endregion
    }
}
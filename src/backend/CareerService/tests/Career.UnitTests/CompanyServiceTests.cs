using Career.Application.Responses;
using Career.Application.Services;
using Career.Application.Tests.Common;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Entities;
using Career.Domain.Exceptions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Career.Domain.Dtos;
using Xunit;

namespace Career.Application.Tests.Services
{
    public class CompanyServiceTests
    {
        private readonly CompanyServiceTestFixture _fixture;

        public CompanyServiceTests()
        {
            _fixture = new CompanyServiceTestFixture();
        }

        #region CreateCompany Tests

        [Fact]
        public async Task CreateCompany_WithValidRequest_ShouldCreateCompanySuccessfully()
        {
            // Arrange
            var request = _fixture.CreateValidCompanyRequest();
            var userId = Guid.NewGuid().ToString();
            var userInfo = CommonTestFakers.CreateUserInfo(userId);
            var userRoles = CommonTestFakers.CreateUserRoles(userId, "company_owner");

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockProfileService
                .Setup(x => x.GetUserRoles(It.IsAny<string>()))
                .ReturnsAsync(userRoles);

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<Company>()))
                .Returns(Task.CompletedTask);

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<CompanyConfiguration>()))
                .Returns(Task.CompletedTask);

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            var result = await service.CreateCompany(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
            
            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<Company>()), Times.Once);
            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<CompanyConfiguration>()), Times.Once);
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateCompany_WithoutCompanyOwnerRole_ShouldThrowDomainException()
        {
            // Arrange
            var request = _fixture.CreateValidCompanyRequest();
            var userId = Guid.NewGuid().ToString();
            var userInfo = CommonTestFakers.CreateUserInfo(userId);
            var userRoles = CommonTestFakers.CreateUserRoles(userId, "regular_user");

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockProfileService
                .Setup(x => x.GetUserRoles(It.IsAny<string>()))
                .ReturnsAsync(userRoles);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => service.CreateCompany(request));
            
            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<Company>()), Times.Never);
        }

        #endregion

        #region UpdateCompany Tests

        [Fact]
        public async Task UpdateCompany_WithValidRequest_ShouldUpdateSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(userId);
            var request = _fixture.CreateUpdateCompanyRequest(company.Id);
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync(company);

            _fixture.MockGenericRepository
                .Setup(x => x.Update(It.IsAny<Company>()));

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            var result = await service.UpdateCompany(request);

            // Assert
            Assert.NotNull(result);
            _fixture.MockGenericRepository.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task UpdateCompany_WithNonExistentCompany_ShouldThrowNullEntityException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = _fixture.CreateUpdateCompanyRequest(Guid.NewGuid());
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync((Company)null);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NullEntityException>(() => service.UpdateCompany(request));
        }

        [Fact]
        public async Task UpdateCompany_WithNonOwner_ShouldThrowDomainException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var request = _fixture.CreateUpdateCompanyRequest(company.Id);
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(request.CompanyId))
                .ReturnsAsync(company);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => service.UpdateCompany(request));
        }

        #endregion

        #region GetCompany Tests

        [Fact]
        public async Task GetCompany_WithPublicCompany_ShouldReturnCompany()
        {
            // Arrange
            var company = CommonTestFakers.CreateCompany();
            company.CompanyConfiguration.IsPrivate = false;

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompany(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(company.Id, result.Id);
            Assert.Equal(company.Name, result.Name);
        }

        [Fact]
        public async Task GetCompany_WithPrivateCompanyAndOwner_ShouldReturnFullDetails()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(userId);
            company.CompanyConfiguration.IsPrivate = true;
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompany(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(company.Id, result.Id);
        }

        [Fact]
        public async Task GetCompany_WithPrivateCompanyAndStaffMember_ShouldReturnFilteredDetails()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            company.CompanyConfiguration.IsPrivate = true;
            company.CompanyConfiguration.ShowStaffs = false;
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyContainsStaff(company.Id, userId))
                .ReturnsAsync(true);

            _fixture.MockCompanyDomainService
                .Setup(x => x.GetCompanyResponseByConfigurations(company.CompanyConfiguration, company))
                .Returns(new CompanyResponseDto()
                {
                    Id = company.Id,
                    Name = company.Name,
                    CreatedAt = company.CreatedAt,
                    Description = company.Description,
                    Location = company.Location,
                    Logo = company.Logo,
                    Rate = company.Rate,
                    Verified = company.Verified,
                    Website = company.Website,
                });

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompany(company.Id);

            // Assert
            Assert.NotNull(result);
            _fixture.MockCompanyDomainService.Verify(
                x => x.GetCompanyResponseByConfigurations(company.CompanyConfiguration, company), 
                Times.Once
            );
        }

        [Fact]
        public async Task GetCompany_WithNonExistentCompany_ShouldThrowNullEntityException()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(companyId))
                .ReturnsAsync((Company)null);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NullEntityException>(() => service.GetCompany(companyId));
        }

        #endregion

        #region GetCompanyFiltered Tests

        [Fact]
        public async Task GetCompanyFiltered_WithValidRequest_ShouldReturnPaginatedCompanies()
        {
            // Arrange
            var request = _fixture.CreateFilterRequest();
            var companies = new[] { 
                CommonTestFakers.CreateCompany(), 
                CommonTestFakers.CreateCompany() 
            }.ToList();

            _fixture.MockCompanyRepository
                .Setup(x => x.GetCompaniesPaginated(It.IsAny<Domain.Dtos.CompanyFilterDto>()))
                .Returns(_fixture.CreatePaginatedCompanies(companies));

            _fixture.MockCompanyDomainService
                .Setup(x => x.GetCompanyResponseByConfigurations(It.IsAny<CompanyConfiguration>(), It.IsAny<Company>()))
                .Returns((CompanyConfiguration config, Company comp) => comp);

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompanyFiltered(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Companies.Count);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCompanyFiltered_WithPerPageOver100_ShouldThrowRequestException()
        {
            // Arrange
            var request = _fixture.CreateFilterRequest();
            request.PerPage = 150;

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<RequestException>(() => service.GetCompanyFiltered(request));
        }

        #endregion

        #region FireStaffFromCompany Tests

        [Fact]
        public async Task FireStaffFromCompany_WithValidRequest_ShouldRemoveStaffAndNotify()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var staffUserId = Guid.NewGuid().ToString();
            var staff = CommonTestFakers.CreateStaff(staffUserId, company.Id);
            var ownerInfo = CommonTestFakers.CreateUserInfo(ownerId);
            var reason = "Performance issues";

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(ownerInfo);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffByIdAndCompany(staff.Id, company.Id))
                .ReturnsAsync(staff);

            _fixture.MockGenericRepository
                .Setup(x => x.Remove(It.IsAny<Staff>()));

            _fixture.MockGenericRepository
                .Setup(x => x.Add(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            _fixture.MockRealTimeNotifier
                .Setup(x => x.SendNotification(It.IsAny<Domain.Dtos.Notifications.SendNotificationDto>()))
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            await service.FireStaffFromCompany(company.Id, staff.Id, reason);

            // Assert
            _fixture.MockGenericRepository.Verify(x => x.Remove(It.IsAny<Staff>()), Times.Once);
            _fixture.MockGenericRepository.Verify(x => x.Add(It.IsAny<Notification>()), Times.Once);
            _fixture.MockRealTimeNotifier.Verify(
                x => x.SendNotification(It.IsAny<Domain.Dtos.Notifications.SendNotificationDto>()), 
                Times.Once
            );
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task FireStaffFromCompany_WithNonOwner_ShouldThrowDomainException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var staffId = Guid.NewGuid();
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => service.FireStaffFromCompany(company.Id, staffId, "reason"));
        }

        [Fact]
        public async Task FireStaffFromCompany_WithNonExistentStaff_ShouldThrowNullEntityException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var staffId = Guid.NewGuid();
            var ownerInfo = CommonTestFakers.CreateUserInfo(ownerId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(ownerInfo);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffByIdAndCompany(staffId, company.Id))
                .ReturnsAsync((Staff)null);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NullEntityException>(() => service.FireStaffFromCompany(company.Id, staffId, "reason"));
        }

        #endregion

        #region GetCompanyStaffs Tests

        [Fact]
        public async Task GetCompanyStaffs_WithPublicShowStaffs_ShouldReturnAllStaffs()
        {
            // Arrange
            var company = CommonTestFakers.CreateCompany();
            company.CompanyConfiguration.ShowStaffs = true;
            var staffs = CommonTestFakers.CreateHiringManagers(3, company.Id);
            var staffRoles = staffs.Select(s => s.StaffRole).ToList();

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockStaffRepository
                .Setup(x => x.GetAllStaffsFromACompany(company.Id))
                .ReturnsAsync(staffs);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffsRole(It.IsAny<System.Collections.Generic.List<Guid>>()))
                .ReturnsAsync(staffRoles);

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompanyStaffs(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(company.Id, result.CompanyId);
            Assert.Equal(3, result.Staffs.Count);
        }

        [Fact]
        public async Task GetCompanyStaffs_WithPrivateShowStaffsAndOwner_ShouldReturnStaffs()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            company.CompanyConfiguration.ShowStaffs = false;
            var userInfo = CommonTestFakers.CreateUserInfo(ownerId);
            var staffs = CommonTestFakers.CreateHiringManagers(2, company.Id);
            var staffRoles = staffs.Select(s => s.StaffRole).ToList();

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockStaffRepository
                .Setup(x => x.GetAllStaffsFromACompany(company.Id))
                .ReturnsAsync(staffs);

            _fixture.MockStaffRepository
                .Setup(x => x.GetStaffsRole(It.IsAny<System.Collections.Generic.List<Guid>>()))
                .ReturnsAsync(staffRoles);

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompanyStaffs(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Staffs.Count);
        }

        [Fact]
        public async Task GetCompanyStaffs_WithPrivateShowStaffsAndUnauthorized_ShouldThrowDomainException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            company.CompanyConfiguration.ShowStaffs = false;
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyContainsStaff(company.Id, userId))
                .ReturnsAsync(false);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => service.GetCompanyStaffs(company.Id));
        }

        [Fact]
        public async Task GetCompanyStaffs_WithNonExistentCompany_ShouldThrowNullEntityException()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(companyId))
                .ReturnsAsync((Company)null);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NullEntityException>(() => service.GetCompanyStaffs(companyId));
        }

        #endregion

        #region GetCompanyConfigurationInfos Tests

        [Fact]
        public async Task GetCompanyConfigurationInfos_WithOwner_ShouldReturnConfiguration()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var userInfo = CommonTestFakers.CreateUserInfo(ownerId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            var service = _fixture.CreateService();

            // Act
            var result = await service.GetCompanyConfigurationInfos(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(company.Id, result.CompanyId);
        }

        [Fact]
        public async Task GetCompanyConfigurationInfos_WithNonOwner_ShouldThrowDomainException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => service.GetCompanyConfigurationInfos(company.Id));
        }

        #endregion

        #region UpdateCompanyConfiguration Tests

        [Fact]
        public async Task UpdateCompanyConfiguration_WithValidRequest_ShouldUpdateSuccessfully()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var request = _fixture.CreateCompanyConfigurationRequest();
            var userInfo = CommonTestFakers.CreateUserInfo(ownerId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockGenericRepository
                .Setup(x => x.Update(It.IsAny<Company>()));

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            var result = await service.UpdateCompanyConfiguration(company.Id, request);

            // Assert
            Assert.NotNull(result);
            _fixture.MockGenericRepository.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task UpdateCompanyConfiguration_WithNonOwner_ShouldThrowDomainException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = CommonTestFakers.CreateCompany(ownerId);
            var request = _fixture.CreateCompanyConfigurationRequest();
            var userInfo = CommonTestFakers.CreateUserInfo(userId);

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyById(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => service.UpdateCompanyConfiguration(company.Id, request));
        }

        #endregion

        #region DeleteCompany Tests

        [Fact]
        public async Task DeleteCompany_WithOwner_ShouldDeactivateCompanyAndJobs()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var company = _fixture.CreateCompanyWithJobs(ownerId, 3);
            var userInfo = CommonTestFakers.CreateUserInfo(ownerId);
            var reactivateUrl = "https://example.com/reactivate";

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyByIdWithJobs(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            _fixture.MockGenericRepository
                .Setup(x => x.Update(It.IsAny<Company>()));

            _fixture.MockUnitOfWork
                .Setup(x => x.Commit())
                .Returns(Task.CompletedTask);

            _fixture.MockEmailService
                .Setup(x => x.SendEmailCompanyDeleted(
                    company.Name, 
                    userInfo.userName, 
                    userInfo.email, 
                    reactivateUrl, 
                    It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);

            var service = _fixture.CreateService();

            // Act
            await service.DeleteCompany(company.Id, reactivateUrl);

            // Assert
            Assert.False(company.IsActive);
            Assert.All(company.Jobs, job => Assert.False(job.IsActive));
            
            _fixture.MockGenericRepository.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
            _fixture.MockEmailService.Verify(
                x => x.SendEmailCompanyDeleted(
                    company.Name, 
                    userInfo.userName, 
                    userInfo.email, 
                    reactivateUrl, 
                    It.IsAny<DateTime>()), 
                Times.Once
            );
            _fixture.MockUnitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task DeleteCompany_WithNonExistentCompany_ShouldThrowNullEntityException()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var reactivateUrl = "https://example.com/reactivate";

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyByIdWithJobs(companyId))
                .ReturnsAsync((Company)null);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NullEntityException>(() => service.DeleteCompany(companyId, reactivateUrl));
        }

        [Fact]
        public async Task DeleteCompany_WithNonOwner_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var ownerId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var company = _fixture.CreateCompanyWithJobs(ownerId);
            var userInfo = CommonTestFakers.CreateUserInfo(userId);
            var reactivateUrl = "https://example.com/reactivate";

            _fixture.MockCompanyRepository
                .Setup(x => x.CompanyByIdWithJobs(company.Id))
                .ReturnsAsync(company);

            _fixture.MockProfileService
                .Setup(x => x.GetUserInfos(It.IsAny<string>()))
                .ReturnsAsync(userInfo);

            var service = _fixture.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteCompany(company.Id, reactivateUrl));
        }

        #endregion
    }
}
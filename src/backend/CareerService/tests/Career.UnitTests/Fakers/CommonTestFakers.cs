using Bogus;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Dtos;
using Career.Domain.Entities;
using Career.Domain.Enums;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using Career.Domain.Aggregates.JobRoot;

namespace Career.Application.Tests.Common
{
    /// <summary>
    /// Shared Bogus fakers for reuse across all test fixtures
    /// </summary>
    public static class CommonTestFakers
    {
        private static readonly Faker _faker = new Faker();

        #region User & Profile Fakers

        public static UserInfosDto CreateUserInfo(string userId = null)
        {
            return new UserInfosDto
            {
                id = userId ?? Guid.NewGuid().ToString(),
                userName = _faker.Internet.UserName(),
                email = _faker.Internet.Email(),
                createdAt = DateTime.UtcNow
            };
        }

        public static ProfileInfosDto CreateProfile()
        {
            return new ProfileInfosDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
                IsPrivate = false
            };
        }

        public static UserRolesDto CreateUserRoles(string userId, params string[] roleNames)
        {
            var roles = new List<UserRoleDto>();
            
            if (roleNames.Length == 0)
            {
                roleNames = new[] { "company_owner" };
            }

            foreach (var roleName in roleNames)
            {
                roles.Add(new UserRoleDto
                {
                    userId = userId,
                    roleName = roleName
                });
            }

            return new UserRolesDto
            {
                userId = userId,
                roles = roles
            };
        }

        #endregion

        #region Location Faker

        public static Location CreateLocation()
        {
            return new Location(
                _faker.Address.Country(),
                _faker.Address.State(),
                _faker.Address.ZipCode()
            );
        }

        #endregion

        #region Company Fakers
        public static Company CreateCompany(string ownerId = null, bool isActive = true)
        {
            var companyId = Guid.NewGuid();
            var ownerIdValue = ownerId ?? Guid.NewGuid().ToString();

            return new Company
            {
                Id = companyId,
                Name = _faker.Company.CompanyName(),
                OwnerId = ownerIdValue,
                Logo = _faker.Image.PicsumUrl(),
                Location = CreateLocation(),
                Description = _faker.Lorem.Paragraph(),
                Verified = _faker.Random.Bool(),
                Website = _faker.Internet.Url(),
                CreatedAt = DateTime.UtcNow,
                Rate = _faker.Random.Decimal(0, 5),
                IsActive = isActive,
                CompanyConfigurationId = Guid.NewGuid(),
                CompanyConfiguration = CreateCompanyConfiguration(companyId),
                Staffs = new List<Staff>(),
                Jobs = new List<Job>()
            };
        }

        public static CompanyConfiguration CreateCompanyConfiguration(Guid companyId)
        {
            return new CompanyConfiguration
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                IsPrivate = false,
                ShowStaffs = true,
                HighlightVerifiedStatus = true,
                NotifyStaffsOnNewReview = false,
                NotifyStaffsOnNewJobApplication = false,
                NotifyStaffsOnJobApplicationUpdate = false
            };
        }

        #endregion

        #region Staff Fakers

        public static Staff CreateStaff(string userId, Guid companyId, string role = null)
        {
            var staffId = Guid.NewGuid();
            return new Staff
            {
                Id = staffId,
                UserId = userId,
                CompanyId = companyId,
                StaffRole = new StaffRole
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    CompanyId = companyId,
                    Role = role ?? StaffRolesConsts.HiringManagers
                }
            };
        }

        public static List<Staff> CreateHiringManagers(int count, Guid? companyId = null)
        {
            var managers = new List<Staff>();
            var companyIdValue = companyId ?? Guid.NewGuid();

            for (int i = 0; i < count; i++)
            {
                managers.Add(CreateStaff(
                    Guid.NewGuid().ToString(), 
                    companyIdValue, 
                    StaffRolesConsts.HiringManagers
                ));
            }
            return managers;
        }

        public static RequestStaff CreateRequestStaff(
            string userId, 
            Guid companyId, 
            string requesterId, 
            ERequestStaffStatus status = ERequestStaffStatus.PENDING)
        {
            return new RequestStaff
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyId = companyId,
                RequesterId = requesterId,
                Role = StaffRolesConsts.HiringManagers,
                Status = status
            };
        }

        public static List<StaffRole> CreateStaffRoles(Guid staffId, Guid companyId)
        {
            return new List<StaffRole>
            {
                new StaffRole
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    CompanyId = companyId,
                    Role = StaffRolesConsts.HiringManagers
                }
            };
        }

        #endregion

        #region Notification Fakers

        public static Notification CreateNotification(
            string userId, 
            ENotificationType type = ENotificationType.Information,
            string senderId = null)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SenderId = senderId,
                Title = _faker.Lorem.Sentence(),
                Message = _faker.Lorem.Paragraph(),
                Type = type,
                IsRead = false
            };
        }

        public static List<Notification> CreateNotifications(int count, string userId)
        {
            var notifications = new List<Notification>();
            for (int i = 0; i < count; i++)
            {
                notifications.Add(CreateNotification(userId));
            }
            return notifications;
        }

        #endregion

        #region Salary Faker

        public static Salary CreateSalary()
        {
            var minSalary = _faker.Random.Decimal(30000, 50000);
            var maxSalary = _faker.Random.Decimal(minSalary + 1000, 100000);
            
            return new Salary(
                minSalary,
                maxSalary,
                _faker.PickRandom<ECurrency>()
            );
        }

        #endregion
    }
}
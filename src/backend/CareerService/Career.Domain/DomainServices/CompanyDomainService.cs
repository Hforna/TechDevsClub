using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Dtos;
using Career.Domain.Exceptions;
using Career.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.DomainServices
{
    public interface ICompanyDomainService
    {
        public Task<Staff> AddStaffToCompany(Company company, string userId);
        public Task<bool> CanUserHandleHiringManagement(Company company, string userId);
        public CompanyResponseDto GetCompanyResponseByConfigurations(
            CompanyConfiguration configuration, 
            Company company);
    }

    public class CompanyDomainService : ICompanyDomainService
    {
        private readonly IUnitOfWork _uow;

        public CompanyDomainService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Staff> AddStaffToCompany(Company company, string userId)
        {
            var contains = await _uow.CompanyRepository.CompanyContainsStaff(company.Id, userId);

            if (contains)
                throw new DomainException(ResourceExceptMessages.STAFF_IN_COMPANY);

            var staff = new Staff() { CompanyId = company.Id, UserId = userId };

            company.Staffs.Add(staff);

            return staff;
        }

        public async Task<bool> CanUserHandleHiringManagement(Company company, string userId)
        {
            var canHandle = true;
            if (company.OwnerId != userId)
            {
                var staff = await _uow.StaffRepository.GetStaffByUserIdAndCompany(userId, company.Id);

                if (staff is null)
                    canHandle = false;

                var staffRoles = await _uow.StaffRepository.GetStaffRolesInCompany(company.Id, staff.Id);

                if(staffRoles!.Any(d => d.Role == StaffRolesConsts.HiringManagers) == false)
                    canHandle = false;
            }
            return canHandle;
        }

        public CompanyResponseDto GetCompanyResponseByConfigurations(
            CompanyConfiguration configuration, 
            Company company)
        {
            var companyResponse = new CompanyResponseDto();

            companyResponse.Verified = configuration.HighlightVerifiedStatus 
                ? companyResponse.Verified 
                : null;

            return companyResponse;
        }
    }
}

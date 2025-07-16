using Career.Domain.Aggregates.CompanyRoot;
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
        public Task AddStaffToCompany(Company company, string userId);
    }

    public class CompanyDomainService : ICompanyDomainService
    {
        private readonly IUnitOfWork _uow;

        public CompanyDomainService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task AddStaffToCompany(Company company, string userId)
        {
            var contains = await _uow.CompanyRepository.CompanyContainsStaff(company.Id, userId);

            if (contains)
                throw new DomainException(ResourceExceptMessages.STAFF_IN_COMPANY);

            var staff = new Staff() { CompanyId = company.Id, UserId = userId };

            company.Staffs.Add(staff);
        }
    }
}

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
        public Task AddStaff(Company company, Staff staff);
    }

    public class CompanyDomainService : ICompanyDomainService
    {
        private readonly IUnitOfWork _uow;

        public async Task AddStaff(Company company, Staff staff)
        {
            var contains = await _uow.CompanyRepository.CompanyContainsStaff(company.Id, staff);

            if (contains)
                throw new DomainException(ResourceExceptMessages.STAFF_IN_COMPANY);

            company.Staffs.Add(staff);
        }
    }
}

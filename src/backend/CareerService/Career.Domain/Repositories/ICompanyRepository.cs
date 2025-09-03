using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Career.Domain.Repositories
{
    public interface ICompanyRepository
    {
        public Task<bool> CompanyContainsStaff(Guid companyId, string userId);
        public Task<bool> CompanyContainsStaff(Guid companyId, Guid staffId);
        public Task<Company?> CompanyById(Guid companyId);
        public IPagedList<Company> GetCompaniesPaginated(CompanyFilterDto dto);
    }
}

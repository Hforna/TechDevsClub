using Career.Domain.Aggregates.CompanyRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Repositories
{
    public interface ICompanyRepository
    {
        public Task<bool> CompanyContainsStaff(Guid companyId, Staff staff);
    }
}

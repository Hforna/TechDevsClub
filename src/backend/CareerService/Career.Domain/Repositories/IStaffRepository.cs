using Career.Domain.Aggregates.CompanyRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Repositories
{
    public interface IStaffRepository
    {
        public Task<List<StaffRole>?> GetStaffRolesInCompany(Guid companyId, Guid staffId);
        public Task<Staff?> GetStaffByUserIdAndCompany(string userId, Guid companyId);
    }
}

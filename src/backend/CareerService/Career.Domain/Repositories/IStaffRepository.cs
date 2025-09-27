using Career.Domain.Aggregates.CompanyRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Career.Domain.Repositories
{
    public interface IStaffRepository
    {
        public Task<List<StaffRole>?> GetStaffRolesInCompany(Guid companyId, Guid staffId);
        public Task<Staff?> GetStaffByUserIdAndCompany(string userId, Guid companyId);
        public Task<RequestStaff?> GetRequestStaffById(Guid requestStaff);
        public Task<Staff?> GetStaffByIdAndCompany(Guid staffId, Guid companyId);
        public IPagedList<RequestStaff> GetUserStaffRequestsPaged(int perPage, int page, string userId);
        public Task<List<Staff>> GetAllStaffsFromACompany(Guid companyId);
        public Task<List<StaffRole>> GetStaffsRole(List<Guid> staffsId);
        public Task<List<Staff>> GetHiringManagersFromCompany(Guid companyId);
    }
}

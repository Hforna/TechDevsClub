using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace Career.Infrastructure.Persistence
{
    public class StaffRepository : IStaffRepository
    {
        private readonly DataContext _context;

        public StaffRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Staff>> GetAllStaffsFromACompany(Guid companyId)
        {
            return await _context.Staffs.Where(d => d.CompanyId == companyId).ToListAsync();
        }

        public async Task<RequestStaff?> GetRequestStaffById(Guid requestStaff)
        {
            return await _context.RequestsStaffs.SingleOrDefaultAsync(d => d.Id == requestStaff);
        }

        public async Task<Staff?> GetStaffByIdAndCompany(Guid staffId, Guid companyId)
        {
            return await _context.Staffs.SingleOrDefaultAsync(d => d.Id == staffId && d.CompanyId == companyId);
        }

        public async Task<Staff?> GetStaffByUserIdAndCompany(string userId, Guid companyId)
        {
            return await _context.Staffs
                .SingleOrDefaultAsync(d => d.UserId == userId && d.CompanyId == companyId);
        }

        public async Task<List<StaffRole>?> GetStaffRolesInCompany(Guid companyId, Guid staffId)
        {
            return await _context.StaffRoles
                .AsNoTracking()
                .Where(d => d.CompanyId == companyId && d.StaffId == staffId)
                .ToListAsync();
        }

        public async Task<List<StaffRole>> GetStaffsRole(List<Guid> staffsId)
        {
            return await _context.StaffRoles.Where(d => staffsId.Contains(d.StaffId)).ToListAsync();
        }

        public IPagedList<RequestStaff> GetUserStaffRequestsPaged(int perPage, int page, string userId)
        {
            return _context.RequestsStaffs
                .AsNoTracking()
                .Where(d => d.RequesterId == userId)
                .ToPagedList(page, perPage);
        }
    }
}

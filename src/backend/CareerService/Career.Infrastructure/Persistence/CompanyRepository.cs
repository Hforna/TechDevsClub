using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Persistence
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DataContext _context;

        public CompanyRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> CompanyContainsStaff(Guid companyId, Staff staff)
        {
            return await _context.Companies
                .AnyAsync(d => d.Id == companyId 
                        && d.Staffs.Contains(staff));
        }
    }
}

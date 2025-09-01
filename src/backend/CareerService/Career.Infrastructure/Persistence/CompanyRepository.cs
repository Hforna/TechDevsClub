using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Dtos;
using Career.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace Career.Infrastructure.Persistence
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DataContext _context;

        public CompanyRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Company?> CompanyById(Guid companyId)
        {
            return await _context.Companies.Include(d => d.CompanyConfiguration)
                .SingleOrDefaultAsync(d => d.Id == companyId);
        }

        public async Task<bool> CompanyContainsStaff(Guid companyId, string userId)
        {
            return await _context.Companies
                .AnyAsync(d => d.Id == companyId 
                        && d.Staffs.Select(d => d.UserId).Contains(userId));
        }

        public IPagedList<Company> GetCompaniesPaginated(CompanyFilterDto dto)
        {
            var companies = _context.Companies.Include(d => d.CompanyConfiguration).AsNoTracking();

            if (string.IsNullOrEmpty(dto.Name) == false)
                companies = companies.Where(d => d.Name.Contains(dto.Name, StringComparison.CurrentCultureIgnoreCase));
            if (string.IsNullOrEmpty(dto.Country) == false)
                companies = companies.Where(d => d.Location.Country.Equals(dto.Country, StringComparison.CurrentCultureIgnoreCase));
            if (dto.Verified is not null)
                companies = companies.Where(d => d.Verified == dto.Verified);
            if (dto.Rate is not null)
                companies = companies.Where(d => d.Rate >= (int)dto.Rate.Value);

            return companies.OrderByDescending(d => d.CreatedAt).ToPagedList(dto.Page, dto.PerPage);
        }
    }
}

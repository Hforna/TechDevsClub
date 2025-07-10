using Career.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public ICompanyRepository CompanyRepository { get; set; }
        public IGenericRepository GenericRepository { get; set; }

        private readonly DataContext _context;

        public UnitOfWork(ICompanyRepository companyRepository, IGenericRepository genericRepository, DataContext context)
        {
            CompanyRepository = companyRepository;
            GenericRepository = genericRepository;
            _context = context;
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

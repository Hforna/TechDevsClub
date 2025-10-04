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
        public IStaffRepository StaffRepository { get; set; }
        public INotificationRepository NotificationRepository { get; set; }
        public IJobRepository JobRepository { get; set; }

        private readonly DataContext _context;

        public UnitOfWork(ICompanyRepository companyRepository, IGenericRepository genericRepository,
            IStaffRepository staffRepository, INotificationRepository notificationRepository, 
            DataContext dataContext, IJobRepository jobRepository)
        {
            _context = dataContext;
            CompanyRepository = companyRepository;
            GenericRepository = genericRepository;
            StaffRepository = staffRepository;
            NotificationRepository = notificationRepository;
            JobRepository = jobRepository;
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

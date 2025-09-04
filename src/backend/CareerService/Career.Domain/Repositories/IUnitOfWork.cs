using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Repositories
{
    public interface IUnitOfWork
    {
        public ICompanyRepository CompanyRepository { get; set; }
        public IStaffRepository StaffRepository { get; set; }
        public IGenericRepository GenericRepository { get; set; }
        public INotificationRepository NotificationRepository { get; set; }
        public Task Commit();
    }
}

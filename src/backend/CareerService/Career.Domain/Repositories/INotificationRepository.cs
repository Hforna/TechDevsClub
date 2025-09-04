using Career.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Career.Domain.Repositories
{
    public interface INotificationRepository
    {
        public IPagedList<Notification> GetNotificatonsPaginated(int perPage, int page, string userId);
    }
}

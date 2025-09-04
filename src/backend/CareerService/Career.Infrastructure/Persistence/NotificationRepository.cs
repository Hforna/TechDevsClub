using Career.Domain.Entities;
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
    public class NotificationRepository : INotificationRepository
    {
        private readonly DataContext _context;

        public NotificationRepository(DataContext context)
        {
            _context = context;
        }

        public IPagedList<Notification> GetNotificatonsPaginated(int perPage, int page, string userId)
        {
            return _context.Notification.AsNoTracking().Where(d => d.UserId == userId).ToPagedList(page, perPage);
        }
    }
}

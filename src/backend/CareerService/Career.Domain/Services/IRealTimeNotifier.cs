using Career.Domain.Dtos.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services
{
    public interface IRealTimeNotifier
    {
        public Task SendNotification(SendNotificationDto dto);
        public Task SendInformationNotificationManyUsers(InformationNotificationManyUsersDto dto);
    }
}

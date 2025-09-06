using Career.Domain.Dtos;
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
    }
}

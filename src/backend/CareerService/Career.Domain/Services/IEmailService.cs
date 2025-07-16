using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services
{
    public interface IEmailService
    {
        public Task SendEmailToUserBeStaff(string toUserName, string toEmail, string companyName);
    }
}

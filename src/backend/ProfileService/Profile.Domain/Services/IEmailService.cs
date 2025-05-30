using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Services
{
    public interface IEmailService
    {
        public Task SendEmail(string toEmail, string toName, string subject, string body);
        public string EmailConfirmation(string link);
    }
}

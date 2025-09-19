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
        public Task SendBatchEmails(BatchEmailDto emails);
    }

    public sealed record BatchEmailDto
    {
        public BatchEmailDto(List<SimpleEmailDto> emails, string companyName, string companyLocation, string jobUrl, string jobTitle)
        {
            Emails = emails;
            CompanyName = companyName;
            CompanyLocation = companyLocation;
            JobUrl = jobUrl;
            JobTitle = jobTitle;
        }

        public List<SimpleEmailDto> Emails { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLocation { get; set; }
        public string JobUrl { get; set; }
        public string JobTitle { get; set; }
    }

    public sealed record SimpleEmailDto
    {
        public SimpleEmailDto(string email, string userName)
        {
            Email = email;
            UserName = userName;
        }

        public string Email { get; set; }
        public string UserName { get; set; }
    }
}

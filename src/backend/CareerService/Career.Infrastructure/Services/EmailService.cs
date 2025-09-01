using Career.Domain.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Career.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmptSettings _smptSettings;

        public EmailService(IOptions<SmptSettings> smptSettings)
        {
            _smptSettings = smptSettings.Value;
        }

        private string UserRequestToBeStaffSubject(string toUserName)
        {
            return $"Hi {toUserName} you recived a new request to be a staff from a company";
        }

        private string UserRequestToBeStaffBody(string company)
        {
            var body = @$"<!DOCTYPE html>
                <html>
                <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; margin: 0; padding: 20px;"">

                    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);"">
                        <p style=""font-size: 16px;"">You recived a new request for be linked as staff on the company {company}, 
                        look at you notifications for accept the request</p>
                    </div>
                </body>
                </html>";

            return body;
        }

        public async Task SendEmailToUserBeStaff(string toUserName, string toEmail, string companyName)
        {
            var message = new MimeMessage();

            var subject = UserRequestToBeStaffSubject(toUserName);
            var body = UserRequestToBeStaffBody(companyName);

            message.From.Add(new MailboxAddress(_smptSettings.SenderName, _smptSettings.SenderEmail));
            message.To.Add(new MailboxAddress(toUserName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(_smptSettings.Server, _smptSettings.Port, true);

                await client.AuthenticateAsync(_smptSettings.SenderEmail, _smptSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }

    public class SmptSettings
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}

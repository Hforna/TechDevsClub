using Azure.Storage.Blobs.Models;
using Career.Domain.Services;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Career.Infrastructure.Services
{
    public class EmailTemplates
    {
        public static string JobToUserBody(string companyName, string jobTitle, string jobLocation, string jobUrl)
        {
            return $@"<!doctype html>
            <html>
              <head>
                <meta charset=""utf-8"" />
                <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
              </head>
              <body style=""margin:0;padding:0;background-color:#f9fafb;font-family:Arial,Helvetica,sans-serif;color:#111;"">
                <table role=""presentation"" style=""width:100%;border-collapse:collapse;"">
                  <tr>
                    <td align=""center"" style=""padding:24px 12px;"">
                      <table role=""presentation"" style=""max-width:600px;width:100%;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e6e9ee;"">
                        <tr>
                          <td style=""padding:20px 24px 12px;"">
                            <h2 style=""margin:0;font-size:20px;font-weight:600;color:#0f1724;"">New job you might like</h2>
                            <p style=""margin:8px 0 0;font-size:14px;color:#4b5563;"">{companyName} · {jobLocation}</p>
                          </td>
                        </tr>
                        <tr>
                          <td style=""padding:12px 24px;border-top:1px solid #f1f5f9;"">
                            <h3 style=""margin:0;font-size:18px;font-weight:700;color:#0b1220;"">{jobTitle}</h3>
                            <div style=""margin-top:16px;"">
                              <a href=""{jobUrl}"" style=""display:inline-block;padding:12px 20px;border-radius:6px;text-decoration:none;font-weight:600;background:#4f46e5;color:#ffffff;"">View job &amp; apply</a>
                            </div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";
        }
        public static string JobToUserPlainBody(string jobUrl, string jobTitle, string companyName, string location)
        {
            return $@"Hi {{user_name}},

                A new job that might interest you is now available.

                Job: {jobTitle}
                Company: {companyName}
                Location: {location}

                See full details and apply here:
                {jobUrl}
                ";
        }

        public static string UserRequestToBeStaffBody(string company)
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

        public static string UserRequestToBeStaffSubject(string toUserName)
        {
            return $"Hi {toUserName} you recived a new request to be a staff from a company";
        }

        public static string CompanyDeletedSubject(string userName,  string companyName)
        {
            return $"Hi {userName}, Your Company {companyName} Has Been Deactivated — 30 Days to Reactivate";
        }
        public static string CompanyDeletedBody(string userName, string companyName, string activateUrl, DateTime deactivateDate)
        {
            return $@"<tr>
   <td style=""padding:0 28px 20px 28px;border-top:1px solid #eef2f7;"">
      <p style=""margin:16px 0 8px 0;color:#344054;font-size:15px;line-height:1.5;""> Hello <strong>[User Name]</strong>, </p>
      <p style=""margin:0 0 12px 0;color:#344054;font-size:15px;line-height:1.5;""> This is to inform you that your company <strong>{companyName}</strong> was deactivated on {deactivateDate}<strong></strong>. </p>
      <p style=""margin:0 0 16px 0;color:#374151;font-size:15px;line-height:1.5;""> We will retain your company data and settings securely for <strong>30 days</strong>. During this retention period you may reactivate the company and restore full access. </p>
      <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:20px 0;"">
         <tr>
            <td align=""center""> <a href=""{activateUrl}"" style=""display:inline-block;padding:12px 20px;border-radius:6px;text-decoration:none;font-weight:600;font-size:15px;background:#4f46e5;color:#ffffff;""> Reactivate Company </a> </td>
         </tr>
      </table>
      <p style=""margin:0 0 8px 0;color:#475569;font-size:14px;line-height:1.5;""> If you do not reactivate within 30 days, your company and its associated data may be permanently deleted. </p>
   </td>
</tr>
<tr>
   <td style=""padding:16px 28px 20px 28px;border-top:1px solid #eef2f7;background:#fbfcfd;"">
      <p style=""margin:0;color:#6b7280;font-size:13px;line-height:1.4;""> This message was sent because you or an administrator for <strong>{companyName}</strong> requested or triggered deactivation. If you believe this is an error, please contact support. </p>
   </td>
</tr>
</table> </td> </tr> </table>";
        }
    }

    public class EmailService : EmailTemplates, IEmailService
    {
        public readonly SmptSettings _smptSettings;
        public readonly ISendGridClient _gridClient;
        public readonly ILogger<IEmailService> _logger;

        public EmailService(IOptions<SmptSettings> smptSettings, ISendGridClient sendGridClient, ILogger<IEmailService> logger)
        {
            _smptSettings = smptSettings.Value;
            _gridClient = sendGridClient;
            _logger = logger;
        }

        public async Task SendEmailToUserBeStaff(string toUserName, string toEmail, string companyName)
        {
            var message = new MimeMessage();

            var subject = EmailTemplates.UserRequestToBeStaffSubject(toUserName);
            var body = UserRequestToBeStaffBody(companyName);

            message.From.Add(new MailboxAddress(_smptSettings.SenderName, _smptSettings.SenderEmail));
            message.To.Add(new MailboxAddress(toUserName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };

            await SendMessage(message);
        }

        public async Task SendBatchEmailWhenJobCreated(BatchEmailDto emailsInfos)
        {
            if(emailsInfos.Emails is null || !emailsInfos.Emails.Any())
                return;

            var fromEmail = new EmailAddress(_smptSettings.SenderEmail, _smptSettings.Username);

            var emailTasks = emailsInfos.Emails.Select(async info =>
            {
                var toEmail = new EmailAddress(info.Email, info.UserName);
                var subject = $"Hi {info.UserName} we got a job that maybe you can be interested";
                var body = JobToUserBody(emailsInfos.CompanyName, emailsInfos.JobTitle, emailsInfos.CompanyLocation, emailsInfos.JobUrl);
                var plainText = JobToUserPlainBody(emailsInfos.JobUrl, emailsInfos.JobTitle, emailsInfos.CompanyName, emailsInfos.CompanyLocation);

                var message = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, plainText, body);
                var response = await _gridClient.SendEmailAsync(message);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Email couldn't be sent to {info.Email} body: {response.Body} status code: {response.StatusCode}");
                }
            });

            await Task.WhenAll(emailTasks);
        }

        public async Task SendEmailCompanyDeleted(string companyName, string toUserName, string toEmail, string activateUrl, DateTime deactivateDate)
        {
            var message = new MimeMessage();

            var subject = CompanyDeletedSubject(toUserName, companyName);
            var body = CompanyDeletedBody(toUserName, companyName, activateUrl, deactivateDate);

            message.From.Add(new MailboxAddress(_smptSettings.SenderName, _smptSettings.SenderEmail));
            message.To.Add(new MailboxAddress(toUserName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };

            await SendMessage(message);
        }

        private async Task SendMessage(MimeMessage message)
        {
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

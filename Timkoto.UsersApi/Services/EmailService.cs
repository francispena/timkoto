using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService()
        {
            _configuration = Startup.Configuration;
        }

        public async Task<bool> SendRegistrationLink(string emailAddress, string regLink, List<string> messages)
        {
            const string from = "no-reply@timkoto.com";
            const string fromName = "TimKoTo";
            var to = emailAddress;
            var smtpUserName = _configuration["SmtpUserName"];
            var smtpPassword = _configuration["SmtpPassword"];
            const string configSet = "default_config";
            var host = _configuration["SmtpHost"];
            const int port = 587;
            const string subject = "Registration Link";

            var body = "<p>Dear Ma'am/Sir</p>" +
                       "<br>" +
                       "<p>Thank you for your interest in our service.</p>" +
                       "<p>To proceed with your registration, please click on the link below.</p>" +
                       "<br>" +
                       $"<p><a href='{regLink}'>{regLink}</a></p>" +
                       "<br>" +
                       "<p>If the link doesn't work, copy the link and paste it into your browser's address bar.</p>" +
                       "<p>Please do not reply directly to this email. Should you have any questions or clarifications please reach out to your agent.</p>" +
                       "<br>" +
                       "<p>Thank you.</p>" +
                       "<p>Your Team!!!</p>" +
                       "<br>" +
                       "<p>This is a system - generated email.No signature is required.</p>";

            var message = new MailMessage { IsBodyHtml = true, From = new MailAddress(from, fromName) };
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.Headers.Add("X-SES-CONFIGURATION-SET", configSet);

            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                client.EnableSsl = true;

                try
                {
                    await client.SendMailAsync(message);
                }
                catch
                {
                    // ignored
                }
            }

            return true;
        }

        public async Task<bool> SendPasswordResetCode(User user, List<string> messages)
        {
            const string from = "no-reply@timkoto.com";
            const string fromName = "TimKoTo";
            var to = user.Email;
            var smtpUserName = _configuration["SmtpUserName"];
            var smtpPassword = _configuration["SmtpPassword"];
            const string configSet = "default_config";
            var host = _configuration["SmtpHost"];
            const int port = 587;
            const string subject = "Password Reset";

            var body = $"<p>Dear {user.UserName}</p>" +
                       "<br>" +
                       "<p>A request has been received to reset the password for your Timkoto account.</p>" +
                       "<p>Please enter the code below in the password reset page.</p>" +
                       "<br>" +
                       $"<p>{user.PasswordResetCode}</p>" +
                       "<br>" +
                       "<p>If you did not initiate this request, please contact immediately your agent.</p>" +
                       "<br>" +
                       "<p>Your Team!!!</p>" +
                       "<br>" +
                       "<p>This is a system - generated email.No signature is required.</p>";

            var message = new MailMessage { IsBodyHtml = true, From = new MailAddress(from, fromName) };
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.Headers.Add("X-SES-CONFIGURATION-SET", configSet);

            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                client.EnableSsl = true;

                try
                {
                    await client.SendMailAsync(message);
                }
                catch
                {
                    // ignored
                }
            }

            return true;
        }
    }
}
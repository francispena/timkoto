using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
            
            var body =
                "<h1>Timkoto Registration Link</h1>" +
                "<p>Hi!</p>" +
                "<br>" +
                "<p>Please click on the link below to register.</p>" +
                "<br>" +
                $"<p><a href='{regLink}'>{regLink}</a></p>" +
                "<br>" +
                "<p>Your Team!!!</p>";

            var message = new MailMessage {IsBodyHtml = true, From = new MailAddress(from, fromName)};
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
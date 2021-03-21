using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Services
{
    public class EmailService
    {
        public async Task<bool> SendEmail()
        {
            // Replace sender@example.com with your "From" address. 
            // This address must be verified with Amazon SES.
            String FROM = "no-reply@timkoto.com";
            String FROMNAME = "Timkoto";

            // Replace recipient@example.com with a "To" address. If your account 
            // is still in the sandbox, this address must be verified.
            //String TO = "melvin_780702@yahoo.com";
            String TO = "iskoap@yahoo.com";

            // Replace smtp_username with your Amazon SES SMTP user name.
            String SMTP_USERNAME = "AKIASWF2UQS5HPA2LF52";

            // Replace smtp_password with your Amazon SES SMTP password.
            String SMTP_PASSWORD = "BLDqaks6CB0KduBGKlQJMSQk4R/pLE0/CslEUxkSuDxS";

            // (Optional) the name of a configuration set to use for this message.
            // If you comment out this line, you also need to remove or comment out
            // the "X-SES-CONFIGURATION-SET" header below.
            String CONFIGSET = "default_config";

            // If you're using Amazon SES in a region other than US West (Oregon), 
            // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
            // endpoint in the appropriate AWS Region.
            String HOST = "email-smtp.ap-southeast-1.amazonaws.com";

            // The port you will connect to on the Amazon SES SMTP endpoint. We
            // are choosing port 587 because we will use STARTTLS to encrypt
            // the connection.
            int PORT = 587;

            // The subject line of the email
            String SUBJECT =
                "Registration Link";

            // The body of the email
            String BODY =
                "<h1>Timkoto Registration Link</h1>" +
                "<p>Please click on the link below to register.</p>" +
                "<br>" +
                "<br>" +
                "<p><a href='https://timkoto.com/register/12345678900987654321'>https://timkoto.com/register/12345678900987654321</a></p>" +
                "<br>" +
                "<br>" +
                "<p>Your Team!!!</p>";

        // Create and build a new MailMessage object
        MailMessage message = new MailMessage { IsBodyHtml = true, From = new MailAddress(FROM, FROMNAME) };
            message.To.Add(new MailAddress(TO));
            message.Subject = SUBJECT;
            message.Body = BODY;
            // Comment or delete the next line if you are not using a configuration set
            message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

            using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
            {
                // Pass SMTP credentials
                client.Credentials =
                    new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                // Enable SSL encryption
                client.EnableSsl = true;

                // Try to send the message. Show status in console.
                try
                {
                    Console.WriteLine("Attempting to send email...");
                    client.Send(message);
                    Console.WriteLine("Email sent!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);
                }
            }

            return true;
        }
    }
}
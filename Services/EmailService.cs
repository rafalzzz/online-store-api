using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        public EmailService()
        {
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {

            bool useUsl = true;
            string senderName = Environment.GetEnvironmentVariable(EnvironmentVariables.SenderName);
            string senderEmail = Environment.GetEnvironmentVariable(EnvironmentVariables.SenderEmail);
            string senderEmailPassword = Environment.GetEnvironmentVariable(EnvironmentVariables.SenderEmailPassword);
            string smtpServer = Environment.GetEnvironmentVariable(EnvironmentVariables.SmtpServer);
            string smtpPort = Environment.GetEnvironmentVariable(EnvironmentVariables.SmtpPort);

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, int.Parse(smtpPort), useUsl);
            await client.AuthenticateAsync(senderEmail, senderEmailPassword);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }
}
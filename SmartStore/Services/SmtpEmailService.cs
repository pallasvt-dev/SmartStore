using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace SmartStore.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail)
                || string.IsNullOrWhiteSpace(_settings.AppPassword)
                || _settings.AppPassword.Contains("YOUR_GMAIL_APP_PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chưa cấu hình Gmail app password trong EmailSettings:AppPassword.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword)
            };

            await client.SendMailAsync(message);
        }
    }
}

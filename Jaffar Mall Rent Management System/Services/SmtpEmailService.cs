using System.Net;
using System.Net.Mail;
using Jaffar_Mall_Rent_Management_System.Models;
using Microsoft.Extensions.Options;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtpSettings = smtpOptions.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));
            }

            if (string.IsNullOrWhiteSpace(_smtpSettings.Host) ||
                string.IsNullOrWhiteSpace(_smtpSettings.FromEmail))
            {
                throw new InvalidOperationException("SMTP settings are not configured.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
            };

            await client.SendMailAsync(message);
        }
    }
}

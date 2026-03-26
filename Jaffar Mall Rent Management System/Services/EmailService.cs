using System.Net;
using System.Net.Mail;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendLeaseAssignmentEmailAsync(string toEmail, string tenantName, string propertyName, decimal rentAmount, int months, DateTime startDate, DateTime endDate)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");
                var toAddress = new MailAddress(toEmail, tenantName);

                string subject = $"Lease Assignment Confirmation - {propertyName}";
                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                            <h2 style='color: #2563eb;'>Lease Assignment Confirmation</h2>
                            <p>Dear <strong>{tenantName}</strong>,</p>
                            <p>We are pleased to inform you that you have been successfully assigned to the following property:</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Property:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{propertyName}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Monthly Rent:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>PKR {rentAmount:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Start Date:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{startDate:MMM dd, yyyy}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Deadline (End Date):</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{endDate:MMM dd, yyyy}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Duration:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{months} Months</td>
                                </tr>
                            </table>

                            <p style='margin-top: 20px;'>Please ensure that all payments are made on or before the due dates as per your agreement.</p>
                            <p>If you have any questions, please do not hesitate to contact our management office.</p>
                            
                            <br/>
                            <p>Best regards,</p>
                            <div style='margin-top: 15px; font-family: Arial, sans-serif; line-height: 1.5;'>
                                <strong style='color: #eab308; font-size: 18px;'>Manager</strong><br/>
                                <strong style='color: #1e3a8a; font-size: 15px;'>Jaffar Mall Management Office</strong><br/>
                                <strong style='color: #1e3a8a; font-size: 14px;'>Contact us: +92 310 3709000</strong><br/>
                                <strong style='color: #1e3a8a; font-size: 14px;'>Main GT Road, Jhelum, Punjab, Pakistan</strong>
                            </div>
                        </div>
                    </body>
                    </html>";

                using var smtp = new SmtpClient
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, smtpPass)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                Console.WriteLine($"[EMAIL QUEUED] To: {toEmail}, Subject: {subject}");

                if (smtpUser != "your_email@gmail.com") 
                {
                    await smtp.SendMailAsync(message);
                    Console.WriteLine($"[EMAIL SENT SUCCESSFULLY] To: {toEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}

using System.Net;
using System.Net.Mail;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _officeEmail = "jaffar.mall.office.jhelum@gmail.com";

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendLeaseAssignmentEmailAsync(string toEmail, string tenantName, string propertyName, decimal rentAmount, int months, DateTime startDate, DateTime endDate, int rentDueDays, decimal securityDeposit, int securityDueDays, int incrementMonths = 0, decimal incrementPercentage = 0)
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
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Payment Schedule:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>Every {rentDueDays} Days</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Security Deposit:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>PKR {securityDeposit:N2}</td>
                                </tr>
                                 <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Security Due:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>Within {securityDueDays} Days</td>
                                </tr>
                                {(incrementMonths > 0 && incrementPercentage > 0 ? $@"
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Rent Increment:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{incrementPercentage}% increase every {incrementMonths} months</td>
                                </tr>" : "")}
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

        public async Task SendLeaseStatusUpdateEmailAsync(string toEmail, string tenantName, string propertyName, string newStatus)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");
                var toAddress = new MailAddress(toEmail, tenantName);

                string subject = $"Lease Status Update - {propertyName}";
                string statusColor = newStatus.ToLower().Contains("terminated") ? "#ef4444" : "#f59e0b";
                
                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                            <h2 style='color: #2563eb;'>Lease Status Update</h2>
                            <p>Dear <strong>{tenantName}</strong>,</p>
                            <p>This is to inform you that the status of your lease for <strong>{propertyName}</strong> has been updated.</p>
                            
                            <div style='background-color: #f8fafc; padding: 15px; border-radius: 6px; border-left: 4px solid {statusColor}; margin: 20px 0;'>
                                <strong>New Status:</strong> <span style='color: {statusColor}; font-weight: bold; text-transform: uppercase;'>{newStatus}</span>
                            </div>

                            <p>If this was unexpected or if you have any questions, please contact the management office immediately.</p>
                            
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

                if (smtpUser != "your_email@gmail.com") 
                {
                    await smtp.SendMailAsync(message);
                    Console.WriteLine($"[STATUS EMAIL SENT] To: {toEmail}, Status: {newStatus}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending status email: {ex.Message}");
            }
        }
        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string tenantName, string propertyName, decimal paidAmount, decimal remainingBalance, string paymentType, int relaxationDays, decimal remainingSecurityBalance)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");
                var toAddress = new MailAddress(toEmail, tenantName);

                string subject = $"Payment Confirmation - {propertyName}";
                
                string relaxationInfo = relaxationDays > 0 
                    ? $"<p style='color: #ef4444;'><strong>Note:</strong> Please clear the remaining balance within the next {relaxationDays} days.</p>" 
                    : "";

                string securityInfo = remainingSecurityBalance > 0
                    ? $@"<tr>
                            <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Remaining Security Balance:</strong></td>
                            <td style='padding: 8px; border-bottom: 1px solid #ddd; color: #ef4444;'>
                                PKR {remainingSecurityBalance:N2}
                            </td>
                        </tr>"
                    : "";

                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                            <h2 style='color: #10b981;'>Payment Received Successfully</h2>
                            <p>Dear <strong>{tenantName}</strong>,</p>
                            <p>We have successfully received your {paymentType} payment for <strong>{propertyName}</strong>.</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Amount Paid:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>PKR {paidAmount:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Remaining Rent Balance:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd; color: {(remainingBalance > 0 ? "#ef4444" : "#10b981")};'>
                                        PKR {remainingBalance:N2}
                                    </td>
                                </tr>
                                {securityInfo}
                            </table>

                            {relaxationInfo}

                            <p style='margin-top: 20px;'>Thank you for your prompt payment.</p>
                            
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

                AddBccRecipients(message);

                if (smtpUser != "your_email@gmail.com") 
                {
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending payment confirmation: {ex.Message}");
            }
        }

        public async Task SendMaintenanceVoucherEmailAsync(Maintenance maintenance, Property property, bool isUpdate)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");
                
                string ownerEmail = "owner.jaffar.mall@gmail.com";
                var toAddress = new MailAddress(ownerEmail, "Mall Owner");

                string actionText = isUpdate ? "Updated" : "New";
                string subject = $"[{actionText.ToUpper()}] Maintenance Voucher #{maintenance.Id:D5} - {property.Name}";
                
                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                            <h2 style='color: #2563eb;'>{actionText} Maintenance Voucher</h2>
                            <p>Dear <strong>Owner</strong>,</p>
                            <p>A maintenance voucher has been {(isUpdate ? "updated" : "generated")} by the management office.</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Voucher No:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd; color: #dc2626; font-weight: bold;'>#{maintenance.Id:D5}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Date:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{maintenance.CreatedAt:MMM dd, yyyy}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Property:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{property.Name}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Issue Title:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{maintenance.Title}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Repair Cost:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>PKR {maintenance.RepairCost:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Received by Repairer:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold; color: #1d4ed8;'>PKR {maintenance.AmountPaid:N2}</td>
                                </tr>
                            </table>

                            <br/>
                            <p>Best regards,</p>
                            <div style='margin-top: 15px; font-family: Arial, sans-serif; line-height: 1.5;'>
                                <strong style='color: #eab308; font-size: 18px;'>Manager</strong><br/>
                                <strong style='color: #1e3a8a; font-size: 15px;'>Jaffar Mall Management Office</strong><br/>
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

                AddBccRecipients(message);

                if (smtpUser != "your_email@gmail.com") 
                {
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending maintenance email: {ex.Message}");
            }
        }

        private void AddBccRecipients(MailMessage message)
        {
            // Always BCC Office
            message.Bcc.Add(new MailAddress(_officeEmail, "Office Copy"));

            // BCC Owner if configured
            var ownerEmail = _configuration["EmailSettings:OwnerEmail"];
            if (!string.IsNullOrEmpty(ownerEmail))
            {
                message.Bcc.Add(new MailAddress(ownerEmail, "Owner Copy"));
            }
        }
    }
}

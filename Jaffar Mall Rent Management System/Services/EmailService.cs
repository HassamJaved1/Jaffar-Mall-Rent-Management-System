using System.Net;
using System.Net.Mail;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly PdfService _pdfService;
        private readonly string _officeEmail = "jaffar.mall.office.jhelum@gmail.com";

        public EmailService(IConfiguration configuration, PdfService pdfService)
        {
            _configuration = configuration;
            _pdfService = pdfService;
        }

        public async Task SendLeaseAssignmentEmailAsync(string toEmail, string tenantName, string propertyName, decimal rentAmount, int months, DateTime startDate, DateTime endDate, int rentDueMonths, decimal securityDeposit, int securityDueDays, int incrementMonths = 0, decimal incrementPercentage = 0)
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
                    <body style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; color: #1e293b; line-height: 1.8; background-color: #f8fafc; padding: 40px 0;'>
                        <div style='max-width: 650px; margin: 0 auto; background-color: #ffffff; padding: 40px; border-radius: 12px; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1);'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <div style='margin-bottom: 10px;'>
                                    <span style='color: #1e3a8a; font-size: 28px; font-weight: bold; letter-spacing: 0.05em;'>JAFFAR</span>
                                    <span style='color: #eab308; font-size: 28px; font-weight: bold; letter-spacing: 0.05em;'>MALL</span>
                                </div>
                                <h2 style='color: #1e3a8a; margin: 0; font-size: 20px; letter-spacing: -0.025em;'>LEASE AGREEMENT CONFIRMATION</h2>
                                <div style='height: 3px; width: 60px; background-color: #eab308; margin: 15px auto;'></div>
                            </div>

                            <p>Dear <strong>{tenantName}</strong>,</p>

                            <p>We are pleased to officially confirm the lease assignment for the property named <strong>{propertyName}</strong>. This communication serves as a formal acknowledgment of the terms agreed upon for your tenancy at Jaffar Mall.</p>

                            <p>According to our records, your lease is scheduled to commence on <strong>{startDate:MMMM dd, yyyy}</strong> and will remain in effect for a duration of <strong>{months} months</strong>, concluding on <strong>{endDate:MMMM dd, yyyy}</strong>. The monthly rental commitment for this unit has been established at <strong>PKR {rentAmount:N2}</strong>, with payments due every <strong>{rentDueMonths} months</strong>.</p>

                            {(incrementMonths > 0 && incrementPercentage > 0 ? $@"
                            <p style='background-color: #fffbeb; padding: 15px; border-radius: 8px; border-left: 4px solid #f59e0b; margin: 20px 0;'>
                                <strong>Note on Rent Increment:</strong> Please be advised that as per the agreement, a rent increase of <strong>{incrementPercentage}%</strong> will be applied automatically every <strong>{incrementMonths} months</strong> during the tenure of this lease.
                            </p>" : "")}

                            <p>To finalize the security requirements, a deposit of <strong>PKR {securityDeposit:N2} has been made. This deposit is held to ensure the maintenance and care of the premises throughout your occupancy.</p>

                            <p>We are committed to providing a professional and supportive managed environment for all our tenants. Should you have any questions regarding these terms or require further assistance, please do not hesitate to contact our management office directly.</p>

                            <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #e2e8f0;'>
                                <p style='margin: 0; font-weight: 600; color: #1e3a8a;'>Manager</p>
                                <p style='margin: 4px 0; font-weight: 600; color: #1e3a8a;'>Jaffar Mall Management Office</p>
                                <p style='margin: 4px 0; color: #eab308; font-size: 14px;'>Main GT Road, Jhelum,                Punjab, Pakistan</p>
                                <p style='margin: 4px 0; color: #eab308; font-size: 14px;'>Contact: +92 310 3709000</p>
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

                // Generate and Attach PDF Contract (Assignment)
                try 
                {
                    var pdfData = _pdfService.GenerateLeaseContractPdf(tenantName, propertyName, rentAmount, months, startDate, endDate, rentDueMonths, securityDeposit, securityDueDays, incrementMonths, incrementPercentage, "Active");
                    var fileName = $"Lease_Assignment_{propertyName.Replace(" ", "_")}.pdf";
                    var attachment = new Attachment(new MemoryStream(pdfData), fileName, "application/pdf");
                    message.Attachments.Add(attachment);
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"Error attaching assignment PDF: {pdfEx.Message}");
                }

                AddBccRecipients(message);

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

        public async Task SendLeaseStatusUpdateEmailAsync(string toEmail, string tenantName, string propertyName, string newStatus, decimal rentAmount, int months, DateTime startDate, DateTime endDate, int rentDueMonths, decimal securityDeposit, int securityDueDays, int incrementMonths = 0, decimal incrementPercentage = 0)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? "your_password";
 
                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");
                var toAddress = new MailAddress(toEmail, tenantName);
 
                string subject = $"Lease Update Notification - {propertyName} ({newStatus})";
                string statusColor = newStatus.ToLower().Contains("terminated") ? "#ef4444" : "#2563eb";

                string body = $@"
                    <html>
                    <body style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; color: #1e293b; line-height: 1.8; background-color: #f8fafc; padding: 40px 0;'>
                        <div style='max-width: 650px; margin: 0 auto; background-color: #ffffff; padding: 40px; border-radius: 12px; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1);'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <div style='margin-bottom: 10px;'>
                                    <span style='color: #1e3a8a; font-size: 28px; font-weight: bold; letter-spacing: 0.05em;'>JAFFAR</span>
                                    <span style='color: #eab308; font-size: 28px; font-weight: bold; letter-spacing: 0.05em;'>MALL</span>
                                </div>
                                <h1 style='color: #1e3a8a; margin: 0; font-size: 24px; letter-spacing: -0.025em;'>LEASE STATUS UPDATE</h1>
                                <div style='height: 3px; width: 60px; background-color: #eab308; margin: 15px auto;'></div>
                            </div>
 
                            <p>Dear <strong>{tenantName}</strong>,</p>
 
                            <p>This communication serves as a formal notification regarding the status of your lease for the property located at <strong>{propertyName}</strong>. Please be advised that the status has been updated to:</p>
 
                            <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; border-left: 4px solid {statusColor}; margin: 25px 0; text-align: center;'>
                                <span style='color: {statusColor}; font-size: 18px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.1em;'>{newStatus}</span>
                            </div>
 
                            <h3 style='color: #1e3a8a; font-size: 16px; border-bottom: 1px solid #e2e8f0; padding-bottom: 8px;'>Current Lease Terms Reference:</h3>
                            <p style='font-size: 14px;'>
                                <strong>Period:</strong> {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy} ({months} Months)<br/>
                                <strong>Monthly Rent:</strong> PKR {rentAmount:N2}<br/>
                                <strong>Rent Cycle:</strong> Every {rentDueMonths} Months<br/>
                                <strong>Security Deposit:</strong> PKR {securityDeposit:N2}
                            </p>

                            {(incrementMonths > 0 && incrementPercentage > 0 ? $@"
                            <p style='background-color: #fffbeb; padding: 12px; border-radius: 6px; border-left: 3px solid #f59e0b; margin-top: 15px; font-size: 13px;'>
                                <strong>Rent Increment:</strong> {incrementPercentage}% increase every {incrementMonths} months.
                            </p>" : "")}
 
                            <p style='margin-top: 25px;'>We have attached the updated digital copy of your contract for your records. If this status change was unexpected or if you require any clarification, please contact our management office immediately.</p>
 
                            <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #e2e8f0;'>
                                <p style='margin: 0; font-weight: 600; color: #1e3a8a;'>Manager</p>
                                <p style='margin: 4px 0; font-weight: 600; color: #1e3a8a;'>Jaffar Mall Management Office</p>
                                <p style='margin: 4px 0; color: #eab308; font-size: 14px;'>Main GT Road, Jhelum, Punjab, Pakistan</p>
                                <p style='margin: 4px 0; color: #eab308; font-size: 14px;'>Contact: +92 310 3709000</p>
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

                // Generate and Attach Status-Specific PDF Contract
                try 
                {
                    var pdfData = _pdfService.GenerateLeaseContractPdf(tenantName, propertyName, rentAmount, months, startDate, endDate, rentDueMonths, securityDeposit, securityDueDays, incrementMonths, incrementPercentage, newStatus);
                    var fileName = $"{newStatus.Replace(" ", "_")}_Notice_{propertyName.Replace(" ", "_")}.pdf";
                    var attachment = new Attachment(new MemoryStream(pdfData), fileName, "application/pdf");
                    message.Attachments.Add(attachment);
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"Error attaching status PDF: {pdfEx.Message}");
                }

                AddBccRecipients(message);

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





                








        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string tenantName, string propertyName, decimal paidAmount, decimal remainingBalance, string paymentType, int relaxationDays, decimal remainingSecurityBalance, RentPayment payment)
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
                                <strong style='color: #1e3a8a; font-size: 18px;'>Manager</strong><br/>
                                <strong style='color: #1e3a8a; font-size: 15px;'>Jaffar Mall Management Office</strong><br/>
                                <strong style='color: #eab308; font-size: 14px;'>Contact us: +92 310 3709000</strong><br/>
                                <strong style='color: #eab308; font-size: 14px;'>Main GT Road, Jhelum, Punjab, Pakistan</strong>
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

                // Generate and Attach PDF Payment Voucher
                try
                {
                    var pdfData = _pdfService.GeneratePaymentVoucherPdf(payment, tenantName, propertyName);
                    var attachment = new Attachment(new MemoryStream(pdfData), $"Payment_Voucher_{payment.Id:D5}.pdf", "application/pdf");
                    message.Attachments.Add(attachment);
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"Error attaching payment PDF: {pdfEx.Message}");
                }

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
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Issue / Repair Cost Estimate:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>PKR {maintenance.RepairCost:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Amount Paid to Repairer:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold; color: #1d4ed8;'>PKR {maintenance.AmountPaid:N2}</td>
                                </tr>
                                <tr style='background-color: #1e3a8a;'>
                                    <td style='padding: 10px; font-weight: bold; color: white; font-size: 15px;'>GRAND TOTAL</td>
                                    <td style='padding: 10px; font-weight: bold; color: #eab308; font-size: 16px;'>PKR {(maintenance.RepairCost + maintenance.AmountPaid):N2}</td>
                                </tr>
                            </table>

                            <br/>
                            <p>Best regards,</p>
                            <div style='margin-top: 15px; font-family: Arial, sans-serif; line-height: 1.5;'>
                                <strong style='color: #1e3a8a; font-size: 18px;'>Manager</strong><br/>
                                <strong style='color: #1e3a8a; font-size: 15px;'>Jaffar Mall Management Office</strong><br/>
                                <strong style='color: #eab308; font-size: 14px;'>Contact us: +92 310 3709000</strong><br/>
                                <strong style='color: #eab308; font-size: 14px;'>Main GT Road, Jhelum, Punjab, Pakistan</strong>
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

                // Generate and Attach PDF Maintenance Voucher
                try
                {
                    var pdfData = _pdfService.GenerateMaintenanceVoucherPdf(maintenance, property.Name);
                    var attachment = new Attachment(new MemoryStream(pdfData), $"Maintenance_Voucher_{maintenance.Id:D5}.pdf", "application/pdf");
                    message.Attachments.Add(attachment);
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"Error attaching maintenance PDF: {pdfEx.Message}");
                }

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

        // ─── Rent Due Reminder (7-day advance) ─────────────────────────────────────
        public async Task SendRentReminderEmailAsync(string toEmail, string tenantName, string propertyName, decimal rentAmount, DateTime dueDate, string managerEmail)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort   = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser   = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass   = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");

                string subject = $"Rent Payment Reminder – Due in 7 Days | {propertyName}";
                string body = $@"
                <html>
                <body style='font-family:""Segoe UI"",Tahoma,Geneva,Verdana,sans-serif;color:#1e293b;line-height:1.8;background-color:#f8fafc;padding:40px 0;'>
                    <div style='max-width:650px;margin:0 auto;background:#ffffff;padding:40px;border-radius:12px;border:1px solid #e2e8f0;box-shadow:0 4px 6px -1px rgb(0 0 0/0.1);'>
                        <div style='text-align:center;margin-bottom:30px;'>
                            <div style='margin-bottom:10px;'>
                                <span style='color:#1e3a8a;font-size:28px;font-weight:bold;'>JAFFAR</span>
                                <span style='color:#eab308;font-size:28px;font-weight:bold;'>MALL</span>
                            </div>
                            <h2 style='color:#1e3a8a;margin:0;font-size:18px;'>RENT PAYMENT REMINDER</h2>
                            <div style='height:3px;width:60px;background:#eab308;margin:15px auto;'></div>
                        </div>
                        <p>Dear <strong>{tenantName}</strong>,</p>
                        <p>This is a friendly reminder that your rent payment for <strong>{propertyName}</strong> is due in <strong>7 days</strong>, on <strong>{dueDate:MMMM dd, yyyy}</strong>.</p>
                        <div style='background:#fffbeb;padding:20px;border-radius:8px;border-left:4px solid #f59e0b;margin:25px 0;text-align:center;'>
                            <p style='margin:0;font-size:14px;color:#92400e;'>Amount Due</p>
                            <p style='margin:8px 0 0;font-size:28px;font-weight:bold;color:#1e3a8a;'>PKR {rentAmount:N2}</p>
                        </div>
                        <p>Please ensure the payment is made before the due date to avoid any late fees or inconvenience. If you have already submitted your payment, kindly disregard this message.</p>
                        <p>For any queries, please contact our management office.</p>
                        <div style='margin-top:40px;padding-top:20px;border-top:1px solid #e2e8f0;'>
                            <p style='margin:0;font-weight:600;color:#1e3a8a;'>Manager</p>
                            <p style='margin:4px 0;font-weight:600;color:#1e3a8a;'>Jaffar Mall Management Office</p>
                            <p style='margin:4px 0;color:#eab308;font-size:14px;'>Main GT Road, Jhelum, Punjab, Pakistan</p>
                            <p style='margin:4px 0;color:#eab308;font-size:14px;'>Contact: +92 310 3709000</p>
                        </div>
                    </div>
                </body>
                </html>";

                using var smtp = new SmtpClient { Host = smtpServer, Port = smtpPort, EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network, UseDefaultCredentials = false, Credentials = new NetworkCredential(smtpUser, smtpPass) };

                // Email to tenant
                if (!string.IsNullOrEmpty(toEmail) && smtpUser != "your_email@gmail.com")
                {
                    using var tenantMsg = new MailMessage(fromAddress, new MailAddress(toEmail, tenantName)) { Subject = subject, Body = body, IsBodyHtml = true };
                    AddBccRecipients(tenantMsg);
                    await smtp.SendMailAsync(tenantMsg);
                    Console.WriteLine($"[REMINDER SENT] Tenant: {toEmail}");
                }

                // Notification to manager
                if (!string.IsNullOrEmpty(managerEmail) && smtpUser != "your_email@gmail.com")
                {
                    string managerBody = $@"<html><body style='font-family:Segoe UI,sans-serif;padding:30px;'>
                        <h2 style='color:#1e3a8a;'>Rent Reminder Sent to Tenant</h2>
                        <p>A <strong>7-day rent reminder</strong> has been automatically sent to:</p>
                        <ul>
                            <li><strong>Tenant:</strong> {tenantName}</li>
                            <li><strong>Property:</strong> {propertyName}</li>
                            <li><strong>Amount Due:</strong> PKR {rentAmount:N2}</li>
                            <li><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</li>
                        </ul>
                        <p style='color:#64748b;font-size:13px;'>This is an automated management notification from Jaffar Mall System.</p>
                    </body></html>";
                    using var managerMsg = new MailMessage(fromAddress, new MailAddress(managerEmail, "Manager")) { Subject = $"[ACTION NEEDED] Rent Due in 7 Days – {tenantName} ({propertyName})", Body = managerBody, IsBodyHtml = true };
                    await smtp.SendMailAsync(managerMsg);
                    Console.WriteLine($"[REMINDER NOTIFIED] Manager: {managerEmail}");
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error sending rent reminder: {ex.Message}"); }
        }

        // ─── Rent Overdue Notice (due date reached, no payment) ────────────────────
        public async Task SendRentOverdueEmailAsync(string toEmail, string tenantName, string propertyName, decimal rentAmount, DateTime dueDate, string managerEmail)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort   = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser   = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass   = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");

                string subject = $"⚠️ URGENT: Rent Payment Due Today | {propertyName}";
                string body = $@"
                <html>
                <body style='font-family:""Segoe UI"",Tahoma,Geneva,Verdana,sans-serif;color:#1e293b;line-height:1.8;background-color:#f8fafc;padding:40px 0;'>
                    <div style='max-width:650px;margin:0 auto;background:#ffffff;padding:40px;border-radius:12px;border:1px solid #fecaca;box-shadow:0 4px 6px -1px rgb(0 0 0/0.1);'>
                        <div style='text-align:center;margin-bottom:30px;'>
                            <div style='margin-bottom:10px;'>
                                <span style='color:#1e3a8a;font-size:28px;font-weight:bold;'>JAFFAR</span>
                                <span style='color:#eab308;font-size:28px;font-weight:bold;'>MALL</span>
                            </div>
                            <h2 style='color:#dc2626;margin:0;font-size:18px;'>URGENT: RENT DUE TODAY</h2>
                            <div style='height:3px;width:60px;background:#dc2626;margin:15px auto;'></div>
                        </div>
                        <p>Dear <strong>{tenantName}</strong>,</p>
                        <p>This is an urgent notice that your rent payment for <strong>{propertyName}</strong> is <strong>due today, {dueDate:MMMM dd, yyyy}</strong>, and has not yet been received.</p>
                        <div style='background:#fef2f2;padding:20px;border-radius:8px;border-left:4px solid #dc2626;margin:25px 0;text-align:center;'>
                            <p style='margin:0;font-size:14px;color:#991b1b;'>Overdue Amount</p>
                            <p style='margin:8px 0 0;font-size:28px;font-weight:bold;color:#dc2626;'>PKR {rentAmount:N2}</p>
                            <p style='margin:8px 0 0;font-size:13px;color:#991b1b;'>Due: {dueDate:dd MMM yyyy}</p>
                        </div>
                        <p>We kindly request you to make the payment <strong>immediately</strong> to avoid a late payment penalty. If you are facing difficulties, please contact our office to discuss a suitable arrangement.</p>
                        <div style='margin-top:40px;padding-top:20px;border-top:1px solid #fecaca;'>
                            <p style='margin:0;font-weight:600;color:#1e3a8a;'>Manager</p>
                            <p style='margin:4px 0;font-weight:600;color:#1e3a8a;'>Jaffar Mall Management Office</p>
                            <p style='margin:4px 0;color:#eab308;font-size:14px;'>Main GT Road, Jhelum, Punjab, Pakistan</p>
                            <p style='margin:4px 0;color:#eab308;font-size:14px;'>Contact: +92 310 3709000</p>
                        </div>
                    </div>
                </body>
                </html>";

                using var smtp = new SmtpClient { Host = smtpServer, Port = smtpPort, EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network, UseDefaultCredentials = false, Credentials = new NetworkCredential(smtpUser, smtpPass) };

                // Email to tenant
                if (!string.IsNullOrEmpty(toEmail) && smtpUser != "your_email@gmail.com")
                {
                    using var tenantMsg = new MailMessage(fromAddress, new MailAddress(toEmail, tenantName)) { Subject = subject, Body = body, IsBodyHtml = true };
                    AddBccRecipients(tenantMsg);
                    await smtp.SendMailAsync(tenantMsg);
                    Console.WriteLine($"[OVERDUE NOTICE SENT] Tenant: {toEmail}");
                }

                // Urgent notification to manager
                if (!string.IsNullOrEmpty(managerEmail) && smtpUser != "your_email@gmail.com")
                {
                    string managerBody = $@"<html><body style='font-family:Segoe UI,sans-serif;padding:30px;'>
                        <h2 style='color:#dc2626;'>⚠️ Rent Overdue – Collect Payment Today</h2>
                        <p>The following tenant has <strong>not paid rent</strong> and payment is due <strong>today</strong>:</p>
                        <ul>
                            <li><strong>Tenant:</strong> {tenantName}</li>
                            <li><strong>Property:</strong> {propertyName}</li>
                            <li><strong>Amount Overdue:</strong> PKR {rentAmount:N2}</li>
                            <li><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</li>
                        </ul>
                        <p>An urgent notice has been sent to the tenant. Please follow up to collect the payment.</p>
                        <p style='color:#64748b;font-size:13px;'>This is an automated management alert from Jaffar Mall System.</p>
                    </body></html>";
                    using var managerMsg = new MailMessage(fromAddress, new MailAddress(managerEmail, "Manager")) { Subject = $"[URGENT] Collect Rent Now – {tenantName} ({propertyName})", Body = managerBody, IsBodyHtml = true };
                    await smtp.SendMailAsync(managerMsg);
                    Console.WriteLine($"[OVERDUE NOTIFIED] Manager: {managerEmail}");
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error sending overdue notice: {ex.Message}"); }
        }
        // ─── Rent Increase Notification ────────────────────────────────────────────
        public async Task SendRentIncreaseNotificationAsync(string toEmail, string tenantName, string propertyName, decimal oldRent, decimal newRent, decimal percentage, DateTime effectiveDate)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort   = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser   = _configuration["EmailSettings:SmtpUser"] ?? "your_email@gmail.com";
                var smtpPass   = _configuration["EmailSettings:SmtpPass"] ?? "your_password";

                var fromAddress = new MailAddress(smtpUser, "Jaffar Mall Management");

                string subject = $"Rent Increment Notification | {propertyName}";
                string body = $@"
                <html>
                <body style='font-family:""Segoe UI"",Tahoma,Geneva,Verdana,sans-serif;color:#1e293b;line-height:1.8;background-color:#f8fafc;padding:40px 0;'>
                    <div style='max-width:650px;margin:0 auto;background:#ffffff;padding:40px;border-radius:12px;border:1px solid #e2e8f0;box-shadow:0 4px 6px -1px rgb(0 0 0/0.1);'>
                        <div style='text-align:center;margin-bottom:30px;'>
                            <div style='margin-bottom:10px;'>
                                <span style='color:#1e3a8a;font-size:28px;font-weight:bold;'>JAFFAR</span>
                                <span style='color:#eab308;font-size:28px;font-weight:bold;'>MALL</span>
                            </div>
                            <h2 style='color:#1e3a8a;margin:0;font-size:18px;'>RENT INCREMENT APPLIED</h2>
                            <div style='height:3px;width:60px;background:#eab308;margin:15px auto;'></div>
                        </div>
                        <p>Dear <strong>{tenantName}</strong>,</p>
                        <p>This is to formally notify you that, as per your lease agreement, a scheduled rent increment has been applied to the property <strong>{propertyName}</strong> effective from <strong>{effectiveDate:MMMM dd, yyyy}</strong>.</p>
                        
                        <div style='display:flex; justify-content:space-between; background:#f8fafc; padding:20px; border-radius:8px; border:1px solid #e2e8f0; margin:25px 0;'>
                            <div style='text-align:center; flex:1;'>
                                <p style='margin:0;font-size:12px;color:#64748b;text-transform:uppercase;'>Previous Rent</p>
                                <p style='margin:5px 0 0;font-size:18px;font-weight:600;color:#64748b;'>PKR {oldRent:N0}</p>
                            </div>
                            <div style='text-align:center; flex:1; border-left:1px solid #e2e8f0; border-right:1px solid #e2e8f0;'>
                                <p style='margin:0;font-size:12px;color:#059669;text-transform:uppercase;'>Increase ({percentage}%)</p>
                                <p style='margin:5px 0 0;font-size:18px;font-weight:600;color:#059669;'>+PKR {(newRent - oldRent):N0}</p>
                            </div>
                            <div style='text-align:center; flex:1;'>
                                <p style='margin:0;font-size:12px;color:#1e3a8a;text-transform:uppercase;'>New Monthly Rent</p>
                                <p style='margin:5px 0 0;font-size:18px;font-weight:bold;color:#1e3a8a;'>PKR {newRent:N0}</p>
                            </div>
                        </div>

                        <p>This updated rent amount will be reflected in your next collection period. We appreciate your continued cooperation and tenancy at Jaffar Mall.</p>
                        
                        <div style='margin-top:40px;padding-top:20px;border-top:1px solid #e2e8f0;'>
                            <p style='margin:0;font-weight:600;color:#1e3a8a;'>Manager</p>
                            <p style='margin:4px 0;font-weight:600;color:#1e3a8a;'>Jaffar Mall Management Office</p>
                            <p style='margin:4px 0;color:#eab308;font-size:14px;'>Main GT Road, Jhelum, Punjab, Pakistan</p>
                            <p style='margin:4px 0;color:#eab308;font-size:14px;'>Contact: +92 310 3709000</p>
                        </div>
                    </div>
                </body>
                </html>";

                using var smtp = new SmtpClient { Host = smtpServer, Port = smtpPort, EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network, UseDefaultCredentials = false, Credentials = new NetworkCredential(smtpUser, smtpPass) };

                if (!string.IsNullOrEmpty(toEmail) && smtpUser != "your_email@gmail.com")
                {
                    using var message = new MailMessage(fromAddress, new MailAddress(toEmail, tenantName)) { Subject = subject, Body = body, IsBodyHtml = true };
                    AddBccRecipients(message);
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error sending rent increment notification: {ex.Message}"); }
        }
    }
}

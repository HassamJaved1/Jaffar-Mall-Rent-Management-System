namespace Jaffar_Mall_Rent_Management_System.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}

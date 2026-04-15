namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Rent Management";
        public bool EnableSsl { get; set; } = true;
    }
}

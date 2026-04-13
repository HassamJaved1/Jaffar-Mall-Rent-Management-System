namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class Document
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public long TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty; // Useful for views
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

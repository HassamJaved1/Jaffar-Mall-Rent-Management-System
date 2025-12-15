namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class Maintenance
    {
        public long Id { get; set; }

        public long TenantId { get; set; }

        public long PropertyId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public byte Status { get; set; } = (byte)MaintenanceStatus.Open;

        public byte Priority { get; set; } = (byte)MaintenancePriority.Medium;

        public string? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }

    public enum MaintenanceStatus : short
    {
        Open = 1,
        InProgress = 2,
        Resolved = 3,
        Closed = 4
    }

    public enum MaintenancePriority : short
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
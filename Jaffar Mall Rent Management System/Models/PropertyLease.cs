namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class PropertyLease
    {
        public long Id { get; set; }

        public long TenantId { get; set; }

        public long PropertyId { get; set; }

        public string? Description { get; set; }

        public LeaseStatus Status { get; set; }

        public decimal RentAmount { get; set; }

        public decimal? SecurityDeposit { get; set; }

        public string? AddedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public enum LeaseStatus : short
    {
        Pending = 1,
        Active = 2,
        Terminated = 3,
        Cancelled = 4
    }
}
using System.ComponentModel.DataAnnotations;

namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class RentPayment
    {
        public long Id { get; set; }

        [Required]
        public long LeaseId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Bank Transfer, Mobile Transfer

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property (optional, but good for code structure if we were using EF Core, 
        // though we are using Dapper, it helps to visualize)
        // public PropertyLease Lease { get; set; } 
    }
}

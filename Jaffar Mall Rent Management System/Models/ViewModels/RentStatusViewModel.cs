namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class RentStatusViewModel
    {
        public long LeaseId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyNumber { get; set; } = string.Empty;
        
        public decimal MonthlyRent { get; set; }
        public int LeaseDurationMonths { get; set; }
        
        public DateTime LeaseStartDate { get; set; }
        
        // Calculated fields
        public decimal TotalRentExpected { get; set; } // Based on current date vs start date
        public decimal TotalAmountPaid { get; set; }
        public decimal Balance { get; set; } // Expected - Paid
        
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue
    }
}

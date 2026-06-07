namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class RentStatusViewModel
    {
        public long LeaseId { get; set; }
        public long PropertyId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        
        public decimal MonthlyRent { get; set; }
        public int LeaseDurationMonths { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public decimal PaidSecurityDeposit { get; set; }
        public int RentDueMonths { get; set; }
        
        public DateTime LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }
        public DateTime? NextRentDueDate { get; set; }
        public decimal IntervalRent { get; set; }
        
        // Calculated fields
        public decimal TotalRentExpected { get; set; } // Based on current date vs start date
        public decimal TotalAmountPaid { get; set; }
        public decimal CurrentPaidRent { get; set; }
        public decimal Balance { get; set; } // Expected Rent - Paid Rent
        public decimal SecurityBalance { get; set; } // Total Security - Paid Security
        
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue
    }
}

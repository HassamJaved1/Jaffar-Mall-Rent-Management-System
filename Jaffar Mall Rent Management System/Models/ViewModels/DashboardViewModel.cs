using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public double OccupancyRate { get; set; }
        public decimal PendingRentAmount { get; set; }
        public decimal PendingSecurityAmount { get; set; }
        public int ActiveMaintenanceCount { get; set; }
        public decimal TotalExpectedRent { get; set; }

        public List<RentStatusItem>? RentStatus { get; set; }
        public List<MonthlyCashFlowItem>? MonthlyCashFlow { get; set; }
        public List<LeaseListItemViewModel>? TopProperties { get; set; }
        public List<DashboardAlert>? Alerts { get; set; }
        
        public string ManagerName { get; set; } = "Hassam";
    }

    public class RentStatusItem
    {
        public string? Label { get; set; } // Collected, Pending, Overdue
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string? Color { get; set; }
    }

    public class MonthlyCashFlowItem
    {
        public string? Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public double IncomePercentage { get; set; } // Relative to max income for scaling
        public double ExpensePercentage { get; set; }
    }

    public class DashboardAlert
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string? ActionType { get; set; } // ViewTicket, SendReminder
        public long ReferenceId { get; set; }
        public string? TimeAgo { get; set; }
    }
}

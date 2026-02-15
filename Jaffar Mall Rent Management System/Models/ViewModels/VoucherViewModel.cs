namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class VoucherViewModel
    {
        public RentPayment Payment { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public decimal TotalRentDue { get; set; }
        public string ReceivedBy { get; set; } = "Admin"; // Placeholder
    }
}

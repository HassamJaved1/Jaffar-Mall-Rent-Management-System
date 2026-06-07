namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class VoucherListItemViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string LinkUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class LeaseListItemViewModel
    {
        public PropertyLease Lease { get; set; } = new PropertyLease();
        public string TenantName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyNumber { get; set; } = string.Empty;
    }
}

namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class LeasesDropdown
    {
        public List<Property> Properties { get; set; } = new List<Property>();

        public List<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}

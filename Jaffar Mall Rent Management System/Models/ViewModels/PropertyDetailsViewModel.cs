using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class PropertyDetailsViewModel
    {
        public Property Property { get; set; } = new Property();
        public IEnumerable<PropertyLease> Leases { get; set; } = new List<PropertyLease>();
        public Dictionary<long, Tenant> Tenants { get; set; } = new Dictionary<long, Tenant>();
        public Dictionary<long, IEnumerable<RentPayment>> Payments { get; set; } = new Dictionary<long, IEnumerable<RentPayment>>();
    }
}

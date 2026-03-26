using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class TenantDetailsViewModel
    {
        public Tenant Tenant { get; set; } = new Tenant();
        public IEnumerable<PropertyLease> Leases { get; set; } = new List<PropertyLease>();
        public Dictionary<long, Property> Properties { get; set; } = new Dictionary<long, Property>();
        public Dictionary<long, IEnumerable<RentPayment>> Payments { get; set; } = new Dictionary<long, IEnumerable<RentPayment>>();
    }
}

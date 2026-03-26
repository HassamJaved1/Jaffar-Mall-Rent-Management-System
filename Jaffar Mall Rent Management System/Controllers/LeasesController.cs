using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class LeasesController : Controller
    {
        private readonly LeaseServices _leaseServices;
        private readonly TenantServices _tenantServices;
        private readonly PropertyServices _propertyServices;
        private readonly EmailService _emailService;

        public LeasesController(LeaseServices leaseServices, TenantServices tenantServices, PropertyServices propertyServices, EmailService emailService) 
        {
            _leaseServices = leaseServices;
            _tenantServices = tenantServices;
            _propertyServices = propertyServices;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _leaseServices.PopulateDropdowns();
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLease([FromBody] PropertyLease lease)
        {
            if (!ModelState.IsValid)
            {
                 return BackendResponse<bool>.Failure("Invalid data provided.", 400).ToActionResult();
            }

            // Set defaults if needed
            if (lease.Status == 0) lease.Status = LeaseStatus.Pending;
            lease.AddedBy = "Admin User"; // Placeholder for now

            var result = await _leaseServices.AddLeaseAsync(lease);
            
            if (result.Data)
            {
                // Send Email Notification
                _ = Task.Run(async () => 
                {
                    try {
                        var tenant = await _tenantServices.GetTenantByIdAsync(lease.TenantId);
                        var property = await _propertyServices.GetPropertyByIdAsync(lease.PropertyId);
                        
                        if (tenant == null) Console.WriteLine("[DEBUG] Email failed: Tenant not found.");
                        else if (string.IsNullOrEmpty(tenant.Email)) Console.WriteLine($"[DEBUG] Email failed: Tenant {tenant.Name} has no email address.");
                        else if (property == null) Console.WriteLine("[DEBUG] Email failed: Property not found.");
                        else
                        {
                            Console.WriteLine($"[DEBUG] Attempting to send email to {tenant.Email} for {property.Name}");
                            var startDate = lease.StartDate ?? DateTime.Now;
                            var endDate = lease.EndDate ?? DateTime.Now.AddMonths(lease.Months > 0 ? lease.Months : 1);
                            await _emailService.SendLeaseAssignmentEmailAsync(tenant.Email, tenant.Name, property.Name, lease.RentAmount, lease.Months, startDate, endDate);
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("Error triggering email: " + ex.Message);
                    }
                });

                return BackendResponse<bool>.Success(true, result.Message).ToActionResult();
            }
            else
            {
                return BackendResponse<bool>.Failure(result.Message, result.Code).ToActionResult();
            }
        }
    }
}

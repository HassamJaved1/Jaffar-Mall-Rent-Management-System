using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class LeasesController : Controller
    {
        private readonly LeaseServices _leaseServices;

        public LeasesController(LeaseServices leaseServices) 
        {
            _leaseServices = leaseServices;
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
                return BackendResponse<bool>.Success(true, result.Message).ToActionResult();
            }
            else
            {
                return BackendResponse<bool>.Failure(result.Message, result.Code).ToActionResult();
            }
        }
    }
}

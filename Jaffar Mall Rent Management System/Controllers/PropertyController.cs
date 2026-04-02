using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyServices _propertyServices;
        private readonly LeaseServices _leaseServices;
        private readonly TenantServices _tenantServices;
        private readonly RentServices _rentServices;
        private readonly MaintenanceServices _maintenanceServices;

        public PropertyController(PropertyServices propertyServices, LeaseServices leaseServices, TenantServices tenantServices, RentServices rentServices, MaintenanceServices maintenanceServices)
        {
            _propertyServices = propertyServices;
            _leaseServices = leaseServices;
            _tenantServices = tenantServices;
            _rentServices = rentServices;
            _maintenanceServices = maintenanceServices;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] string? search = null)
        {
            const int pageSize = 10; 
            if (page < 1) page = 1;

            var viewModel = await _propertyServices.GetAllPropertiesAsync(page, pageSize, search);
            
            ViewBag.CurrentSearch = search;
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PropertyListPartial", viewModel);
            }

            return View(viewModel);
        }

        public IActionResult AddProperty()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProperty([FromBody] Property property)
        {
            var response = await _propertyServices.AddPropertyAsync(property);
            if (response.Data)
            {
                return BackendResponse<bool>.Success(true, response.Message)
                                            .ToActionResult();
            }

            return BackendResponse<bool>.Failure(response.Message, response.Code)
                                        .ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> EditProperty(long id)
        {
            var property = await _propertyServices.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> EditProperty([FromBody] Property property)
        {
            var response = await _propertyServices.UpdatePropertyAsync(property);
            if (response.Data)
            {
                return BackendResponse<bool>.Success(true, response.Message)
                                            .ToActionResult();
            }

            return BackendResponse<bool>.Failure(response.Message, response.Code)
                                        .ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProperty(long id)
        {
            var response = await _propertyServices.DeletePropertyAsync(id);
            if (response.Data)
            {
                return BackendResponse<bool>.Success(true, response.Message)
                                            .ToActionResult();
            }
             return BackendResponse<bool>.Failure(response.Message, response.Code)
                                        .ToActionResult();
        }

        public IActionResult Visualize()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPropertiesForVisualization()
        {
            var properties = await _propertyServices.GetAllPropertiesAsync();
            return Json(properties);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var property = await _propertyServices.GetPropertyByIdAsync(id);
            if (property == null) return NotFound();

            var leases = await _leaseServices.GetLeasesByPropertyIdAsync(id);
            
            var viewModel = new PropertyDetailsViewModel 
            {
                Property = property,
                Leases = leases ?? new List<PropertyLease>(),
                Tenants = new Dictionary<long, Tenant>(),
                Payments = new Dictionary<long, IEnumerable<RentPayment>>(),
                MaintenanceHistory = new List<Maintenance>()
            };

            // Fetch maintenance history for this property
            var maintenanceResult = await _maintenanceServices.GetAllMaintenanceAsync();
            if (maintenanceResult.Data != null)
            {
                viewModel.MaintenanceHistory = maintenanceResult.Data.Where(m => m.PropertyId == id).OrderByDescending(m => m.CreatedAt);
            }

            if (leases != null)
            {
                foreach (var lease in leases) 
                {
                     var tenant = await _tenantServices.GetTenantByIdAsync(lease.TenantId);
                     if (tenant != null) viewModel.Tenants[lease.Id] = tenant;

                     var payments = await _rentServices.GetPaymentsByLeaseIdAsync(lease.Id);
                     if (payments.Data != null) viewModel.Payments[lease.Id] = payments.Data;
                }
            }

            return View(viewModel);
        }
    }
}

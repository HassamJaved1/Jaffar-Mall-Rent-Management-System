using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class TenantController : Controller
    {
        private readonly TenantServices _tenantServices;
        private readonly LeaseServices _leaseServices;
        private readonly PropertyServices _propertyServices;
        private readonly RentServices _rentServices;
        private readonly IWebHostEnvironment _environment;

        public TenantController(TenantServices tenantServices, LeaseServices leaseServices, PropertyServices propertyServices, RentServices rentServices, IWebHostEnvironment environment)
        {
            _tenantServices = tenantServices;
            _leaseServices = leaseServices;
            _propertyServices = propertyServices;
            _rentServices = rentServices;
            _environment = environment;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? sort = null)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var viewModel = await _tenantServices.GetAllTenantsAsync(page, pageSize, search, sort);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TenantListPartial", viewModel);
            }

            return View(viewModel);
        }


        public IActionResult AddTenant()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddTenant([FromForm] Tenant tenant, IFormFile? idCardImage)
        {
            if (idCardImage != null)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "id_cards");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + idCardImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await idCardImage.CopyToAsync(fileStream);
                    }

                    tenant.IdCardImageUrl = "/uploads/id_cards/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return BackendResponse<bool>.Failure("Error saving ID Card image: " + ex.Message, 500)
                                                .ToActionResult();
                }
            }
            else
            {
                 return BackendResponse<bool>.Failure("ID Card image is mandatory.", 400)
                                                .ToActionResult();
            }

            var response = await _tenantServices.AddTenantAsync(tenant);

            if (response.Data)
            {
                return BackendResponse<bool>.Success(true, response.Message)
                                            .ToActionResult();
            }

            return BackendResponse<bool>.Failure(response.Message, response.Code)
                                        .ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> EditTenant(long id)
        {
            var tenant = await _tenantServices.GetTenantByIdAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            return View(tenant);
        }

        [HttpPost]
        public async Task<IActionResult> EditTenant([FromForm] Tenant tenant, IFormFile? idCardImage)
        {
            var existingTenant = await _tenantServices.GetTenantByIdAsync(tenant.Id);
            if (existingTenant == null)
            {
                return BackendResponse<bool>.Failure("Tenant not found.", 404).ToActionResult();
            }

            if (idCardImage != null)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "id_cards");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + idCardImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await idCardImage.CopyToAsync(fileStream);
                    }

                    tenant.IdCardImageUrl = "/uploads/id_cards/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return BackendResponse<bool>.Failure("Error saving ID Card image: " + ex.Message, 500).ToActionResult();
                }
            }
            else
            {
                // Preserve existing image if no new one is uploaded
                tenant.IdCardImageUrl = existingTenant.IdCardImageUrl;
            }

            var response = await _tenantServices.UpdateTenantAsync(tenant);
            if (response.Data)
            {
                return BackendResponse<bool>.Success(true, response.Message)
                                            .ToActionResult();
            }

            return BackendResponse<bool>.Failure(response.Message, response.Code)
                                        .ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTenant(long id)
        {
            var response = await _tenantServices.DeleteTenantAsync(id);
            if (response.Data)
            {
                return BackendResponse<bool>.Success(true, response.Message)
                                            .ToActionResult();
            }
             return BackendResponse<bool>.Failure(response.Message, response.Code)
                                        .ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var tenant = await _tenantServices.GetTenantByIdAsync(id);
            if (tenant == null) return NotFound();

            var leases = await _leaseServices.GetLeasesByTenantIdAsync(id);
            
            var viewModel = new TenantDetailsViewModel 
            {
                Tenant = tenant,
                Leases = leases ?? new List<PropertyLease>(),
                Properties = new Dictionary<long, Property>(),
                Payments = new Dictionary<long, IEnumerable<RentPayment>>()
            };

            if (leases != null)
            {
                foreach (var lease in leases) 
                {
                     var property = await _propertyServices.GetPropertyByIdAsync(lease.PropertyId);
                     if (property != null) viewModel.Properties[lease.Id] = property;

                     var payments = await _rentServices.GetPaymentsByLeaseIdAsync(lease.Id);
                     if (payments.Data != null) viewModel.Payments[lease.Id] = payments.Data;
                }
            }

            return View(viewModel);
        }
    }
}

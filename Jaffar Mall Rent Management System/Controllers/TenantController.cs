using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class TenantController : Controller
    {
        private readonly TenantServices _tenantServices;
        public TenantController(TenantServices tenantServices)
        {
            _tenantServices = tenantServices;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] string? search = null)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var viewModel = await _tenantServices.GetAllTenantsAsync(page, pageSize, search);

            ViewBag.CurrentSearch = search;

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
        public async Task<IActionResult> AddTenant([FromBody] Tenant tenant)
        {
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
        public async Task<IActionResult> EditTenant([FromBody] Tenant tenant)
        {
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
    }
}

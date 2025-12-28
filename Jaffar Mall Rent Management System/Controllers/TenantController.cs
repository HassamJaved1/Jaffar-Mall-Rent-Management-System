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

        public IActionResult Index()
        {
            return View();
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
    }
}

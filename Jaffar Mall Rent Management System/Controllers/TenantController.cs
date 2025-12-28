using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

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
            if (tenant == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(tenant);
            try
            {
                // repository will set timestamps
                var added = await _tenantServices.AddTenantAsync(tenant);
                if (added.Data)
                    return RedirectToAction("Index");

                ViewData["Error"] = "Unable to save property. Please try again.";
                return View(tenant);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewData["Error"] = "An error occurred while saving the property.";
                return View(tenant);
            }
        }
    }
}

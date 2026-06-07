using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class DocumentController : Controller
    {
        private readonly DocumentServices _documentServices;
        private readonly TenantServices _tenantServices;

        public DocumentController(DocumentServices documentServices, TenantServices tenantServices)
        {
            _documentServices = documentServices;
            _tenantServices = tenantServices;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _documentServices.GetAllDocumentsAsync();
            return View(result.Data);
        }

        public async Task<IActionResult> Add()
        {
            var tenantsResponse = await _tenantServices.GetAllTenantsAsync();
            ViewBag.Tenants = tenantsResponse;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(string description, long tenantId, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(description) || tenantId <= 0 || file == null)
            {
                ModelState.AddModelError("", "All fields are required.");
                var tenantsResponse = await _tenantServices.GetAllTenantsAsync();
                ViewBag.Tenants = tenantsResponse;
                return View();
            }

            var result = await _documentServices.AddDocumentAsync(description, tenantId, file);
            if (result.Data)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.Message);
            var tenants = await _tenantServices.GetAllTenantsAsync();
            ViewBag.Tenants = tenants;
            return View();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _documentServices.DeleteDocumentAsync(id);
            if (result.Data)
            {
                return Json(new { success = true, message = result.Message });
            }
            
            return Json(new { success = false, message = result.Message });
        }
    }
}

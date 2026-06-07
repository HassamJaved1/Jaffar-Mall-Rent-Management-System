using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly MaintenanceServices _maintenanceServices;
        private readonly PropertyServices _propertyServices;
        private readonly EmailService _emailService;

        public MaintenanceController(MaintenanceServices maintenanceServices, PropertyServices propertyServices, EmailService emailService)
        {
            _maintenanceServices = maintenanceServices;
            _propertyServices = propertyServices;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _maintenanceServices.GetAllMaintenanceAsync();
            var props = await _propertyServices.GetAllPropertiesAsync(); 
            ViewBag.Properties = props ?? new List<Property>();
            
            return View(result.Data);
        }

        public async Task<IActionResult> Add()
        {
            var props = await _propertyServices.GetAllPropertiesAsync();
            ViewBag.Properties = props ?? new List<Property>();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Maintenance maintenance)
        {
            maintenance.Status = (byte)MaintenanceStatus.Resolved; // Default to resolved if it's an after-the-fact voucher entry
            var result = await _maintenanceServices.AddMaintenanceAsync(maintenance);
            
            if (result.Code == 200)
            {
                // Result.Data contains the new integer ID for the record.
                maintenance.Id = result.Data; 
                
                // Dispatch owner email
                var prop = await _propertyServices.GetPropertyByIdAsync(maintenance.PropertyId);
                if (prop != null)
                {
                    await _emailService.SendMaintenanceVoucherEmailAsync(maintenance, prop, false);
                }
                
                return RedirectToAction(nameof(Voucher), new { id = maintenance.Id });
            }
            
            ModelState.AddModelError("", result.Message);
            var props = await _propertyServices.GetAllPropertiesAsync();
            ViewBag.Properties = props ?? new List<Property>();
            return View("Add", maintenance);
        }

        public async Task<IActionResult> Voucher(long id)
        {
            var result = await _maintenanceServices.GetMaintenanceByIdAsync(id);
            if (result.Data == null) return NotFound();
            
            var props = await _propertyServices.GetAllPropertiesAsync();
            ViewBag.Properties = props ?? new List<Property>();
            
            return View(result.Data);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var result = await _maintenanceServices.GetMaintenanceByIdAsync(id);
            if (result.Data == null) return NotFound();
            
            var props = await _propertyServices.GetAllPropertiesAsync();
            ViewBag.Properties = props ?? new List<Property>();
            
            return View("Edit", result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Maintenance maintenance)
        {
            var result = await _maintenanceServices.UpdateMaintenanceAsync(maintenance);
            
            if (result.Code == 200)
            {
                // Dispatch owner email
                var prop = await _propertyServices.GetPropertyByIdAsync(maintenance.PropertyId);
                if (prop != null)
                {
                    await _emailService.SendMaintenanceVoucherEmailAsync(maintenance, prop, true);
                }

                return RedirectToAction(nameof(Voucher), new { id = maintenance.Id });
            }
            
            ModelState.AddModelError("", result.Message);
            var props = await _propertyServices.GetAllPropertiesAsync();
            ViewBag.Properties = props ?? new List<Property>();
            return View("Edit", maintenance);
        }
    }
}

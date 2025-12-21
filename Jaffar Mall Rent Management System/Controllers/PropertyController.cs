using Microsoft.AspNetCore.Mvc;
using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Repositories;
using Jaffar_Mall_Rent_Management_System.Services;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyServices _propertyServices;

        public PropertyController(PropertyServices propertyServices)
        {
            _propertyServices = propertyServices;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddProperty()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProperty(Property property)
        {
            if (property == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(property);

            try
            {
                // repository will set timestamps
                var added = await _propertyServices.AddPropertyAsync(property);
                if (added.Data)
                    return RedirectToAction("Index");

                ViewData["Error"] = "Unable to save property. Please try again.";
                return View(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewData["Error"] = "An error occurred while saving the property.";
                return View(property);
            }
        }
    }
}



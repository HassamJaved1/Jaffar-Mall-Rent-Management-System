using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyServices _propertyServices;

        public PropertyController(PropertyServices propertyServices)
        {
            _propertyServices = propertyServices;
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
            // repository will set timestamps
            var response = await _propertyServices.AddPropertyAsync(property);
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



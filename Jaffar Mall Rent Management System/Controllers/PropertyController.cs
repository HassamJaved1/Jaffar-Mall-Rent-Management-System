using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class PropertyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddProperty()
        {
            return View();
        }

    }
}

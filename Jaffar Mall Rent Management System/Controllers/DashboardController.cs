using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Jaffar_Mall_Rent_Management_System.Services;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardServices _dashboardServices;

        public DashboardController(DashboardServices dashboardServices)
        {
            _dashboardServices = dashboardServices;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dashboardServices.GetDashboardDataAsync();
            return View(model);
        }
    }
}

using Jaffar_Mall_Rent_Management_System.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class LeasesController : Controller
    {
        private readonly LeaseServices _leaseServices;

        public LeasesController(LeaseServices leaseServices) 
        {
            _leaseServices = leaseServices;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _leaseServices.PopulateDropdowns();
            return View();
        }
    }
}

using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Services;
using Jaffar_Mall_Rent_Management_System.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserAuthService _authService;

        public LoginController(UserAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var auth = await _authService.AuthenticateUserAsync(model.Username, model.Password);

            if (auth.Code != 200)
            {
                ModelState.AddModelError("", auth.Message);
                return View(model);
            }

            // login success, redirect
            return RedirectToAction("Index", "Dashboard");
        }
    }
}




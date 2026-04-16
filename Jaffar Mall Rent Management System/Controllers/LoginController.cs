using Jaffar_Mall_Rent_Management_System.Models.ViewModels;
using Jaffar_Mall_Rent_Management_System.Services;
using Jaffar_Mall_Rent_Management_System.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jaffar_Mall_Rent_Management_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserAuthService _authService;

        public LoginController(UserAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel());
        }
        
        [AllowAnonymous]
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

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Username)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            // login success, redirect
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }
    }
}



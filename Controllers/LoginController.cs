using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesOrders.Models;
using System.Threading.Tasks;

namespace SalesOrders.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        // Constructor that takes SignInManager as a dependency
        public LoginController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager; // Initialize the _signInManager field
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt."); // Error message for invalid login
            }
            return View(loginDto); // Return the model to show errors
        }
    }
}

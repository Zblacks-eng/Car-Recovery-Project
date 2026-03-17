using Microsoft.AspNetCore.Mvc;
using MobileCarRecoverySystem.Models;
using MobileCarRecoverySystem.Services;

namespace MobileCarRecoverySystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService accountService;

        public AccountController(AccountService accountService)
        {
            this.accountService = accountService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            bool success = accountService.Login(username, password);

            if (success)
            {
                return RedirectToAction("Index", "Recovery");
            }

            ViewBag.Message = "Invalid username or password";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserAccount user)
        {
            bool success = accountService.Register(user);

            if (success)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Message = "Username already exists";
            return View();
        }
    }
}
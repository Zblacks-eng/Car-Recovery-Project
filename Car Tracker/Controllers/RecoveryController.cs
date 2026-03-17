using Microsoft.AspNetCore.Mvc;
using MobileCarRecoverySystem.Models;
using MobileCarRecoverySystem.Services;

namespace MobileCarRecoverySystem.Controllers
{
    public class RecoveryController : Controller
    {
        private readonly RecoveryService service;

        public RecoveryController(RecoveryService service)
        {
            this.service = service;
        }

        public IActionResult Index()
        {
            var data = service.GetAll();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RecoveryCase item)
        {
            if (string.IsNullOrWhiteSpace(item.CustomerName) ||
                string.IsNullOrWhiteSpace(item.VehicleRegistration))
            {
                ViewBag.Message = "Customer name and vehicle registration are required.";
                return View();
            }

            service.AddCase(item);
            return RedirectToAction("Index");
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string registration)
        {
            var result = service.Search(registration);
            return View(result);
        }

        [HttpPost]
        public IActionResult Delete(string registration)
        {
            service.Remove(registration);
            return RedirectToAction("Index");
        }
    }
}
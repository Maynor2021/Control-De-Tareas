using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Control_De_Tareas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ContextDB _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ContextDB context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
         //   HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Login


  
        

        // Register
        public IActionResult Register()
        {
            return View();
        }

        // Password Recovery
        public IActionResult PasswordRecovery()
        {
            return View();
        }

        // Code Verification
        public IActionResult VerifyCode()
        {
            return View();
        }

        // Password Change
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
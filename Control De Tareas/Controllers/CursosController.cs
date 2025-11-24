using Control_De_Tareas.Authorization;
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    [ProfesorOAdminAuthorize] // Solo profesores y admin
    public class CursosController : Controller
    {
        private ContextDB _context;
        private ILogger<CursosController> _logger;

        public CursosController(ContextDB context, ILogger<CursosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var viewModel = new CursosVm();

           

            return View(viewModel);
        }
    }
}
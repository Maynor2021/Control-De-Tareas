using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Control_De_Tareas.Data;

namespace Control_De_Tareas.Controllers
{
    [Authorize] // Requiere autenticación
    public class TareasController : Controller
    {
        private Context _context;
        private ILogger<TareasController> _logger;
        
        public TareasController(Context context, ILogger<TareasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Todos pueden ver
        public IActionResult Index()
        {
            return View();
        }

        // Solo profesores pueden crear
        [ProfesorAuthorize]
        public IActionResult Crear() => View();

        [HttpPost]
        [ProfesorAuthorize]
        public IActionResult Crear(Tareas tarea)
        {
            if (ModelState.IsValid)
            {
                _context.Tareas.Add(tarea);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tarea);
        }

        // Solo estudiantes pueden entregar
        [EstudianteAuthorize]
        public IActionResult Entregar(Guid id) => View();
    }
}

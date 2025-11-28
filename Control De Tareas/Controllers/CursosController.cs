using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    public class CursosController : Controller
    {
        private ContextDB _context;
        private ILogger<CursosController> _logger;

        public CursosController(ContextDB context, ILogger<CursosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Courses
                .Where(c => !c.IsSoftDeleted)
                .Select(c => new CursoDto
                {
                    Id = c.Id, // ← Ya usa Guid
                    Codigo = c.Code,
                    Nombre = c.Title,
                    Descripcion = c.Description ?? "",
                    Estado = c.IsActive ? "Activo" : "Inactivo",
                    InstructorNombre = "Por asignar",
                    CantidadEstudiantes = 0
                })
                .ToListAsync();

            var viewModel = new CursosVm
            {
                Cursos = cursos
            };

            return View(viewModel);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador,Profesor")]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador,Profesor")]
        public async Task<IActionResult> Crear(string Codigo, string Nombre, string Descripcion)
        {
            if (string.IsNullOrEmpty(Codigo) || string.IsNullOrEmpty(Nombre))
            {
                TempData["Error"] = "Código y Nombre son requeridos";
                return View();
            }

            var curso = new Courses
            {
                Id = Guid.NewGuid(),
                Code = Codigo,
                Title = Nombre,
                Description = Descripcion,
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsSoftDeleted = false
            };

            _context.Courses.Add(curso);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Curso creado exitosamente";
            return RedirectToAction(nameof(Index));
        }
    }
}
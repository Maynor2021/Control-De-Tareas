using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Authorization;
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
            var cursos = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Professor)
                .Include(co => co.Enrollments)
                .Where(co => !co.IsSoftDeleted)
                .Select(co => new CursoDto
                {
                    Id = co.Id,  // ← ID del CourseOffering
                    Codigo = co.Course.Code,
                    Nombre = co.Course.Title,
                    Descripcion = co.Course.Description ?? "",
                    Estado = co.IsActive ? "Activo" : "Inactivo",
                    InstructorNombre = co.Professor.UserName,
                    CantidadEstudiantes = co.Enrollments.Count(e => !e.IsSoftDeleted)
                })
                .ToListAsync();

            var viewModel = new CursosVm
            {
                Cursos = cursos
            };

            return View(viewModel);
        }

        // ========== SOLO ADMIN PUEDE CREAR CURSOS ==========
        [Authorize(Roles = "Administrador")]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
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

        public IActionResult MenuDetalles(Guid id)
        {
            // cambio en la tabla de course and courseOffering a Guid insteaof indt
            //la tabla courseOffering necesita cambiarse Guid sus propiedades , hayque normalizar 
            // Buscar el CourseOffering (no el Course)
            var courseOffering = _context.CourseOfferings
                .Include(co => co.Course) // Join con Courses para obtener datos generales
                .FirstOrDefault(co => co.Id == id);

            if (courseOffering == null)
            {
                return NotFound();
            }

            var model = new DetallesCursosVM
            {
                Id = courseOffering.Id,
                nombre = courseOffering.Course.Title, // Del Course relacionado
                descrption = courseOffering.Course.Description ?? string.Empty,

                // Conteos basados en CourseOfferingId
                TotalTareas = _context.Tareas
                    .Count(t => t.CourseOfferingId == id),

                TotalEnlaces = 1,
                TotalAnuncios = 2,
            };

            return View("DetallesMenu", model);
        }
    }
}


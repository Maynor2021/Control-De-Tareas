using Control_De_Tareas.Authorization;
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Control_De_Tareas.Controllers
{
    [Authorize] // requiere autenticación
    public class TareasController : Controller
    {
        private readonly ContextDB _context;
        private readonly ILogger<TareasController> _logger;

        public TareasController(ContextDB context, ILogger<TareasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ---------------------------------------------------
        // INDEX - TODOS PUEDEN VER LA LISTA DE TAREAS
        // ---------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var tareas = await _context.Tareas
                .Where(t => !t.IsSoftDeleted)
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tareas);
        }

        // ---------------------------------------------------
        // CREAR TAREA (GET) - SOLO ADMIN
        // ---------------------------------------------------
        [Authorize(Policy = "Admin")]
        public IActionResult Crear()
        {
            // Si necesitas enviar CourseOfferings a la vista:
            // ViewBag.CourseOfferings = _context.CourseOfferings.ToList();

            return View();
        }

        // ---------------------------------------------------
        // CREAR TAREA (POST) - SOLO ADMIN
        // ---------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Crear(TareaCreateVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Obtener GUID del usuario autenticado
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claim, out var userGuid))
            {
                _logger.LogWarning("Crear Tarea: Claim inválido");
                ModelState.AddModelError("", "No se pudo identificar al usuario.");
                return View(vm);
            }

            // Validar que existe el CourseOffering
            var existsCO = await _context.CourseOfferings.AnyAsync(co => co.Id == vm.CourseOfferingId);
            if (!existsCO)
            {
                ModelState.AddModelError(nameof(vm.CourseOfferingId), "La oferta de curso no existe.");
                return View(vm);
            }

            // Crear entidad Tareas
            var tarea = new Tareas
            {
                Id = Guid.NewGuid(),
                CourseOfferingId = vm.CourseOfferingId,
                Title = vm.Title,
                Description = vm.Description,
                DueDate = vm.DueDate,
                MaxScore = vm.MaxScore,
                CreatedBy = userGuid,
                IsSoftDeleted = false
            };

            try
            {
                _context.Tareas.Add(tarea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la tarea");
                ModelState.AddModelError("", "Error al guardar la tarea en la base de datos.");
                return View(vm);
            }
        }

        // ---------------------------------------------------
        // ENTREGAR (GET) - SOLO ESTUDIANTES
        // ---------------------------------------------------
        [EstudianteAuthorize]
        public IActionResult Entregar(int id)
        {
            // Puedes cargar la tarea aquí si quieres:
            // var tarea = _context.Tareas.Find(id);

            return View();
        }
    }
}
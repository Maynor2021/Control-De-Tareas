using Control_De_Tareas.Authorization;
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Control_De_Tareas.Controllers
{
    [Authorize]
    public class TareasController : Controller
    {
        private readonly ContextDB _context;
        private readonly ILogger<TareasController> _logger;

        public TareasController(ContextDB context, ILogger<TareasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var tareas = await _context.Tareas
                .Where(t => !t.IsSoftDeleted)
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(t => t.CreatedByUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tareas);
        }

        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Crear()
        {
            var courseOfferings = await _context.CourseOfferings
                .Where(co => !co.IsSoftDeleted && co.IsActive)
                .Include(co => co.Course)
                .Select(co => new SelectListItem
                {
                    Value = co.Id.ToString(),
                    Text = $"{co.Course.Code} - {co.Course.Title} - Sección {co.Section}"
                })
                .ToListAsync();

            ViewBag.CourseOfferings = courseOfferings;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Crear(TareaCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadCourseOfferingsAsync();
                return View(vm);
            }

            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claim, out var userGuid))
            {
                ModelState.AddModelError("", "No se pudo identificar al usuario.");
                await LoadCourseOfferingsAsync();
                return View(vm);
            }

            var existsCO = await _context.CourseOfferings.AnyAsync(co => co.Id == vm.CourseOfferingId);
            if (!existsCO)
            {
                ModelState.AddModelError(nameof(vm.CourseOfferingId), "La oferta de curso no existe.");
                await LoadCourseOfferingsAsync();
                return View(vm);
            }

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
                TempData["Success"] = "Tarea creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la tarea");
                ModelState.AddModelError("", "Error al guardar la tarea en la base de datos.");
                await LoadCourseOfferingsAsync();
                return View(vm);
            }
        }

        [Authorize(Roles = "Administrador,Profesor,Estudiante")]
        public async Task<IActionResult> Detalle(Guid id)
        {
            var tarea = await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsSoftDeleted);

            if (tarea == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Estudiante"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userId, out var studentId))
                {
                    var submission = await _context.Submissions
                        .FirstOrDefaultAsync(s => s.TaskId == id && s.StudentId == studentId);
                    ViewBag.Submission = submission;
                }
            }

            if (User.IsInRole("Profesor"))
            {
                var totalEntregas = await _context.Submissions
                    .CountAsync(s => s.TaskId == id && !s.IsSoftDeleted);
                var entregasCalificadas = await _context.Submissions
                    .CountAsync(s => s.TaskId == id && s.CurrentGrade.HasValue && !s.IsSoftDeleted);

                ViewBag.TotalEntregas = totalEntregas;
                ViewBag.EntregasCalificadas = entregasCalificadas;
            }

            return View(tarea);
        }

        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Editar(Guid id)
        {
            var tarea = await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsSoftDeleted);

            if (tarea == null)
            {
                return NotFound();
            }

            return View(tarea);
        }

        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tarea = await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsSoftDeleted);

            if (tarea == null)
            {
                return NotFound();
            }

            return View(tarea);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                tarea.IsSoftDeleted = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tarea eliminada correctamente";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Estudiante")]
        public async Task<IActionResult> TareasEstudiantes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var studentId))
            {
                return RedirectToAction("Login", "Home");
            }

            var enrolledCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsSoftDeleted)
                .Select(e => e.CourseOfferingId)
                .ToListAsync();

            var tareasConSubmissions = await _context.Tareas
                .Where(t => enrolledCourses.Contains(t.CourseOfferingId) && !t.IsSoftDeleted)
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Select(t => new
                {
                    Tarea = t,
                    Submission = _context.Submissions
                        .FirstOrDefault(s => s.TaskId == t.Id && s.StudentId == studentId)
                })
                .ToListAsync();

            return View(tareasConSubmissions);
        }

        private async Task LoadCourseOfferingsAsync()
        {
            var courseOfferings = await _context.CourseOfferings
                .Where(co => !co.IsSoftDeleted && co.IsActive)
                .Include(co => co.Course)
                .Select(co => new SelectListItem
                {
                    Value = co.Id.ToString(),
                    Text = $"{co.Course.Code} - {co.Course.Title} - Sección {co.Section}"
                })
                .ToListAsync();

            ViewBag.CourseOfferings = courseOfferings;
        }
    }
}
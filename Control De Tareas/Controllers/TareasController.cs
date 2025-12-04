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

        // ========================= LISTADO GENERAL =========================
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

        // ========================= CREAR =========================
        [Authorize(Roles = "Profesor,Administrador")]
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
        [Authorize(Roles = "Profesor,Administrador")]
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

        // ========================= DETALLE (✅ S3-12 SEGURIDAD TOTAL) =========================
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

            // ✅ S3-12 VALIDACIÓN DE SEGURIDAD POR INSCRIPCIÓN
            if (User.IsInRole("Estudiante"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (Guid.TryParse(userId, out var studentId))
                {
                    var tieneAcceso = await _context.Enrollments
                        .AnyAsync(e =>
                            e.StudentId == studentId &&
                            e.CourseOfferingId == tarea.CourseOfferingId &&
                            e.Status == "Active" &&
                            !e.IsSoftDeleted);

                    if (!tieneAcceso)
                    {
                        // ❌ ACCESO BLOQUEADO SI INTENTA ENTRAR A TAREA DE OTRO CURSO
                        return Forbid(); // 403
                    }

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

        // ========================= EDITAR =========================
        [Authorize(Roles = "Profesor,Administrador")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Administrador")]
        public async Task<IActionResult> Editar(Guid id, [Bind("Id,CourseOfferingId,Title,Description,DueDate,MaxScore")] Tareas model)
        {
            _logger.LogInformation("POST Editar invocado | ID={Id}", id);

            if (id == Guid.Empty || model == null || id != model.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError(nameof(model.Title), "El título es obligatorio.");

            if (model.DueDate < DateTime.UtcNow.AddMinutes(-5))
                ModelState.AddModelError(nameof(model.DueDate), "La fecha de entrega debe ser futura.");

            if (!ModelState.IsValid)
            {
                var co = await _context.CourseOfferings.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == model.CourseOfferingId);
                ViewBag.CourseOffering = co;
                return View(model);
            }

            try
            {
                var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && !t.IsSoftDeleted);
                if (tarea == null)
                    return NotFound();

                tarea.Title = model.Title;
                tarea.Description = model.Description;
                tarea.DueDate = model.DueDate;
                tarea.MaxScore = model.MaxScore;

                _context.Tareas.Update(tarea);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tarea actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando tarea");
                ModelState.AddModelError("", "Ocurrió un error al editar la tarea.");
                return View(model);
            }
        }

        // ========================= DELETE =========================
        [Authorize(Roles = "Profesor,Administrador")]
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
        [Authorize(Roles = "Profesor,Administrador")]
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

        // ========================= TAREAS PARA ESTUDIANTES =========================
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

        // ========================= HELPERS =========================
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

using Control_De_Tareas.Authorization;
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Control_De_Tareas.Services; 
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
        private readonly AuditService _auditService; 

        public TareasController(ContextDB context, ILogger<TareasController> logger, AuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService; 
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

        // ---------- CREATE ----------
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

                //  AUDITORÍA: Creación de tarea
                await _auditService.LogCreateAsync("Tarea", tarea.Id, tarea.Title);

                TempData["Success"] = "Tarea creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la tarea");

                //  AUDITORÍA: Error al crear tarea
                await _auditService.LogAsync("TAREA_CREATE_ERROR", "Tarea", null,
                    $"Error al crear tarea '{vm.Title}': {ex.Message}");

                ModelState.AddModelError("", "Error al guardar la tarea en la base de datos.");
                await LoadCourseOfferingsAsync();
                return View(vm);
            }
        }

        // ---------- DETAILS ----------
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

        // ---------- EDIT (GET) ----------
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

        // ---------- EDIT (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Administrador")]
        public async Task<IActionResult> Editar(Guid id, [Bind("Id,CourseOfferingId,Title,Description,DueDate,MaxScore")] Tareas model)
        {
            _logger.LogInformation("👉 POST Editar invocado | ID={Id} | Usuario={User}", id, User?.Identity?.Name);

            if (id == Guid.Empty || model == null || id != model.Id)
            {
                return BadRequest();
            }

            // Validaciones servidor
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

                // Guardar valores antiguos para auditoría
                var oldValues = new
                {
                    tarea.Title,
                    tarea.Description,
                    tarea.DueDate,
                    tarea.MaxScore
                };

                // Actualizar
                tarea.Title = model.Title;
                tarea.Description = model.Description;
                tarea.DueDate = model.DueDate;
                tarea.MaxScore = model.MaxScore;

                _context.Tareas.Update(tarea);
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Actualización de tarea
                await _auditService.LogUpdateAsync("Tarea", tarea.Id, tarea.Title);

                TempData["Success"] = "Tarea actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException dex)
            {
                _logger.LogError(dex, "Concurrency error editando tarea {Id}", id);

                //  AUDITORÍA: Error de concurrencia
                await _auditService.LogAsync("TAREA_UPDATE_CONCURRENCY", "Tarea", id,
                    "Error de concurrencia al editar tarea");

                ModelState.AddModelError("", "La tarea fue modificada por otro usuario. Intenta de nuevo.");
                var co = await _context.CourseOfferings.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == model.CourseOfferingId);
                ViewBag.CourseOffering = co;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando tarea {Id}", id);

                //  AUDITORÍA: Error al actualizar tarea
                await _auditService.LogAsync("TAREA_UPDATE_ERROR", "Tarea", id,
                    $"Error al actualizar tarea: {ex.Message}");

                ModelState.AddModelError("", "Ocurrió un error al editar la tarea.");
                var co = await _context.CourseOfferings.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == model.CourseOfferingId);
                ViewBag.CourseOffering = co;
                return View(model);
            }
        }

        // ---------- DELETE (GET) ----------
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

        // ---------- DELETE (POST) ----------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Administrador")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                // Guardar info para auditoría antes de eliminar
                var tareaInfo = new
                {
                    tarea.Title,
                    tarea.CourseOfferingId,
                    tarea.CreatedBy
                };

                tarea.IsSoftDeleted = true;
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Eliminación de tarea (soft delete)
                await _auditService.LogDeleteAsync("Tarea", tarea.Id, tarea.Title);

                TempData["Success"] = "Tarea eliminada correctamente";
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------- TAREAS PARA ESTUDIANTES ----------
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

            // AUDITORÍA: Acceso a tareas por estudiante
            await _auditService.LogAsync("VIEW_TAREAS_ESTUDIANTE", "Tarea", null,
                $"Estudiante accedió a sus tareas ({tareasConSubmissions.Count} tareas)");

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
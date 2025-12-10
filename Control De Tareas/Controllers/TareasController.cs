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

        // ========================= CREATE =========================
        [Authorize(Roles = "Profesor,Administrador")]
        public async Task<IActionResult> Crear()
        {
            await LoadCourseOfferingsAsync();
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

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            await _auditService.LogCreateAsync("Tarea", tarea.Id, tarea.Title);

            TempData["Success"] = "Tarea creada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // ========================= DETALLE =========================
        [Authorize(Roles = "Administrador,Profesor,Estudiante")]
        public async Task<IActionResult> Detalle(Guid id)
        {
            var tarea = await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsSoftDeleted);

            if (tarea == null)
                return NotFound();

            if (User.IsInRole("Estudiante"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userId, out var studentId))
                {
                    var tieneAcceso = await _context.Enrollments.AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.CourseOfferingId == tarea.CourseOfferingId &&
                        e.Status == "Active" &&
                        !e.IsSoftDeleted);

                    if (!tieneAcceso)
                        return Forbid();

                    var submission = await _context.Submissions
                        .FirstOrDefaultAsync(s => s.TaskId == id && s.StudentId == studentId);

                    ViewBag.Submission = submission;
                }
            }

            if (User.IsInRole("Profesor"))
            {
                ViewBag.TotalEntregas = await _context.Submissions.CountAsync(s => s.TaskId == id);
                ViewBag.EntregasCalificadas = await _context.Submissions.CountAsync(s => s.TaskId == id && s.CurrentGrade.HasValue);
            }

            return View(tarea);
        }

        // ========================= ✅ TAREAS PARA ESTUDIANTES (FIX REAL) =========================
        [Authorize(Policy = "Estudiante")]
        public async Task<IActionResult> TareasEstudiantes(Guid? courseOfferingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var studentId))
                return RedirectToAction("Login", "Home");

            var enrolledCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId && !e.IsSoftDeleted)
                .Select(e => e.CourseOfferingId)
                .ToListAsync();

            var tareasQuery = _context.Tareas
                .Where(t => enrolledCourses.Contains(t.CourseOfferingId) && !t.IsSoftDeleted)
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .AsQueryable();

            // ✅ FILTRO POR CURSO CUANDO SE DA CLICK EN "VER"
            if (courseOfferingId.HasValue)
            {
                tareasQuery = tareasQuery.Where(t => t.CourseOfferingId == courseOfferingId.Value);
            }

            var tareasConSubmissions = await tareasQuery
                .Select(t => new
                {
                    Tarea = t,
                    Submission = _context.Submissions
                        .FirstOrDefault(s => s.TaskId == t.Id && s.StudentId == studentId)
                })
                .ToListAsync();

            await _auditService.LogAsync("VIEW_TAREAS_ESTUDIANTE", "Tarea", null,
                $"Estudiante accedió a tareas ({tareasConSubmissions.Count})");

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

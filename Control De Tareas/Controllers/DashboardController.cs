using Control_De_Tareas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Control_De_Tareas.Controllers
{
    public class DashboardController : Controller
    {
        private readonly Context _context;

        public DashboardController(Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Administrador")) return RedirectToAction("Admin");
            if (User.IsInRole("Profesor")) return RedirectToAction("Profesor");
            if (User.IsInRole("Estudiante")) return RedirectToAction("Estudiante");

            return RedirectToAction("Login", "Home");
        }

        // ==== DASHBOARD ADMIN ====
        public async Task<IActionResult> Admin()
        {
            var model = new
            {
                TotalUsuarios = await _context.Users.CountAsync(),
                TotalCursos = await _context.Courses.CountAsync(),
                TotalTareas = await _context.Tareas.CountAsync(),

                UltimasEntregas = await _context.Submissions
                    .Include(s => s.Task)
                    .Include(s => s.Student)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(8)
                    .ToListAsync()
            };

            return View(model);
        }

        // ==== DASHBOARD PROFESOR ====
        public async Task<IActionResult> Profesor()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId))
                return RedirectToAction("Login", "Home");

            // Cursos impartidos
            var cursosDelProfesor = await _context.CourseOfferings
                .Include(co => co.Course)
                .Where(co => co.ProfessorId == userId)
                .ToListAsync();

            // Tareas creadas por el profesor
            var tareasDelProfesor = await _context.Tareas
                .Where(t => t.CreatedBy == userId)
                .OrderByDescending(t => t.DueDate)
                .Take(10)
                .ToListAsync();

            // ID de course offerings
            var offeringIds = cursosDelProfesor
                .Select(c => c.Id)
                .ToList();

            // Tareas asociadas a esos offering
            var tareasIds = await _context.Tareas
                .Where(t => offeringIds.Contains(t.CourseOfferingId))
                .Select(t => t.Id)
                .ToListAsync();

            // Entregas pendientes
            var pendientes = await _context.Submissions
                .Include(s => s.Task)
                .Where(s => s.CurrentGrade == null && tareasIds.Contains(s.TaskId))
                .ToListAsync();

            var model = new
            {
                Cursos = cursosDelProfesor,
                Tareas = tareasDelProfesor,
                PendientesPorCalificar = pendientes
            };

            return View(model);
        }

        // ==== DASHBOARD ESTUDIANTE ====
        public async Task<IActionResult> Estudiante()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId))
                return RedirectToAction("Login", "Home");

            // Cursos inscritos
            var cursosInscritos = await _context.Enrollments
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Where(e => e.StudentId == userId)
                .ToListAsync();

            var offeringIds = cursosInscritos.Select(e => e.CourseOfferingId).ToList();

            // Tareas de esos offerings
            var tareas = await _context.Tareas
                .Where(t => offeringIds.Contains(t.CourseOfferingId))
                .ToListAsync();

            // Entregas realizadas por estudiante
            var entregadasIds = await _context.Submissions
                .Where(s => s.StudentId == userId)
                .Select(s => s.TaskId)
                .ToListAsync();

            // Tareas que faltan entregar
            var pendientes = tareas
                .Where(t => !entregadasIds.Contains(t.Id))
                .ToList();

            // Últimas calificaciones
            var calificaciones = await _context.Submissions
                .Where(s => s.StudentId == userId && s.CurrentGrade != null)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(6)
                .ToListAsync();

            var model = new
            {
                CursosInscritos = cursosInscritos,
                TareasPendientes = pendientes,
                UltimasCalificaciones = calificaciones
            };

            return View(model);
        }
    }
}

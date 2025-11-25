using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Control_De_Tareas.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ContextDB _context;
        private readonly IHttpContextAccessor _httpAccessor;

        public DashboardController(ContextDB context, IHttpContextAccessor httpAccessor)
        {
            _context = context;
            _httpAccessor = httpAccessor;
        }

        private UserVm GetCurrentUser()
        {
            var encoded = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(encoded)) return null;

            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            return JsonConvert.DeserializeObject<UserVm>(json);
        }

        public IActionResult Index()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Home");

            string rol = user.Rol.Nombre;

            if (rol == "Administrador") return RedirectToAction(nameof(Admin));
            if (rol == "Profesor") return RedirectToAction(nameof(Profesor));
            if (rol == "Estudiante") return RedirectToAction(nameof(Estudiante));

            return RedirectToAction("Login", "Home");
        }

        // ===================== ADMIN ============================
        public async Task<IActionResult> Admin()
        {
            var vm = new AdminDashboardVm
            {
                TotalUsuarios = await _context.Users.CountAsync(),
                TotalCursos = await _context.Courses.CountAsync(),
                TotalTareas = await _context.Tareas.CountAsync(),

                UltimasActividades = await _context.AuditLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(8)
                    .Select(a => $"{a.CreatedAt:dd/MM HH:mm} - {a.Action} - {a.Entity}")
                    .ToListAsync()
            };

            return View(vm);
        }

        // ===================== PROFESOR ============================
        public async Task<IActionResult> Profesor()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Home");

            Guid profesorId = user.UserId;

            var cursos = await _context.CourseOfferings
                .Include(c => c.Course)
                .Where(c => c.ProfessorId == profesorId)
                .ToListAsync();

            var tareas = await _context.Tareas
                .Where(t => t.CreatedBy == profesorId && !t.IsSoftDeleted)
                .OrderByDescending(t => t.DueDate)
                .Take(20)
                .Select(t => new TareaVm
                {
                    Id = t.Id,
                    Titulo = t.Title,
                    Descripcion = t.Description,
                    FechaEntrega = t.DueDate,
                    MaxScore = t.MaxScore
                })
                .ToListAsync();

            var vm = new ProfesorDashboardVm
            {
                Profesor = user.Nombre,
                Cursos = cursos.Select(c => new CursoVm
                {
                    Id = c.Id,
                    Codigo = c.Course.Code,
                    Titulo = c.Course.Title,
                    Seccion = c.Section
                }).ToList(),
                TareasPorCalificar = tareas
            };

            return View(vm);
        }

        // ===================== ESTUDIANTE ============================
        public async Task<IActionResult> Estudiante()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Home");

            Guid estudianteId = user.UserId;

            var enrolls = await _context.Enrollments
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Where(e => e.StudentId == estudianteId)
                .ToListAsync();

            var cursosInscritos = enrolls.Select(e => new CursoInscritoVm
            {
                CourseOfferingId = e.CourseOfferingId,
                Curso = e.CourseOffering.Course.Title,
                Seccion = e.CourseOffering.Section,
                Profesor = ""
            }).ToList();

            var offeringIds = enrolls.Select(e => e.CourseOfferingId).ToList();

            var tareas = await _context.Tareas
                .Where(t => offeringIds.Contains(t.CourseOfferingId))
                .ToListAsync();

            var entregadas = await _context.Submissions
                .Where(s => s.StudentId == estudianteId)
                .Select(s => s.TaskId)
                .ToListAsync();

            var pendientes = tareas
                .Where(t => !entregadas.Contains(t.Id))
                .Select(t => new EntregaPendienteVm
                {
                    TareaId = t.Id,
                    TituloTarea = t.Title,
                    FechaEntrega = t.DueDate
                })
                .ToList();

            var ultimasCalificaciones = await _context.Submissions
                .Include(s => s.Task)
                    .ThenInclude(t => t.CourseOffering)
                        .ThenInclude(co => co.Course)
                .Where(s => s.StudentId == estudianteId && s.CurrentGrade != null)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(6)
                .Select(s => new CalificacionVm
                {
                    Curso = s.Task.CourseOffering.Course.Title,
                    Tarea = s.Task.Title,
                    Nota = s.CurrentGrade ?? 0,
                    Fecha = s.SubmittedAt
                })
                .ToListAsync();

            decimal? promedio = null;
            var notas = await _context.Submissions
                .Where(s => s.StudentId == estudianteId && s.CurrentGrade != null)
                .Select(s => s.CurrentGrade)
                .ToListAsync();

            if (notas.Any()) promedio = notas.Average();

            var proximas = tareas
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new TareaVm
                {
                    Id = t.Id,
                    Titulo = t.Title,
                    Descripcion = t.Description,
                    FechaEntrega = t.DueDate,
                    MaxScore = t.MaxScore
                }).ToList();

            var vm = new EstudianteDashboardVm
            {
                NombreEstudiante = user.Nombre,
                CursosInscritos = cursosInscritos,
                TareasPendientes = pendientes,
                UltimasCalificaciones = ultimasCalificaciones,
                PromedioGeneral = promedio,
                PendientesPorCurso = new List<TareasPorCursoVm>(),
                ProximasEntregas = proximas
            };

            return View(vm);
        }
    }
}

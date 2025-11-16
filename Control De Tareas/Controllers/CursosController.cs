using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Control_De_Tareas.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    [ProfesorOAdminAuthorize] // Solo profesores y admin
    public class CursosController : Controller
    {
        private Context _context;
        private ILogger<CursosController> _logger;

        public CursosController(Context context, ILogger<CursosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var viewModel = new CursosVm();

            try
            {
                var cursos = _context.CourseOfferings
                    .Include(co => co.Course)
                    .Include(co => co.Professor)
                    .Include(co => co.Enrollments)
                    .Select(co => new CursoDto
                    {
                        Id = co.Id,
                        Codigo = co.Course.Code,
                        Nombre = co.Course.Title,
                        InstructorNombre = co.Professor.FullName ?? co.Professor.UserName,
                        CantidadEstudiantes = co.Enrollments.Count(e => !e.IsSoftDeleted && e.Status == "Active"),
                        Estado = co.IsActive ? "Activo" : "Inactivo"
                    })
                    .ToList();

                viewModel.Cursos = cursos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los cursos");
                viewModel.Cursos = new List<CursoDto>();
            }

            return View(viewModel);
        }
    }
}
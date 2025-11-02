using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
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
               
                var cursos = _context.Courses
                    .Include(c => c.Instructor)
                    .Select(c => new CursoDto
                    {
                        Id = c.Id,
                        Codigo = c.Codigo,
                        Nombre = c.Nombre,
                        InstructorNombre=c.Instructor.Instructor,
                        CantidadEstudiantes = 0,
                        Estado = c.Estado
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

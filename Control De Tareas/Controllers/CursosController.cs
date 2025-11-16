using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Control_De_Tareas.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar la visualización y administración de cursos.
    /// Solo accesible para usuarios con rol de Profesor o Administrador.
    /// </summary>
    [ProfesorOAdminAuthorize] // Solo profesores y admin
    public class CursosController : Controller
    {
        private readonly Context _context;
        private readonly ILogger<CursosController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de cursos.
        /// </summary>
        /// <param name="context">Contexto de base de datos EF Core.</param>
        /// <param name="logger">Servicio de logging para registrar errores y eventos.</param>
        public CursosController(Context context, ILogger<CursosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Muestra la lista de cursos registrados en la plataforma.
        /// Carga el instructor asociado y genera un modelo listo para la vista.
        /// </summary>
        /// <returns>Vista con el modelo que contiene la lista de cursos.</returns>
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
                        InstructorNombre = c.Instructor != null ? c.Instructor.Instructor : "",
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

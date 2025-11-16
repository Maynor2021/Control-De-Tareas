using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control_De_Tareas.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar las tareas del sistema.
    /// Permite listarlas, crearlas y entregar tareas según el rol del usuario.
    /// </summary>
    [Authorize] // Requiere autenticación
    public class TareasController : Controller
    {
        private readonly Context _context;
        private readonly ILogger<TareasController> _logger;

        /// <summary>
        /// Constructor del controlador de tareas.
        /// Inicializa el contexto de base de datos y el servicio de logging.
        /// </summary>
        /// <param name="context">Contexto de la base de datos.</param>
        /// <param name="logger">Servicio de logging para registrar información y errores.</param>
        public TareasController(Context context, ILogger<TareasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Muestra la página principal de tareas.
        /// Vista accesible para cualquier usuario autenticado.
        /// </summary>
        /// <returns>Vista de listado de tareas.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Muestra el formulario para que un profesor pueda crear una nueva tarea.
        /// Solo profesores tienen acceso a esta acción.
        /// </summary>
        /// <returns>Vista del formulario de creación.</returns>
        [ProfesorAuthorize]
        public IActionResult Crear() => View();

        /// <summary>
        /// Recibe la información del formulario y registra una nueva tarea en el sistema.
        /// Solo accesible por profesores.
        /// </summary>
        /// <param name="tarea">Objeto Tareas que contiene los datos enviados desde el formulario.</param>
        /// <returns>Redirección a Index si se crea correctamente; vista con errores si falla.</returns>
        [HttpPost]
        [ProfesorAuthorize]
        public IActionResult Crear(Tareas tarea)
        {
            if (ModelState.IsValid)
            {
                _context.Tareas.Add(tarea);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tarea);
        }

        /// <summary>
        /// Vista para que un estudiante entregue una tarea asignada.
        /// Solo usuarios con rol de estudiante pueden acceder.
        /// </summary>
        /// <param name="id">Id de la tarea que se entregará.</param>
        /// <returns>Vista del formulario de entrega.</returns>
        [EstudianteAuthorize]
        public IActionResult Entregar(Guid id) => View();
    }
}

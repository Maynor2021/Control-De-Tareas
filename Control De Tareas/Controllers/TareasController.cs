using Control_De_Tareas.Data.Entitys;
using Microsoft.AspNetCore.Mvc;

namespace Control_De_Tareas.Controllers
{
    public class TareasController : Controller
    {
        private Context _context;
        private ILogger<TareasController> _logger;
        public TareasController(Context context, ILogger<TareasController> logger)
        {
            _context = context;
            _logger = logger;
        }


        public IActionResult Index()
        {
            return View();
        }

    }
}

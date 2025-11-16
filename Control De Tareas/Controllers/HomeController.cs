using System.Diagnostics;
using System.Security.Claims;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    /// <summary>
    /// Controlador principal encargado de manejar navegación general,
    /// login, logout y vistas informativas del sistema.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly Context _context;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor de HomeController.
        /// Inicializa el contexto de base de datos y el logger del sistema.
        /// </summary>
        /// <param name="context">Contexto de base de datos para consultas.</param>
        /// <param name="logger">Servicio de logging para registrar eventos y errores.</param>
        public HomeController(Context context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Página principal del sistema después del inicio de sesión.
        /// </summary>
        /// <returns>Vista principal.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Muestra la vista de privacidad.
        /// </summary>
        /// <returns>Vista informativa de privacidad.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Página de error del sistema.
        /// Usada para mostrar excepciones y fallos inesperados.
        /// </summary>
        /// <returns>Vista de error.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// GET: Vista de login.
        /// Si el usuario ya está autenticado, se redirige a la página principal.
        /// </summary>
        /// <returns>Vista de login o redirección a Index.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true) // 👈 Corregido
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        /// <summary>
        /// POST: Procesa la autenticación del usuario.
        /// Valida credenciales, asigna claims y genera cookie de autenticación.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="password">Contraseña proporcionada.</param>
        /// <returns>Redirección al Home o mensaje de error.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsSoftDeleted);

                if (user != null && user.PasswordHash == password)
                {
                    var userRole = user.UserRoles.FirstOrDefault()?.Role;

                    if (userRole == null)
                    {
                        ViewBag.Error = "Usuario sin rol asignado";
                        return View();
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 👈 Corregido: user.Id
                        new Claim(ClaimTypes.Role, userRole.Name) // 👈 Corregido: userRole.Name
                    };

                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                    _logger.LogInformation($"Usuario {user.UserName} inició sesión");
                    return RedirectToAction("Index");
                }

                ViewBag.Error = "Email o contraseña incorrectos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                ViewBag.Error = "Error al iniciar sesión";
            }

            return View();
        }

        /// <summary>
        /// Cierra la sesión del usuario y elimina la cookie de autenticación.
        /// </summary>
        /// <returns>Redirección a la vista de Login.</returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Vista para registrar un nuevo usuario.
        /// (Funcionalidad futura del sistema.)
        /// </summary>
        /// <returns>Vista de registro.</returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Vista para recuperación de contraseña.
        /// (Funcionalidad futura del sistema.)
        /// </summary>
        /// <returns>Vista de recuperación de contraseña.</returns>
        public IActionResult PasswordRecovery()
        {
            return View();
        }
    }
}
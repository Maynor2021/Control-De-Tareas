using Control_De_Tareas.Data.Entitys;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Control_De_Tareas.Controllers
{
    public class AccountController : Controller
    {
        private readonly Context _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(Context context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Mostrar formulario de login
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Procesar login
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                // Buscar usuario por email
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsSoftDeleted);

                // Validar usuario y contraseña
                if (user != null && user.PasswordHash == password) // TODO: Usar hash en producción
                {
                    // Obtener el rol del usuario
                    var userRole = user.UserRoles.FirstOrDefault()?.Role;
                    
                    if (userRole == null)
                    {
                        ViewBag.Error = "Usuario sin rol asignado";
                        return View();
                    }

                    // Crear claims (información del usuario)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Role, userRole.RoleName)
                    };

                    // Crear identidad y principal
                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    // Crear sesión (Cookie)
                    await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
                    {
                        IsPersistent = true, // Recordar sesión
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                    _logger.LogInformation($"Usuario {user.UserName} inició sesión correctamente");

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Email o contraseña incorrectos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                ViewBag.Error = "Error al iniciar sesión. Intente nuevamente.";
            }

            return View();
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Página de acceso denegado
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

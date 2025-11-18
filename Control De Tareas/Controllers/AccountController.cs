using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Control_De_Tareas.Models;

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
        /// Mostrar formulario de registro
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            // Si ya está autenticado, redirigir al home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Procesar registro de nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(UserVm usersV)
        {
            var user = _context.Users.Where(u => u.Email == usersV.Email).ProjectToType<UserVm>().FirstOrDefault();
            try
            {
                // Validar que las contraseñas coincidan
                if (User ==null )
                {
                    ViewBag.Error = "Email debe ser insetado";
                    return View();
                }

                // Validar que el email no exista
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == email && !u.IsSoftDeleted);

                if (emailExists)
                {
                    ViewBag.Error = "El email ya está registrado";
                    return View();
                }

                // Validar que el username no exista
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.UserName == username && !u.IsSoftDeleted);

                if (usernameExists)
                {
                    ViewBag.Error = "El nombre de usuario ya está en uso";
                    return View();
                }

                // Crear nuevo usuario
                var newUser = new Users
                {
                    UserName = username,
                    FullName = fullName,
                    Email = email,
                    PasswordHash = password, // TODO: Usar hash en producción
                    CreatedAt = DateTime.Now,
                    IsEnabled = true,
                    IsSoftDeleted = false
                };

                // Guardar usuario en la base de datos
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Asignar rol por defecto (ejemplo: "Estudiante")
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Estudiante" && !r.IsSoftDeleted);

                if (defaultRole != null)
                {
                    var userRole = new UserRoles
                    {
                        UserId = newUser.Id,
                        RoleId = defaultRole.Id,
                        AssignedAt = DateTime.Now,
                        IsSoftDeleted = false
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Usuario {newUser.UserName} registrado correctamente");

                // Redirigir al login con mensaje de éxito
                TempData["SuccessMessage"] = "Registro exitoso. Por favor inicia sesión.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de registro");
                ViewBag.Error = "Error al registrar el usuario. Intente nuevamente.";
            }

            return View();
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
            // Redirigir a la vista de Home/Login
            return RedirectToAction("Login", "Home");
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
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsSoftDeleted);

                // Validar usuario y contraseña
                if (user != null && user.PasswordHash == password) // TODO: Usar hash en producción
                {
                    // Obtener el rol del usuario
<<<<<<< HEAD
                    var userRole = user.Rol?.RoleName;
                    
=======
                    var userRole = user.UserRoles.FirstOrDefault()?.Role;

>>>>>>> 33251b12da292b3cd7aa9f4be08621805c2e0e30
                    if (userRole == null)
                    {
                        TempData["Error"] = "Usuario sin rol asignado";
                        return RedirectToAction("Login", "Home");
                    }

                    // Crear claims (información del usuario)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
<<<<<<< HEAD
                        new Claim(ClaimTypes.Role, userRole)
=======
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, userRole.Name)
>>>>>>> 33251b12da292b3cd7aa9f4be08621805c2e0e30
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

                TempData["Error"] = "Email o contraseña incorrectos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                TempData["Error"] = "Error al iniciar sesión. Intente nuevamente.";
            }

            return RedirectToAction("Login", "Home");
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Login", "Home");
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
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Mapster;
using Control_De_Tareas.ViewsModels;

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
        public async Task<IActionResult> Register(string username, string fullName, string email, string password, string confirmPassword)
        {
            try
            {
                // Validar que las contraseñas coincidan
                if (password != confirmPassword)
                {
                    ViewBag.Error = "Las contraseñas no coinciden";
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
            return View();
        }

        /// <summary>
        /// Procesar login
        /// </summary>
        [HttpPost]
        public IActionResult Login(UserVm usersV)
        {
            var user = _context.Users.Where(u => u.Email == usersV.Email && u.IsSoftDeleted == false).ProjectToType<UserVm>().FirstOrDefault();

            // Validar que el usuario exista
            if (user == null)
            {
                ViewBag.Error = "usuario o contraseña incorrectos";
                return View(new UserVm());
            }

            usersV.Password = GetMD5(usersV.Password);

            if (user.Password != usersV.Password)
            {
                ViewBag.Error = "usuario o contraseña incorrectos";
                return View(new UserVm());
            }

            // Obtener los módulos del rol del usuario
            var modulosRoles = _context.RoleModules.Where(rm => rm.IsSoftDeleted == false && rm.RoleId == user.Rol.RoleId).ProjectToType<RolModuloVM>().ToList();
            var AgrupadosID = modulosRoles.Select(mr => mr.Modulo.ModuloAgrupadoId).Distinct().ToList();
            var agrupados = _context.ModuleGroup.Where(ma => ma.IsSoftDeleted == false && AgrupadosID.Contains(ma.GroupModuleId)).ProjectToType<ModuleGroupVm>().ToList();
            foreach (var Item in agrupados)
            {
                var modulosActuales = modulosRoles.Where(mr => mr.Modulo.ModuloAgrupadoId == Item.GroupModuleId).Select(s => s.Modulo.ModuleId).Distinct().ToList();
                Item.Modulos = Item.Modulos.Where(m => modulosActuales.Contains(m.ModuleId)).ToList();
            }

            user.menu = agrupados;
            user.Password = string.Empty;
            var sesionJson = JsonConvert.SerializeObject(user);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sesionJson);
            var sesionBase64 = System.Convert.ToBase64String(plainTextBytes);
            HttpContext.Session.SetString("UserSession", sesionBase64);

            return RedirectToAction("Index", "Home");
        }

        private string GetMD5(string str)
        {
            using (var md5 = MD5.Create())
            {
                var encoding = new ASCIIEncoding();
                byte[] stream = md5.ComputeHash(encoding.GetBytes(str));
                var sb = new StringBuilder();

                for (int i = 0; i < stream.Length; i++)
                    sb.AppendFormat("{0:x2}", stream[i]);

                return sb.ToString();
            }
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
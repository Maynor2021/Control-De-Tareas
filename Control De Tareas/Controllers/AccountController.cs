using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Control_De_Tareas.Controllers
{
    public class AccountController : Controller
    {
        private readonly ContextDB _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ContextDB context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            // 🔒 SOLO ADMINISTRADORES pueden acceder al registro
            if (!User.Identity?.IsAuthenticated == true || !User.IsInRole("Administrador"))
            {
                return RedirectToAction("Login");
            }

            // Cargar roles disponibles (excepto Administrador para seguridad)
            var roles = _context.Roles
                .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                .Select(r => new { r.RoleId, r.RoleName })
                .ToList();

            ViewBag.Roles = roles;

            return View();
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

        [HttpPost]
        public async Task<IActionResult> Login(UserVm usersV)
        {
            var user = _context.Users
                .Include(u => u.Rol)
                .Where(u => u.Email == usersV.Email && u.IsSoftDeleted == false)
                .FirstOrDefault();

            if (user == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View(new UserVm());
            }

            usersV.PasswordHash = GetMD5(usersV.PasswordHash);

            if (user.PasswordHash.ToUpper() != usersV.PasswordHash.ToUpper()) ///para no tener problemas aqui 
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View(new UserVm());
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Rol.RoleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            var modulosRoles = _context.RoleModules
                .Where(rm => rm.IsSoftDeleted == false && rm.RoleId == user.Rol.RoleId)
                .ProjectToType<RolModuloVM>()
                .ToList();

            var AgrupadosID = modulosRoles
                .Select(mr => mr.Modulo.ModuloAgrupadoId)
                .Distinct()
                .ToList();

            var agrupados = _context.ModuleGroup
                .Where(ma => ma.IsSoftDeleted == false && AgrupadosID.Contains(ma.GroupModuleId))
                .ProjectToType<ModuleGroupVm>()
                .ToList();

            foreach (var Item in agrupados)
            {
                var modulosActuales = modulosRoles
                    .Where(mr => mr.Modulo.ModuloAgrupadoId == Item.GroupModuleId)
                    .Select(s => s.Modulo.ModuleId)
                    .Distinct()
                    .ToList();

                Item.Modulos = Item.Modulos
                    .Where(m => modulosActuales.Contains(m.ModuleId))
                    .ToList();
            }

            var userVm = new UserVm
            {
                UserId = user.UserId,
                Nombre = user.UserName,
                Email = user.Email,
                Rol = new RolVm
                {
                    RoleId = user.Rol.RoleId,
                    Descripcion = user.Rol.RoleName,
                    Nombre = user.Rol.RoleName
                },
                menu = agrupados
            };

            var sesionJson = JsonConvert.SerializeObject(userVm);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sesionJson);
            var sesionBase64 = System.Convert.ToBase64String(plainTextBytes);

            HttpContext.Session.SetString("UserSession", sesionBase64);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserVm userVm, Guid roleId)
        {
            // 🔒 SOLO ADMINISTRADORES pueden registrar usuarios
            if (!User.Identity?.IsAuthenticated == true || !User.IsInRole("Administrador"))
            {
                ViewBag.Error = "No tiene permisos para registrar usuarios. Solo administradores pueden registrar nuevos usuarios.";
                return View(new UserVm());
            }

            // Validaciones básicas
            if (string.IsNullOrEmpty(userVm.Nombre))
            {
                ViewBag.Error = "El nombre es requerido";
                return View(userVm);
            }

            if (string.IsNullOrEmpty(userVm.PasswordHash) || userVm.PasswordHash.Length < 6)
            {
                ViewBag.Error = "La contraseña debe tener al menos 6 caracteres";
                return View(userVm);
            }

            // Verificar si el email ya existe
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userVm.Email && u.IsSoftDeleted == false);

            if (existingUser != null)
            {
                ViewBag.Error = "El email ya está registrado";
                return View(userVm);
            }

            // Validar que se seleccionó un rol válido
            var selectedRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId && r.IsSoftDeleted == false && r.RoleName != "Administrador");

            if (selectedRole == null)
            {
                ViewBag.Error = "Debe seleccionar un rol válido";
                return View(userVm);
            }

            try
            {
                // Obtener el ID del usuario administrador actual
                Guid creatorId = Guid.Empty; // Valor por defecto
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(currentUserId, out Guid parsedId))
                {
                    creatorId = parsedId;
                }
                else
                {
                    // Si no se puede obtener el ID del administrador actual, usar un valor por defecto
                    // O buscar el ID del primer administrador
                    var adminUser = await _context.Users
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.Rol.RoleName == "Administrador" && u.IsSoftDeleted == false);

                    if (adminUser != null)
                    {
                        creatorId = adminUser.UserId;
                    }
                }

                // Determinar si es instructor basado en el rol
                string instructorValue = selectedRole.RoleName == "Profesor" ? "Sí" : "No";

                // Crear nuevo usuario
                var newUser = new Users
                {
                    UserId = Guid.NewGuid(),
                    Email = userVm.Email,
                    UserName = userVm.Nombre,
                    PasswordHash = GetMD5(userVm.PasswordHash),
                    Rol = selectedRole,
                    RolId = selectedRole.RoleId,
                    CreateAt = DateTime.Now,
                    IsSoftDeleted = false,
                    Instructor = instructorValue,
                    CreatBy = creatorId,               // CORREGIDO: Si CreatBy es Guid, no Guid?
                    ModifieBy = creatorId              // CORREGIDO: Si ModifieBy es Guid, no Guid?
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                ViewBag.Success = $"Usuario '{userVm.Nombre}' registrado exitosamente como {selectedRole.RoleName}";

                // Recargar roles para la vista
                var roles = _context.Roles
                    .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                    .Select(r => new { r.RoleId, r.RoleName })
                    .ToList();

                ViewBag.Roles = roles;

                return View(new UserVm());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");

                // Recargar roles para la vista
                var roles = _context.Roles
                    .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                    .Select(r => new { r.RoleId, r.RoleName })
                    .ToList();

                ViewBag.Roles = roles;

                ViewBag.Error = "Error al registrar el usuario: " + ex.Message;
                return View(userVm);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
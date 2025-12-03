using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        #region Utilidades

        private string GetMD5(string str)
        {
            // Mantengo MD5 por compatibilidad con la base actual.
            // Recomiendo migrar a PasswordHasher<T> o PBKDF2/BCrypt/Argon2 cuanto antes.
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

        #endregion

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserVm usersV)
        {
            try
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

                if (user.PasswordHash.ToUpper() != usersV.PasswordHash.ToUpper())
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos";
                    return View(new UserVm());
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Role, user.Rol?.RoleName ?? "")
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
                        RoleId = user.Rol?.RoleId ?? Guid.Empty,
                        Descripcion = user.Rol?.RoleName ?? "",
                        Nombre = user.Rol?.RoleName ?? ""
                    },
                    menu = agrupados
                };

                var sesionJson = JsonConvert.SerializeObject(userVm);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sesionJson);
                var sesionBase64 = System.Convert.ToBase64String(plainTextBytes);

                HttpContext.Session.SetString("UserSession", sesionBase64);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Login");
                ViewBag.Error = "Error al iniciar sesión: " + ex.Message;
                return View(new UserVm());
            }
        }

        #endregion

        #region Register (GET + POST) - SOLO ADMIN

        // GET: carga roles para la vista (excepto Administrador)
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Register()
        {
            // 🔒 SOLO ADMINISTRADORES pueden acceder al registro (Authorize lo protege)
            // Carga roles disponibles (excepto Administrador)
            var roles = _context.Roles
                .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                .Select(r => new { r.RoleId, r.RoleName })
                .ToList();

            ViewBag.Roles = roles;

            return View();
        }

        // POST: registro de usuario por administrador
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserVm userVm, Guid roleId)
        {
            // 🔒 SOLO ADMINISTRADORES pueden registrar usuarios (Authorize lo protege)
            try
            {
                // Validaciones básicas
                if (string.IsNullOrEmpty(userVm.Nombre))
                {
                    ViewBag.Error = "El nombre es requerido";
                    // Recargar roles para la vista
                    var rolesRecarga = _context.Roles
                        .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                        .Select(r => new { r.RoleId, r.RoleName })
                        .ToList();
                    ViewBag.Roles = rolesRecarga;
                    return View(userVm);
                }

                if (string.IsNullOrEmpty(userVm.PasswordHash) || userVm.PasswordHash.Length < 6)
                {
                    ViewBag.Error = "La contraseña debe tener al menos 6 caracteres";
                    var rolesRecarga = _context.Roles
                        .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                        .Select(r => new { r.RoleId, r.RoleName })
                        .ToList();
                    ViewBag.Roles = rolesRecarga;
                    return View(userVm);
                }

                // Verificar si el email ya existe
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userVm.Email && u.IsSoftDeleted == false);

                if (existingUser != null)
                {
                    ViewBag.Error = "El email ya está registrado";
                    var rolesRecarga = _context.Roles
                        .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                        .Select(r => new { r.RoleId, r.RoleName })
                        .ToList();
                    ViewBag.Roles = rolesRecarga;
                    return View(userVm);
                }

                // Validar que se seleccionó un rol válido
                var selectedRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleId == roleId && r.IsSoftDeleted == false && r.RoleName != "Administrador");

                if (selectedRole == null)
                {
                    ViewBag.Error = "Debe seleccionar un rol válido";
                    var rolesRecarga = _context.Roles
                        .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                        .Select(r => new { r.RoleId, r.RoleName })
                        .ToList();
                    ViewBag.Roles = rolesRecarga;
                    return View(userVm);
                }

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
                    CreateAt = DateTime.UtcNow,
                    IsSoftDeleted = false,
                    Instructor = instructorValue,
                    CreatBy = creatorId,
                    ModifieBy = creatorId
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                ViewBag.Success = $"Usuario '{userVm.Nombre}' registrado exitosamente como {selectedRole.RoleName}";

                // Recargar roles para la vista
                var rolesFinal = _context.Roles
                    .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                    .Select(r => new { r.RoleId, r.RoleName })
                    .ToList();

                ViewBag.Roles = rolesFinal;

                return View(new UserVm());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");

                // Recargar roles para la vista
                var rolesRecarga = _context.Roles
                    .Where(r => r.IsSoftDeleted == false && r.RoleName != "Administrador")
                    .Select(r => new { r.RoleId, r.RoleName })
                    .ToList();

                ViewBag.Roles = rolesRecarga;

                ViewBag.Error = "Error al registrar el usuario: " + ex.Message;
                return View(userVm);
            }
        }

        #endregion

        #region Change Password

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Solo usuarios logueados pueden cambiar contraseña
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login");
            }

            // Obtener email del usuario actual
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            ViewBag.Email = userEmail;
            ViewBag.Msg = TempData["Msg"];
            ViewBag.Success = TempData["Success"];

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContraseña(string CurrentPassword, string Password, string ConfirmPassword)
        {
            // Solo usuarios logueados
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // 🔥 VALIDACIÓN EXTRA: Verificar que CurrentPassword no sea null
                if (string.IsNullOrEmpty(CurrentPassword))
                {
                    TempData["Msg"] = "Debes ingresar tu contraseña actual";
                    return RedirectToAction("ChangePassword");
                }

                // 1. Obtener usuario actual
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["Msg"] = "No se pudo identificar el usuario";
                    return RedirectToAction("ChangePassword");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userEmail && u.IsSoftDeleted == false);

                if (user == null)
                {
                    TempData["Msg"] = "Usuario no encontrado";
                    return RedirectToAction("ChangePassword");
                }

                // 2. Verificar contraseña actual
                string encryptedCurrentPassword = GetMD5(CurrentPassword);
                if (user.PasswordHash.ToUpper() != encryptedCurrentPassword.ToUpper())
                {
                    TempData["Msg"] = "La contraseña actual es incorrecta";
                    return RedirectToAction("ChangePassword");
                }

                // 3. Validar que las nuevas contraseñas coincidan
                if (Password != ConfirmPassword)
                {
                    TempData["Msg"] = "Las nuevas contraseñas no coinciden";
                    return RedirectToAction("ChangePassword");
                }

                // 4. Validar longitud mínima
                if (string.IsNullOrEmpty(Password) || Password.Length < 6)
                {
                    TempData["Msg"] = "La nueva contraseña debe tener al menos 6 caracteres";
                    return RedirectToAction("ChangePassword");
                }

                // 5. Encriptar la nueva contraseña con MD5
                // Usa el mismo algoritmo que en el login
                string encryptedPassword = GetMD5(Password);

                // 6. Actualizar la contraseña
                user.PasswordHash = encryptedPassword;
                user.ModifieBy = user.UserId; // El usuario se modifica a sí mismo

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // 7. Cerrar sesión para que ingrese con la nueva contraseña
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();

                // 8. Mostrar mensaje de éxito
                TempData["Success"] = "Contraseña cambiada exitosamente. Por favor, inicie sesión con su nueva contraseña.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                TempData["Msg"] = "Error al cambiar la contraseña: " + ex.Message;
                return RedirectToAction("ChangePassword");
            }
        }

        #endregion

        #region Logout

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        #endregion
    }
}

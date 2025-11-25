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
                    sb.AppendFormat("{0:X2}", stream[i]);

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

            if (user.PasswordHash != usersV.PasswordHash)
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
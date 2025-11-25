using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Mapster;
using Mapster.EFCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

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
        public IActionResult Login(UserVm usersV)
        {
           
            var user = _context.Users
                .Where(u => u.Email == usersV.Email && u.IsSoftDeleted == false)
                .ProjectToType<UserVm>()
                .FirstOrDefault();

          
            if (user == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View(new UserVm());
            }

            // Hashear la contraseña ingresada
            usersV.PasswordHash = GetMD5(usersV.PasswordHash);

            // Validar que las contraseñas coincidan
            if (user.PasswordHash != usersV.PasswordHash)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View(new UserVm());
            }

            // Obtener los módulos del rol del usuario
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


            user.menu = agrupados;
            user.PasswordHash = string.Empty;

            var sesionJson = JsonConvert.SerializeObject(user);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sesionJson);
            var sesionBase64 = System.Convert.ToBase64String(plainTextBytes);

            HttpContext.Session.SetString("UserSession", sesionBase64);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("login", "Home");

        }


    }
}
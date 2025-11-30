using Microsoft.AspNetCore.Mvc;
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Control_De_Tareas.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly ContextDB _context;

        public SubmissionsController(ContextDB context)
        {
            _context = context;
        }

        // GET: Submissions (Mis Entregas)
        public async Task<IActionResult> Index()
        {
            // Obtener información del usuario desde la sesión
            var userInfo = GetCurrentUser();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Determinar qué entregas mostrar según el rol
            IQueryable<Submissions> query = _context.Submissions
                .Include(s => s.Task)
                .Include(s => s.Student)
                .Include(s => s.SubmissionFiles)
                .Include(s => s.Grades)
                .Where(s => !s.IsSoftDeleted);

            // Si es estudiante, solo mostrar sus propias entregas
            if (userInfo.Rol?.Nombre == "Estudiante")
            {
                query = query.Where(s => s.StudentId == userInfo.UserId);
            }

            var submissions = await query
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            ViewBag.UserRole = userInfo.Rol?.Nombre;
            ViewBag.UserId = userInfo.UserId;
            ViewBag.UserName = userInfo.Nombre;

            return View(submissions);
        }

        // Método auxiliar para obtener usuario actual
        private UserVm GetCurrentUser()
        {
            var sesionBase64 = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(sesionBase64))
                return null;

            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(sesionBase64);
                var sesion = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                return JsonConvert.DeserializeObject<UserVm>(sesion);
            }
            catch
            {
                return null;
            }
        }
    }
}
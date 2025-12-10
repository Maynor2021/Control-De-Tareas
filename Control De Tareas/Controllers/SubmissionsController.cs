using Microsoft.AspNetCore.Mvc;
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Control_De_Tareas.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Control_De_Tareas.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly ContextDB _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly AuditService _auditService;

        public SubmissionsController(ContextDB context, IFileStorageService fileStorageService, AuditService auditService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _auditService = auditService;
        }

        // ============================
        // ✅ INDEX
        // ============================
        public async Task<IActionResult> Index()
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null)
                return RedirectToAction("Login", "Account");

            IQueryable<Submissions> query = _context.Submissions
                .Include(s => s.Task)
                .Include(s => s.Student)
                .Include(s => s.SubmissionFiles)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Grader) // ← AÑADIDO: Incluir quien calificó
                .Where(s => !s.IsSoftDeleted);

            if (userInfo.Rol?.Nombre == "Estudiante")
                query = query.Where(s => s.StudentId == userInfo.UserId);

            var submissions = await query
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            ViewBag.UserRole = userInfo.Rol?.Nombre;
            ViewBag.UserId = userInfo.UserId;
            ViewBag.UserName = userInfo.Nombre;

            return View(submissions);
        }

        // ============================
        // ✅ DETAILS (OJITO AZUL)
        // ============================
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null)
                return RedirectToAction("Login", "Account");

            var submission = await _context.Submissions
                .Include(s => s.Task)
                    .ThenInclude(t => t.CourseOffering)
                        .ThenInclude(co => co.Course)
                .Include(s => s.Student)
                .Include(s => s.SubmissionFiles)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Grader) // ← AÑADIDO: Incluir quien calificó
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

            if (submission == null)
                return NotFound();

            // 🔐 Seguridad: estudiante solo ve su entrega
            if (userInfo.Rol?.Nombre == "Estudiante" && submission.StudentId != userInfo.UserId)
                return Forbid();

            return View(submission);
        }

        // ============================
        // ✅ CREATE GET
        // ============================
        [HttpGet]
        public async Task<IActionResult> Create(Guid? taskId)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null)
                return RedirectToAction("Login", "Account");

            var model = new SubmissionCreateVm();

            if (taskId.HasValue)
            {
                var task = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == taskId.Value && !t.IsSoftDeleted);
                if (task != null)
                {
                    model.TaskId = task.Id;
                    model.TaskTitle = task.Title;
                    model.TaskDueDate = task.DueDate;
                }
            }

            ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
            return View(model);
        }

        // ============================
        // ✅ CREATE POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubmissionCreateVm model)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null)
                return RedirectToAction("Login", "Account");

            ModelState.Remove("TaskTitle");
            ModelState.Remove("TaskDueDate");

            if (!ModelState.IsValid)
            {
                ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                return View(model);
            }

            try
            {
                var task = await _context.Tareas
                    .Include(t => t.CourseOffering)
                    .FirstOrDefaultAsync(t => t.Id == model.TaskId && !t.IsSoftDeleted);

                if (task == null)
                {
                    ModelState.AddModelError("", "La tarea seleccionada no existe.");
                    ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                    return View(model);
                }

                var isEnrolled = await _context.Enrollments.AnyAsync(e =>
                    e.StudentId == userInfo.UserId &&
                    e.CourseOfferingId == task.CourseOfferingId &&
                    !e.IsSoftDeleted &&
                    e.Status == "Active");

                if (!isEnrolled)
                {
                    ModelState.AddModelError("", "No estás inscrito en este curso.");
                    ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                    return View(model);
                }

                if (DateTime.Now > task.DueDate)
                {
                    ModelState.AddModelError("", "La fecha límite ya venció.");
                    ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                    return View(model);
                }

                var submission = new Submissions
                {
                    Id = Guid.NewGuid(),
                    StudentId = userInfo.UserId,
                    TaskId = model.TaskId,
                    SubmittedAt = DateTime.Now,
                    Comments = model.Comments,
                    Status = "Submitted",
                    CurrentGrade = 0,
                    IsSoftDeleted = false
                };

                _context.Submissions.Add(submission);

                int filesCount = 0;

                if (model.Files != null && model.Files.Any())
                {
                    filesCount = model.Files.Count;

                    foreach (var file in model.Files)
                    {
                        if (!_fileStorageService.ValidateFile(file, out string errorMessage))
                        {
                            ModelState.AddModelError("Files", errorMessage);
                            ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                            return View(model);
                        }

                        string extension = Path.GetExtension(file.FileName);
                        string uniqueFileName = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}{extension}";

                        int courseOfferingIdInt = Math.Abs(task.CourseOffering.GetHashCode());
                        int taskIdInt = Math.Abs(task.Id.GetHashCode());

                        string filePath = await _fileStorageService.SaveFileAsync(
                            file, courseOfferingIdInt, taskIdInt, uniqueFileName);

                        var submissionFile = new SubmissionFiles
                        {
                            Id = Guid.NewGuid(),
                            SubmissionId = submission.Id,
                            FileName = file.FileName,
                            FilePath = filePath,
                            UploadedAt = DateTime.Now,
                            IsSoftDeleted = false
                        };

                        _context.SubmissionFiles.Add(submissionFile);
                    }
                }

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("SUBMISSION_CREATE", "Entrega", submission.Id,
                    $"Entrega creada con {filesCount} archivos");

                TempData["Success"] = "✅ Entrega creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _auditService.LogAsync("SUBMISSION_ERROR", "Entrega", null, ex.Message);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================
        // ✅ REVIEW POR TAREA (S3-14)
        // ============================
        [HttpGet]
        public async Task<IActionResult> Review(Guid taskId)
        {
            var userInfo = GetCurrentUser();

            if (userInfo == null ||
                (userInfo.Rol?.Nombre != "Profesor" && userInfo.Rol?.Nombre != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            var task = await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsSoftDeleted);

            if (task == null)
                return NotFound();

            var submissions = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.SubmissionFiles)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Grader) // ← AÑADIDO: Incluir quien calificó
                .Where(s => s.TaskId == taskId && !s.IsSoftDeleted)
                .OrderBy(s => s.Student.UserName)
                .ToListAsync();

            ViewBag.TaskTitle = task.Title;
            ViewBag.MaxScore = task.MaxScore;

            return View(submissions);
        }

        // ============================
        // ✅ CALIFICAR ENTREGA
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(Guid id, decimal grade, string feedback)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null ||
                (userInfo.Rol?.Nombre != "Profesor" && userInfo.Rol?.Nombre != "Administrador"))
                return RedirectToAction("Login", "Account");

            if (grade < 0 || grade > 100)
            {
                TempData["Error"] = "La nota debe estar entre 0 y 100.";
                return RedirectToAction(nameof(Index));
            }

            var submission = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Task)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

            if (submission == null)
                return NotFound();

            submission.CurrentGrade = grade;
            submission.Status = "Calificada";

            var gradeEntity = new Grades
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                GraderId = userInfo.UserId,
                Score = grade,
                Feedback = feedback,
                GradedAt = DateTime.Now,
                IsSoftDeleted = false
            };

            _context.Grades.Add(gradeEntity);

            await _auditService.LogAsync("SUBMISSION_GRADE", "Entrega", submission.Id,
                $"Entrega calificada con {grade}");

            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Entrega calificada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // ✅ HELPERS
        // ============================
        private UserVm GetCurrentUser()
        {
            var sesionBase64 = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(sesionBase64)) return null;

            var base64EncodedBytes = Convert.FromBase64String(sesionBase64);
            var sesion = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            return JsonConvert.DeserializeObject<UserVm>(sesion);
        }

        private async Task<List<TaskSelectVm>> GetAvailableTasksForStudent(Guid studentId)
        {
            return await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Enrollments)
                .Where(t => !t.IsSoftDeleted &&
                            t.CourseOffering.Enrollments
                                .Any(e => e.StudentId == studentId && !e.IsSoftDeleted))
                .OrderByDescending(t => t.DueDate)
                .Select(t => new TaskSelectVm
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    CourseName = t.CourseOffering.Course.Title
                }).ToListAsync();
        }
    }

    public class TaskSelectVm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public string CourseName { get; set; }
    }
}
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

        // GET: Submissions (Mis Entregas)
        public async Task<IActionResult> Index()
        {
            var userInfo = GetCurrentUser();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Submissions> query = _context.Submissions
                .Include(s => s.Task)
                .Include(s => s.Student)
                .Include(s => s.SubmissionFiles)
                .Include(s => s.Grades)
                .Where(s => !s.IsSoftDeleted);

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

        // GET: Submissions/Create
        [HttpGet]
        public async Task<IActionResult> Create(Guid? taskId)
        {
            var userInfo = GetCurrentUser();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new SubmissionCreateVm();

            // Si se pasa un TaskId, cargar info de la tarea
            if (taskId.HasValue)
            {
                var task = await _context.Tareas
                    .FirstOrDefaultAsync(t => t.Id == taskId.Value && !t.IsSoftDeleted);

                if (task != null)
                {
                    model.TaskId = task.Id;
                    model.TaskTitle = task.Title;
                    model.TaskDueDate = task.DueDate;
                }
            }

            // Cargar lista de tareas disponibles
            ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);

            return View(model);
        }

        // POST: Submissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubmissionCreateVm model)
        {
            var userInfo = GetCurrentUser();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // REMOVER VALIDACIONES DE CAMPOS AUXILIARES
            ModelState.Remove("TaskTitle");
            ModelState.Remove("TaskDueDate");

            if (!ModelState.IsValid)
            {
                ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                return View(model);
            }

            try
            {
                // 1. Validar que la tarea existe
                var task = await _context.Tareas
                    .Include(t => t.CourseOffering)
                    .FirstOrDefaultAsync(t => t.Id == model.TaskId && !t.IsSoftDeleted);

                if (task == null)
                {
                    ModelState.AddModelError("", "La tarea seleccionada no existe.");
                    ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                    return View(model);
                }

                // TM-157: Validar que el estudiante esté inscrito en el curso
                var isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.StudentId == userInfo.UserId
                                   && e.CourseOfferingId == task.CourseOfferingId
                                   && !e.IsSoftDeleted
                                   && e.Status == "Active");

                if (!isEnrolled)
                {
                    ModelState.AddModelError("", "No estás inscrito en el curso de esta tarea. No puedes realizar la entrega.");
                    ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                    return View(model);
                }

                // TM-158: Validar fecha límite
                if (DateTime.Now > task.DueDate)
                {
                    var horasRetraso = (DateTime.Now - task.DueDate).TotalHours;

                    ModelState.AddModelError("",
                        $"La fecha límite de esta tarea ya venció el {task.DueDate:dd/MM/yyyy HH:mm}. " +
                        $"Han transcurrido {Math.Round(horasRetraso, 1)} horas desde el vencimiento. " +
                        $"No se pueden realizar entregas tardías.");

                    ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                    return View(model);
                }

                // Advertencia si está cerca del vencimiento (últimas 24 horas)
                var horasRestantes = (task.DueDate - DateTime.Now).TotalHours;
                if (horasRestantes <= 24 && horasRestantes > 0)
                {
                    TempData["Warning"] = $"⚠️ Atención: Esta tarea vence en {Math.Round(horasRestantes, 1)} horas.";
                }

                // 2. Crear el registro de Submission
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

                // 3. Guardar archivos
                int filesCount = 0;
                if (model.Files != null && model.Files.Any())
                {
                    filesCount = model.Files.Count;
                    foreach (var file in model.Files)
                    {
                        // Validar archivo
                        if (!_fileStorageService.ValidateFile(file, out string errorMessage))
                        {
                            ModelState.AddModelError("Files", $"Archivo '{file.FileName}': {errorMessage}");
                            ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                            return View(model);
                        }

                        // Generar nombre único
                        string extension = Path.GetExtension(file.FileName);
                        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        string uniqueFileName = $"{timestamp}_{Guid.NewGuid()}{extension}";

                        // Guardar físicamente usando la estructura existente
                        int courseOfferingIdInt = Math.Abs(task.CourseOffering.GetHashCode());
                        int taskIdInt = Math.Abs(task.Id.GetHashCode());

                        string filePath = await _fileStorageService.SaveFileAsync(
                            file,
                            courseOfferingIdInt,
                            taskIdInt,
                            uniqueFileName
                        );

                        // Crear registro en SubmissionFiles
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

                // 4. Guardar todo en la base de datos
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Entrega creada exitosamente
                await _auditService.LogAsync("SUBMISSION_CREATE", "Entrega", submission.Id,
                    $"Estudiante '{userInfo.Nombre}' entregó tarea '{task.Title}' con {filesCount} archivo(s)");

                TempData["Success"] = $"✅ Entrega realizada exitosamente. Se subieron {filesCount} archivo(s).";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //  AUDITORÍA: Error al crear entrega
                await _auditService.LogAsync("SUBMISSION_CREATE_ERROR", "Entrega", null,
                    $"Error al crear entrega para tarea {model.TaskId}: {ex.Message}");

                ModelState.AddModelError("", $"Error al procesar la entrega: {ex.Message}");
                ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                return View(model);
            }
        }

        // POST: Submissions/Delete (eliminación de entrega)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Task)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

                if (submission == null)
                {
                    return NotFound();
                }

                // Verificar permisos: solo el estudiante dueño puede eliminar su entrega
                if (submission.StudentId != userInfo.UserId && userInfo.Rol?.Nombre != "Administrador")
                {
                    TempData["Error"] = "No tienes permiso para eliminar esta entrega.";
                    return RedirectToAction(nameof(Index));
                }

                //  AUDITORÍA: Antes de eliminar (guardar info)
                var submissionInfo = new
                {
                    submission.Task?.Title,
                    submission.StudentId,
                    submission.SubmittedAt
                };

                // Soft delete de la entrega
                submission.IsSoftDeleted = true;
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Entrega eliminada
                await _auditService.LogDeleteAsync("Entrega", submission.Id,
                    $"Entrega de tarea '{submission.Task?.Title}' eliminada");

                TempData["Success"] = "Entrega eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //  AUDITORÍA: Error al eliminar entrega
                await _auditService.LogAsync("SUBMISSION_DELETE_ERROR", "Entrega", id,
                    $"Error al eliminar entrega: {ex.Message}");

                TempData["Error"] = $"Error al eliminar la entrega: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Submissions/Grade (para profesores/administradores)
        [HttpGet]
        public async Task<IActionResult> Grade(Guid id)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null || (userInfo.Rol?.Nombre != "Profesor" && userInfo.Rol?.Nombre != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            var submission = await _context.Submissions
                .Include(s => s.Task)
                .Include(s => s.Student)
                .Include(s => s.SubmissionFiles)
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        // POST: Submissions/Grade (calificar entrega)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(Guid id, decimal grade, string feedback)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null || (userInfo.Rol?.Nombre != "Profesor" && userInfo.Rol?.Nombre != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Task)
                    .Include(s => s.Student)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

                if (submission == null)
                {
                    return NotFound();
                }

                // Validar que la calificación esté dentro del rango permitido
                var task = submission.Task;
                if (grade < 0 || grade > (task?.MaxScore ?? 100))
                {
                    ModelState.AddModelError("", $"La calificación debe estar entre 0 y {task?.MaxScore ?? 100}");
                    return View("Grade", submission);
                }

                // Guardar calificación anterior para auditoría
                var oldGrade = submission.CurrentGrade;

                // Actualizar calificación
                submission.CurrentGrade = grade;
                submission.Status = "Graded";

                // Crear registro de Grade
                var gradeRecord = new Grades
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    Score = grade,
                    Feedback = feedback,
                    GradedAt = DateTime.Now,
                    GraderId = userInfo.UserId,
                    IsSoftDeleted = false
                };

                _context.Grades.Add(gradeRecord);
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Entrega calificada
                await _auditService.LogAsync("SUBMISSION_GRADE", "Calificación", gradeRecord.Id,
                    $"Profesor '{userInfo.Nombre}' calificó entrega de '{submission.Student?.UserName}' " +
                    $"con {grade}/{task?.MaxScore}. Feedback: {feedback}");

                TempData["Success"] = $"Calificación registrada exitosamente: {grade}/{task?.MaxScore}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //  AUDITORÍA: Error al calificar
                await _auditService.LogAsync("SUBMISSION_GRADE_ERROR", "Calificación", id,
                    $"Error al calificar entrega: {ex.Message}");

                TempData["Error"] = $"Error al registrar la calificación: {ex.Message}";
                return RedirectToAction("Grade", new { id });
            }
        }

        // POST: Submissions/Reopen (reabrir entrega para nueva entrega)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(Guid id)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null || (userInfo.Rol?.Nombre != "Profesor" && userInfo.Rol?.Nombre != "Administrador"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Task)
                    .Include(s => s.Student)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

                if (submission == null)
                {
                    return NotFound();
                }

                // Reabrir entrega
                submission.Status = "Submitted";
                submission.CurrentGrade = null;

                // AUDITORÍA: Entrega reabierta
                await _auditService.LogAsync("SUBMISSION_REOPEN", "Entrega", submission.Id,
                    $"Profesor '{userInfo.Nombre}' reabrió entrega de '{submission.Student?.UserName}' " +
                    $"para tarea '{submission.Task?.Title}'");

                await _context.SaveChangesAsync();

                TempData["Success"] = "Entrega reabierta. El estudiante puede realizar una nueva entrega.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // AUDITORÍA: Error al reabrir
                await _auditService.LogAsync("SUBMISSION_REOPEN_ERROR", "Entrega", id,
                    $"Error al reabrir entrega: {ex.Message}");

                TempData["Error"] = $"Error al reabrir la entrega: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

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

        private async Task<List<TaskSelectVm>> GetAvailableTasksForStudent(Guid studentId)
        {
            var tasks = await _context.Tareas
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(t => t.CourseOffering)
                    .ThenInclude(co => co.Enrollments)
                .Where(t => !t.IsSoftDeleted &&
                            t.CourseOffering.Enrollments.Any(e => e.StudentId == studentId && !e.IsSoftDeleted))
                .OrderByDescending(t => t.DueDate)
                .Select(t => new TaskSelectVm
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    CourseName = t.CourseOffering.Course.Title ?? "Sin curso"
                })
                .ToListAsync();

            return tasks;
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
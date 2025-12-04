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

        public SubmissionsController(ContextDB context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
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
                if (model.Files != null && model.Files.Any())
                {
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
                        // Convertir Guid a int para compatibilidad con FileStorageService
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

                TempData["Success"] = $"✅ Entrega realizada exitosamente. Se subieron {model.Files?.Count ?? 0} archivo(s).";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al procesar la entrega: {ex.Message}");
                ViewBag.Tasks = await GetAvailableTasksForStudent(userInfo.UserId);
                return View(model);
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
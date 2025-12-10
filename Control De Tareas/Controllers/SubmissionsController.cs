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
using Microsoft.Extensions.Logging;

namespace Control_De_Tareas.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly ContextDB _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly AuditService _auditService;
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(ContextDB context, IFileStorageService fileStorageService,
            AuditService auditService, ILogger<SubmissionsController> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _auditService = auditService;
            _logger = logger;
        }

        // ============================
        // INDEX
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
                    .ThenInclude(g => g.Grader)
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
        // DETAILS (OJITO AZUL)
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
                    .ThenInclude(g => g.Grader)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

            if (submission == null)
                return NotFound();

            // 🔐 Seguridad: estudiante solo ve su entrega
            if (userInfo.Rol?.Nombre == "Estudiante" && submission.StudentId != userInfo.UserId)
            {
                TempData["Error"] = "No tienes permisos para ver esta entrega.";
                return RedirectToAction("Index");
            }

            return View(submission);
        }

        // ============================
        // CREATE GET
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
        // CREATE POST
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
        // REVIEW POR TAREA (S3-14)
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
                    .ThenInclude(g => g.Grader)
                .Where(s => s.TaskId == taskId && !s.IsSoftDeleted)
                .OrderBy(s => s.Student.UserName)
                .ToListAsync();

            ViewBag.TaskTitle = task.Title;
            ViewBag.MaxScore = task.MaxScore;

            return View(submissions);
        }

        // ============================
        // CALIFICAR ENTREGA
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
        // DESCARGAR ARCHIVO DE ENTREGA
        // ============================
        [HttpGet]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO DESCARGA DE ARCHIVO ===");
                _logger.LogInformation("FileId: {FileId}", fileId);

                var userInfo = GetCurrentUser();
                if (userInfo == null)
                {
                    _logger.LogWarning("Usuario no autenticado");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("Usuario: {Nombre} ({Rol})", userInfo.Nombre, userInfo.Rol?.Nombre);
                _logger.LogInformation("UserId: {UserId}", userInfo.UserId);

                // Obtener el archivo de entrega con información relacionada
                var submissionFile = await _context.SubmissionFiles
                    .Include(f => f.Submission)
                        .ThenInclude(s => s.Student)
                    .Include(f => f.Submission)
                        .ThenInclude(s => s.Task)
                            .ThenInclude(t => t.CourseOffering)
                    .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsSoftDeleted);

                if (submissionFile == null)
                {
                    _logger.LogError("Archivo de entrega no encontrado en la base de datos");
                    return NotFound("El archivo no existe o ha sido eliminado.");
                }

                _logger.LogInformation("Archivo encontrado: {FileName}", submissionFile.FileName);
                _logger.LogInformation("StudentId: {StudentId}", submissionFile.Submission?.StudentId);
                _logger.LogInformation("TaskId: {TaskId}", submissionFile.Submission?.TaskId);

                // 🔐 Verificar permisos de seguridad
                var userRole = userInfo.Rol?.Nombre;
                var userId = userInfo.UserId;

                // Estudiante: solo puede descargar sus propios archivos
                if (userRole == "Estudiante")
                {
                    if (submissionFile.Submission?.StudentId != userId)
                    {
                        _logger.LogWarning("Estudiante intentando descargar archivo ajeno");
                        TempData["Error"] = "No tienes permisos para descargar este archivo.";
                        return RedirectToAction("Index");
                    }
                    _logger.LogInformation("Estudiante autorizado - es su propia entrega");
                }

                // Profesor: puede descargar archivos de sus cursos
                if (userRole == "Profesor")
                {
                    var task = submissionFile.Submission?.Task;
                    if (task == null || task.CourseOffering == null)
                    {
                        _logger.LogError("No se pudo obtener información del curso");
                        TempData["Error"] = "Error al verificar permisos.";
                        return RedirectToAction("Index");
                    }

                    _logger.LogInformation("Profesor del curso: {ProfessorId}", task.CourseOffering.ProfessorId);
                    _logger.LogInformation("Profesor actual: {UserId}", userId);

                    if (task.CourseOffering.ProfessorId != userId)
                    {
                        _logger.LogWarning("Profesor no es del curso. Acceso denegado.");
                        TempData["Error"] = "Solo el profesor de este curso puede descargar las entregas.";
                        return RedirectToAction("Index");
                    }

                    _logger.LogInformation("Profesor autorizado para descargar");
                }

                // Administrador: siempre tiene acceso
                if (userRole == "Administrador")
                {
                    _logger.LogInformation("Administrador autorizado");
                }

                if (userRole != "Administrador" && userRole != "Profesor" && userRole != "Estudiante")
                {
                    _logger.LogWarning("Rol desconocido: {UserRole}", userRole);
                    TempData["Error"] = "No tienes permisos para descargar este archivo.";
                    return RedirectToAction("Index");
                }

                // Verificar si el archivo existe físicamente
                var filePath = submissionFile.FilePath;
                _logger.LogInformation("FilePath original: {FilePath}", filePath);

                // Si FilePath es relativo, convertirlo a absoluto
                if (!Path.IsPathRooted(filePath))
                {
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    // Remover ~/ o / del inicio si existe
                    if (filePath.StartsWith("~/") || filePath.StartsWith("/"))
                    {
                        filePath = filePath.TrimStart('~', '/');
                    }

                    filePath = Path.Combine(webRootPath, filePath);
                }

                _logger.LogInformation("FilePath absoluto: {FilePath}", filePath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("ARCHIVO NO ENCONTRADO EN EL SERVIDOR: {FilePath}", filePath);

                    // Verificar si la carpeta existe
                    var directory = Path.GetDirectoryName(filePath);
                    _logger.LogInformation("Directorio: {Directory}", directory);

                    if (directory != null && Directory.Exists(directory))
                    {
                        _logger.LogInformation("El directorio existe pero el archivo no");
                        var filesInDirectory = Directory.GetFiles(directory);
                        _logger.LogInformation("Archivos en el directorio: {Count}", filesInDirectory.Length);
                        foreach (var file in filesInDirectory)
                        {
                            _logger.LogInformation(" - {File}", Path.GetFileName(file));
                        }
                    }
                    else
                    {
                        _logger.LogError("El directorio no existe: {Directory}", directory);
                    }

                    return NotFound($"El archivo '{submissionFile.FileName}' no existe en el servidor. Ruta: {filePath}");
                }

                // Obtener información del archivo
                var fileInfo = new FileInfo(filePath);
                _logger.LogInformation("Archivo encontrado. Tamaño: {Size} bytes", fileInfo.Length);
                _logger.LogInformation("Última modificación: {LastWriteTime}", fileInfo.LastWriteTime);

                // Leer el archivo
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(submissionFile.FileName);

                _logger.LogInformation("Content-Type: {ContentType}", contentType);
                _logger.LogInformation("Tamaño leído: {Bytes} bytes", fileBytes.Length);

                // Registrar auditoría
                await _auditService.LogAsync("SUBMISSION_FILE_DOWNLOAD", "ArchivoEntrega", submissionFile.Id,
                    $"Archivo descargado: {submissionFile.FileName} por {userInfo.Nombre} ({userRole})");

                _logger.LogInformation("=== DESCARGA EXITOSA ===");

                // Forzar descarga en lugar de vista previa
                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = submissionFile.FileName,
                    Inline = false  // Esto fuerza la descarga en lugar de abrir en el navegador
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR CRÍTICO descargando archivo {FileId}", fileId);

                // Registrar error en auditoría
                await _auditService.LogAsync("SUBMISSION_FILE_DOWNLOAD_ERROR", "ArchivoEntrega", fileId,
                    $"Error al descargar archivo: {ex.Message}");

                TempData["Error"] = $"Error al descargar el archivo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // ============================
        // REABRIR ENTREGA
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(Guid id)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null ||
                (userInfo.Rol?.Nombre != "Profesor" && userInfo.Rol?.Nombre != "Administrador"))
                return RedirectToAction("Login", "Account");

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsSoftDeleted);

            if (submission == null)
                return NotFound();

            submission.Status = "Submitted";
            submission.CurrentGrade = null;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("SUBMISSION_REOPEN", "Entrega", submission.Id,
                "Entrega reabierta para recalificación");

            TempData["Success"] = "✅ Entrega reabierta correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // HELPERS
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

        // ============================
        // OBTENER CONTENT TYPE
        // ============================
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            var mimeTypes = new Dictionary<string, string>
    {
        {".pdf", "application/pdf"},
        {".doc", "application/msword"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".txt", "text/plain"},
        {".rtf", "application/rtf"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".png", "image/png"},
        {".gif", "image/gif"},
        {".bmp", "image/bmp"},
        {".zip", "application/zip"},
        {".rar", "application/x-rar-compressed"},
        {".7z", "application/x-7z-compressed"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".csv", "text/csv"},
        {".html", "text/html"},
        {".htm", "text/html"},
        {".xml", "application/xml"},
        {".json", "application/json"},
        {".mp4", "video/mp4"},
        {".mp3", "audio/mpeg"},
        {".wav", "audio/wav"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"}
    };

            if (mimeTypes.ContainsKey(extension))
            {
                return mimeTypes[extension];
            }

            // Si no se encuentra, usar genérico
            _logger.LogWarning("Extensión no reconocida: {Extension}, usando application/octet-stream", extension);
            return "application/octet-stream";
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
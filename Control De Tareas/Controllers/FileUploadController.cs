using Microsoft.AspNetCore.Mvc;
using Control_De_Tareas.Models;
using Control_De_Tareas.Services;
using System;
using System.Threading.Tasks;

namespace Control_De_Tareas.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly IFileStorageService _fileStorageService;

        public FileUploadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        // GET: FileUpload/Upload
        [HttpGet]
        public IActionResult Upload()
        {
            return View(new FileUploadVm());
        }

        // POST: FileUpload/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FileUploadVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validar el archivo
                if (!_fileStorageService.ValidateFile(model.File, out string errorMessage))
                {
                    ModelState.AddModelError("File", errorMessage);
                    return View(model);
                }

                // Generar nombre único
                string extension = System.IO.Path.GetExtension(model.File.FileName);
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string uniqueFileName = $"{timestamp}_{Guid.NewGuid()}{extension}";

                // Guardar el archivo físicamente
                string filePath = await _fileStorageService.SaveFileAsync(
                    model.File,
                    model.CourseOfferingId,
                    model.TaskId,
                    uniqueFileName
                );

                TempData["Success"] = $"Archivo '{model.File.FileName}' subido exitosamente en: {filePath}";
                return RedirectToAction(nameof(Upload));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }
    }
}
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Control_De_Tareas.Services; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    public class CursosController : Controller
    {
        private ContextDB _context;
        private ILogger<CursosController> _logger;
        private readonly AuditService _auditService; 

        public CursosController(ContextDB context, ILogger<CursosController> logger, AuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService; 
        }

        public async Task<IActionResult> Index()
        {
            var cursos = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Professor)
                .Include(co => co.Enrollments)
                .Where(co => !co.IsSoftDeleted)
                .Select(co => new CursoDto
                {
                    Id = co.Id,
                    Codigo = co.Course.Code,
                    Nombre = co.Course.Title,
                    Descripcion = co.Course.Description ?? "",
                    Estado = co.IsActive ? "Activo" : "Inactivo",
                    InstructorNombre = co.Professor.UserName,
                    CantidadEstudiantes = co.Enrollments.Count(e => !e.IsSoftDeleted)
                })
                .ToListAsync();

            var viewModel = new CursosVm
            {
                Cursos = cursos
            };

            return View(viewModel);
        }

        // ========== SOLO ADMIN PUEDE CREAR CURSOS ==========
        [Authorize(Roles = "Administrador")]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear(string Codigo, string Nombre, string Descripcion)
        {
            if (string.IsNullOrEmpty(Codigo) || string.IsNullOrEmpty(Nombre))
            {
                TempData["Error"] = "Código y Nombre son requeridos";
                return View();
            }

            try
            {
                // Verificar si ya existe un curso con el mismo código
                var cursoExistente = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Code == Codigo && !c.IsSoftDeleted);

                if (cursoExistente != null)
                {
                    TempData["Error"] = $"Ya existe un curso con el código: {Codigo}";
                    return View();
                }

                var curso = new Courses
                {
                    Id = Guid.NewGuid(),
                    Code = Codigo,
                    Title = Nombre,
                    Description = Descripcion,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    IsSoftDeleted = false
                };

                _context.Courses.Add(curso);
                await _context.SaveChangesAsync();

                // AUDITORÍA: Curso creado
                await _auditService.LogCreateAsync("Curso", curso.Id, $"{Codigo} - {Nombre}");

                TempData["Success"] = "Curso creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear curso {Codigo}", Codigo);

                // AUDITORÍA: Error al crear curso
                await _auditService.LogAsync("CURSO_CREATE_ERROR", "Curso", null,
                    $"Error al crear curso {Codigo} - {Nombre}: {ex.Message}");

                TempData["Error"] = $"Error al crear el curso: {ex.Message}";
                return View();
            }
        }

        // ========== EDITAR CURSO ==========
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(Guid id)
        {
            var curso = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsSoftDeleted);

            if (curso == null)
            {
                return NotFound();
            }

            var model = new CursoEditVm
            {
                Id = curso.Id,
                Codigo = curso.Code,
                Nombre = curso.Title,
                Descripcion = curso.Description ?? "",
                Estado = curso.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(CursoEditVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var curso = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsSoftDeleted);

                if (curso == null)
                {
                    return NotFound();
                }

                // Verificar si el código ya está en uso por otro curso
                if (curso.Code != model.Codigo)
                {
                    var codigoExistente = await _context.Courses
                        .AnyAsync(c => c.Code == model.Codigo && c.Id != model.Id && !c.IsSoftDeleted);

                    if (codigoExistente)
                    {
                        ModelState.AddModelError("Codigo", $"El código '{model.Codigo}' ya está en uso por otro curso");
                        return View(model);
                    }
                }

                // Guardar valores antiguos para auditoría
                var oldValues = new
                {
                    curso.Code,
                    curso.Title,
                    curso.Description,
                    curso.IsActive
                };

                // Actualizar curso
                curso.Code = model.Codigo;
                curso.Title = model.Nombre;
                curso.Description = model.Descripcion;
                curso.IsActive = model.Estado;

                _context.Courses.Update(curso);
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Curso actualizado
                await _auditService.LogUpdateAsync("Curso", curso.Id, $"{model.Codigo} - {model.Nombre}");

                TempData["Success"] = "Curso actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar curso {Id}", model.Id);

                //  AUDITORÍA: Error al editar curso
                await _auditService.LogAsync("CURSO_EDIT_ERROR", "Curso", model.Id,
                    $"Error al editar curso {model.Codigo} - {model.Nombre}: {ex.Message}");

                TempData["Error"] = $"Error al actualizar el curso: {ex.Message}";
                return View(model);
            }
        }

        // ========== ELIMINAR CURSO (SOFT DELETE) ==========
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var curso = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsSoftDeleted);

            if (curso == null)
            {
                return NotFound();
            }

            var model = new CursoDeleteVm
            {
                Id = curso.Id,
                Codigo = curso.Code,
                Nombre = curso.Title,
                Descripcion = curso.Description ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            try
            {
                var curso = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsSoftDeleted);

                if (curso == null)
                {
                    return NotFound();
                }

                // Verificar si el curso tiene ofertas activas
                var tieneOfertas = await _context.CourseOfferings
                    .AnyAsync(co => co.CourseId == id && !co.IsSoftDeleted);

                if (tieneOfertas)
                {
                    TempData["Error"] = "No se puede eliminar el curso porque tiene ofertas activas. Primero elimine las ofertas asociadas.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete
                curso.IsSoftDeleted = true;
                curso.IsActive = false;
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Curso eliminado
                await _auditService.LogDeleteAsync("Curso", curso.Id, $"{curso.Code} - {curso.Title}");

                TempData["Success"] = "Curso eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar curso {Id}", id);

                //  AUDITORÍA: Error al eliminar curso
                await _auditService.LogAsync("CURSO_DELETE_ERROR", "Curso", id,
                    $"Error al eliminar curso: {ex.Message}");

                TempData["Error"] = $"Error al eliminar el curso: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== ACTIVAR/DESACTIVAR CURSO ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ToggleEstado(Guid id)
        {
            try
            {
                var curso = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsSoftDeleted);

                if (curso == null)
                {
                    return NotFound();
                }

                var estadoAnterior = curso.IsActive;
                curso.IsActive = !curso.IsActive;
                await _context.SaveChangesAsync();

                //  AUDITORÍA: Estado del curso cambiado
                await _auditService.LogAsync("CURSO_TOGGLE_ESTADO", "Curso", curso.Id,
                    $"Curso {curso.Code} - {curso.Title} cambió de " +
                    $"{(estadoAnterior ? "Activo" : "Inactivo")} a {(curso.IsActive ? "Activo" : "Inactivo")}");

                var estado = curso.IsActive ? "activado" : "desactivado";
                TempData["Success"] = $"Curso {estado} exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del curso {Id}", id);

                //  AUDITORÍA: Error al cambiar estado
                await _auditService.LogAsync("CURSO_TOGGLE_ESTADO_ERROR", "Curso", id,
                    $"Error al cambiar estado del curso: {ex.Message}");

                TempData["Error"] = $"Error al cambiar el estado del curso: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult MenuDetalles(Guid id)
        {
           
            var courseOffering = _context.CourseOfferings
                .Include(co => co.Course)
                .FirstOrDefault(co => co.Id == id);

            if (courseOffering == null)
            {
                return NotFound();
            }

            var model = new DetallesCursosVM
            {
                Id = courseOffering.Id,
                nombre = courseOffering.Course.Title,
                descrption = courseOffering.Course.Description ?? string.Empty,
                TotalTareas = _context.Tareas
                    .Count(t => t.CourseOfferingId == id),
                TotalEnlaces = 1,
                TotalAnuncios = 2,
            };

            return View("DetallesMenu", model);
        }
    }
}

// ========== VIEW MODELS NECESARIOS ==========
public class CursoEditVm
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
}

public class CursoDeleteVm
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
}
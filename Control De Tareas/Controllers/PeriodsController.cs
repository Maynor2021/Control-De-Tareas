using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    public class PeriodsController : Controller
    {
        private readonly ContextDB _context;

        public PeriodsController(ContextDB context)
        {
            _context = context;
        }

        // GET: Periods
        /// <summary>
        /// Lista todos los períodos académicos.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var periods = await _context.Periods
                .Where(p => !p.IsSoftDeleted)
                .Include(p => p.CourseOfferings)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            var models = periods.Select(p => new PeriodModel
            {
                Id = p.Id,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CourseOfferingsCount = p.CourseOfferings.Count(co => !co.IsSoftDeleted)
            }).ToList();

            return View(models);
        }

        // GET: Periods/Details/5
        /// <summary>
        /// Muestra los detalles de un período específico.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var period = await _context.Periods
                .Include(p => p.CourseOfferings)
                    .ThenInclude(co => co.Course)
                .Include(p => p.CourseOfferings)
                    .ThenInclude(co => co.Professor)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            var model = new PeriodModel
            {
                Id = period.Id,
                Name = period.Name,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsActive = period.IsActive,
                CreatedAt = period.CreatedAt,
                CourseOfferingsCount = period.CourseOfferings.Count(co => !co.IsSoftDeleted)
            };

            return View(model);
        }

        // GET: Periods/Create
        /// <summary>
        /// Muestra el formulario para crear un nuevo período.
        /// </summary>
        public IActionResult Create()
        {
            var model = new PeriodModel
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddMonths(6).Date,
                IsActive = false // Por defecto inactivo hasta que se configure
            };
            return View(model);
        }

        // POST: Periods/Create
        /// <summary>
        /// Procesa la creación de un nuevo período.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PeriodModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validación: Si se intenta activar, verificar que no haya otro período activo
            if (model.IsActive)
            {
                var hasActivePeriod = await _context.Periods
                    .AnyAsync(p => p.IsActive && !p.IsSoftDeleted);

                if (hasActivePeriod)
                {
                    ModelState.AddModelError("IsActive",
                        "Ya existe un período activo. Desactívalo antes de activar este.");
                    return View(model);
                }
            }

            // Validación: Verificar que no haya solapamiento de fechas con otros períodos
            var hasOverlap = await _context.Periods
                .AnyAsync(p => !p.IsSoftDeleted &&
                    ((model.StartDate >= p.StartDate && model.StartDate <= p.EndDate) ||
                     (model.EndDate >= p.StartDate && model.EndDate <= p.EndDate) ||
                     (model.StartDate <= p.StartDate && model.EndDate >= p.EndDate)));

            if (hasOverlap)
            {
                ModelState.AddModelError("",
                    "Las fechas de este período se solapan con otro período existente.");
                return View(model);
            }

            // Crear la entidad
            var period = new Periods
            {
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now,
                IsSoftDeleted = false
            };

            _context.Periods.Add(period);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Período creado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: Periods/Edit/5
        /// <summary>
        /// Muestra el formulario para editar un período existente.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var period = await _context.Periods
                .Include(p => p.CourseOfferings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            var model = new PeriodModel
            {
                Id = period.Id,
                Name = period.Name,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsActive = period.IsActive,
                CreatedAt = period.CreatedAt,
                CourseOfferingsCount = period.CourseOfferings.Count(co => !co.IsSoftDeleted)
            };

            return View(model);
        }

        // POST: Periods/Edit/5
        /// <summary>
        /// Procesa la edición de un período existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PeriodModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var period = await _context.Periods
                .Include(p => p.CourseOfferings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            // Validación: No permitir editar si tiene ofertas de cursos
            if (period.CourseOfferings.Any(co => !co.IsSoftDeleted))
            {
                ModelState.AddModelError("",
                    "No se puede editar un período que ya tiene ofertas de cursos asignadas.");
                return View(model);
            }

            // Validación: Si se intenta activar, verificar que no haya otro período activo
            if (model.IsActive && !period.IsActive)
            {
                var hasActivePeriod = await _context.Periods
                    .AnyAsync(p => p.IsActive && !p.IsSoftDeleted && p.Id != id);

                if (hasActivePeriod)
                {
                    ModelState.AddModelError("IsActive",
                        "Ya existe un período activo. Desactívalo antes de activar este.");
                    return View(model);
                }
            }

            // Actualizar entidad
            period.Name = model.Name;
            period.StartDate = model.StartDate;
            period.EndDate = model.EndDate;
            period.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Período actualizado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: Periods/Delete/5
        /// <summary>
        /// Muestra confirmación para eliminar un período.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var period = await _context.Periods
                .Include(p => p.CourseOfferings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            var model = new PeriodModel
            {
                Id = period.Id,
                Name = period.Name,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsActive = period.IsActive,
                CreatedAt = period.CreatedAt,
                CourseOfferingsCount = period.CourseOfferings.Count(co => !co.IsSoftDeleted)
            };

            return View(model);
        }

        // POST: Periods/Delete/5
        /// <summary>
        /// Procesa la eliminación lógica (soft delete) de un período.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var period = await _context.Periods
                .Include(p => p.CourseOfferings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            // Validación: No permitir eliminar si tiene ofertas de cursos
            if (period.CourseOfferings.Any(co => !co.IsSoftDeleted))
            {
                TempData["Error"] = "No se puede eliminar un período que tiene ofertas de cursos asignadas.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete
            period.IsSoftDeleted = true;
            period.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Período eliminado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // POST: Periods/ToggleActive/5
        /// <summary>
        /// Activa o desactiva un período.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var period = await _context.Periods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            // Si se va a activar, desactivar otros períodos
            if (!period.IsActive)
            {
                var activePeriods = await _context.Periods
                    .Where(p => p.IsActive && !p.IsSoftDeleted && p.Id != id)
                    .ToListAsync();

                foreach (var ap in activePeriods)
                {
                    ap.IsActive = false;
                }
            }

            period.IsActive = !period.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = period.IsActive ?
                "Período activado exitosamente" :
                "Período desactivado exitosamente";

            return RedirectToAction(nameof(Index));
        }
    }
}
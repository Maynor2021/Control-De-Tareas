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
        public async Task<IActionResult> Index()
        {
            var periods = await _context.Periods
                .Where(p => !p.IsSoftDeleted)
                .Include(p => p.CourseOfferings)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            var models = periods.Select(p => new PeriodVm
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
        public async Task<IActionResult> Details(Guid? id)
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

            var model = new PeriodVm
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
        public IActionResult Create()
        {
            var model = new PeriodVm
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddMonths(6).Date,
                IsActive = false
            };
            return View(model);
        }

        // POST: Periods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PeriodVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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

            var period = new Periods
            {
                Id = Guid.NewGuid(),
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
        public async Task<IActionResult> Edit(Guid? id)
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

            var model = new PeriodVm
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PeriodVm model)
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

            if (period.CourseOfferings.Any(co => !co.IsSoftDeleted))
            {
                ModelState.AddModelError("",
                    "No se puede editar un período que ya tiene ofertas de cursos asignadas.");
                return View(model);
            }

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

            period.Name = model.Name;
            period.StartDate = model.StartDate;
            period.EndDate = model.EndDate;
            period.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Período actualizado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: Periods/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
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

            var model = new PeriodVm
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var period = await _context.Periods
                .Include(p => p.CourseOfferings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

            if (period.CourseOfferings.Any(co => !co.IsSoftDeleted))
            {
                TempData["Error"] = "No se puede eliminar un período que tiene ofertas de cursos asignadas.";
                return RedirectToAction(nameof(Index));
            }

            period.IsSoftDeleted = true;
            period.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Período eliminado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // POST: Periods/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var period = await _context.Periods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsSoftDeleted);

            if (period == null)
            {
                return NotFound();
            }

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
using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Controllers
{
    public class CourseOfferingsController : Controller
    {
        private readonly ContextDB _context;

        public CourseOfferingsController(ContextDB context)
        {
            _context = context;
        }

        // GET: CourseOfferings
        /// <summary>
        /// Lista todas las ofertas de cursos.
        /// </summary>
        public async Task<IActionResult> Index(int? periodId)
        {
            var query = _context.CourseOfferings
                .Where(co => !co.IsSoftDeleted)
                .Include(co => co.Course)
                .Include(co => co.Professor)
                .Include(co => co.Period)
                .Include(co => co.Enrollments)
                .Include(co => co.Tareas)
                .Include(co => co.Announcements)
                .AsQueryable();

            // Filtrar por período si se proporciona
            if (periodId.HasValue)
            {
                query = query.Where(co => co.PeriodId == periodId.Value);
            }

            var offerings = await query
          .OrderByDescending(co => co.Period.StartDate)
          .ThenBy(co => co.Course != null ? co.Course.Title : "") // ✅ FIX
          .ToListAsync(); 


            var models = offerings.Select(co => new CourseOfferingVm
            {
                Id = co.Id,
                CourseId = co.CourseId,
                ProfessorId = co.ProfessorId,
                PeriodId = co.PeriodId,
                Section = co.Section,
                CreatedAt = co.CreatedAt,
                IsActive = co.IsActive,
                CourseName = co.Course.Title,
                CourseCode = co.Course.Code ?? "",
                ProfessorName = co.Professor.UserName,
                ProfessorEmail = co.Professor.Email,
                PeriodName = co.Period.Name,
                PeriodStartDate = co.Period.StartDate,
                PeriodEndDate = co.Period.EndDate,
                EnrolledStudentsCount = co.Enrollments.Count(e => !e.IsSoftDeleted),
                TasksCount = co.Tareas.Count(t => !t.IsSoftDeleted),
                AnnouncementsCount = co.Announcements.Count(a => !a.IsSoftDeleted)
            }).ToList();

            // Pasar lista de períodos para el filtro
            ViewBag.Periods = await _context.Periods
                .Where(p => !p.IsSoftDeleted)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = periodId.HasValue && p.Id == periodId.Value
                })
                .ToListAsync();

            ViewBag.SelectedPeriodId = periodId;

            return View(models);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offering = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Professor)
                .Include(co => co.Period)
                .Include(co => co.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(co => co.Tareas)
                .Include(co => co.Announcements)
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsSoftDeleted);

            if (offering == null)
            {
                return NotFound();
            }

            var model = new CourseOfferingVm
            {
                Id = offering.Id,
                CourseId = offering.CourseId,
                ProfessorId = offering.ProfessorId,
                PeriodId = offering.PeriodId,
                Section = offering.Section,
                CreatedAt = offering.CreatedAt,
                IsActive = offering.IsActive,
                CourseName = offering.Course.Title,
                CourseCode = offering.Course.Code ?? "",
                ProfessorName = offering.Professor.UserName,
                ProfessorEmail = offering.Professor.Email,
                PeriodName = offering.Period.Name,
                PeriodStartDate = offering.Period.StartDate,
                PeriodEndDate = offering.Period.EndDate,
                EnrolledStudentsCount = offering.Enrollments.Count(e => !e.IsSoftDeleted),
                TasksCount = offering.Tareas.Count(t => !t.IsSoftDeleted),
                AnnouncementsCount = offering.Announcements.Count(a => !a.IsSoftDeleted)
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new CourseOfferingVm()); 
        }

        // POST: CourseOfferings/Create
        /// <summary>
        /// Procesa la creación de una nueva oferta de curso.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseOfferingVm model) 
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            // Validación: Verificar que no exista la misma combinación
            var exists = await _context.CourseOfferings
                .AnyAsync(co => !co.IsSoftDeleted &&
                    co.CourseId == model.CourseId &&
                    co.PeriodId == model.PeriodId &&
                    co.Section == model.Section);

            if (exists)
            {
                ModelState.AddModelError("",
                    "Ya existe una oferta para este curso, período y sección.");
                await LoadDropdownsAsync();
                return View(model);
            }

            // Crear la entidad
            var offering = new CourseOfferings
            {
                CourseId = model.CourseId,
                ProfessorId = model.ProfessorId,
                PeriodId = model.PeriodId,
                Section = model.Section,
                CreatedAt = DateTime.Now,
                IsActive = model.IsActive,
                IsSoftDeleted = false
            };

            _context.CourseOfferings.Add(offering);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oferta de curso creada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: CourseOfferings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offering = await _context.CourseOfferings
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsSoftDeleted);

            if (offering == null)
            {
                return NotFound();
            }

            var model = new CourseOfferingVm 
            {
                Id = offering.Id,
                CourseId = offering.CourseId,
                ProfessorId = offering.ProfessorId,
                PeriodId = offering.PeriodId,
                Section = offering.Section,
                CreatedAt = offering.CreatedAt,
                IsActive = offering.IsActive
            };

            await LoadDropdownsAsync(model);
            return View(model);
        }

        // POST: CourseOfferings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseOfferingVm model) 
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var offering = await _context.CourseOfferings
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsSoftDeleted);

            if (offering == null)
            {
                return NotFound();
            }

            // Validación: Verificar combinación única (excluyendo la actual)
            var exists = await _context.CourseOfferings
                .AnyAsync(co => !co.IsSoftDeleted &&
                    co.Id != id &&
                    co.CourseId == model.CourseId &&
                    co.PeriodId == model.PeriodId &&
                    co.Section == model.Section);

            if (exists)
            {
                ModelState.AddModelError("",
                    "Ya existe otra oferta para este curso, período y sección.");
                await LoadDropdownsAsync(model);
                return View(model);
            }

            // Actualizar
            offering.CourseId = model.CourseId;
            offering.ProfessorId = model.ProfessorId;
            offering.PeriodId = model.PeriodId;
            offering.Section = model.Section;
            offering.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Oferta actualizada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: CourseOfferings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offering = await _context.CourseOfferings
                .Include(co => co.Course)
                .Include(co => co.Professor)
                .Include(co => co.Period)
                .Include(co => co.Enrollments)
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsSoftDeleted);

            if (offering == null)
            {
                return NotFound();
            }

            var model = new CourseOfferingVm 
            {
                Id = offering.Id,
                CourseName = offering.Course.Title,
                ProfessorName = offering.Professor.UserName,
                PeriodName = offering.Period.Name,
                Section = offering.Section,
                EnrolledStudentsCount = offering.Enrollments.Count(e => !e.IsSoftDeleted)
            };

            return View(model);
        }

        // POST: CourseOfferings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offering = await _context.CourseOfferings
                .Include(co => co.Enrollments)
                .FirstOrDefaultAsync(co => co.Id == id && !co.IsSoftDeleted);

            if (offering == null)
            {
                return NotFound();
            }

            // Validación: No permitir eliminar si tiene estudiantes inscritos
            if (offering.Enrollments.Any(e => !e.IsSoftDeleted))
            {
                TempData["Error"] = "No se puede eliminar una oferta que tiene estudiantes inscritos.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete
            offering.IsSoftDeleted = true;
            offering.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Oferta eliminada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Carga los dropdowns para el formulario.
        /// </summary>
        private async Task LoadDropdownsAsync(CourseOfferingVm? model = null) 
        {
            ViewBag.Courses = await _context.Courses
                .Where(c => !c.IsSoftDeleted)
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Title}",
                    Selected = model != null && c.Id == model.CourseId
                })
                .ToListAsync();

            ViewBag.Professors = await _context.Users
                .Where(u => !u.IsSoftDeleted && u.UserRoles.Any(ur => ur.Role.RoleName == "Profesor"))
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.UserName,
                    Selected = model != null && u.UserId == model.ProfessorId
                })
                .ToListAsync();

            ViewBag.Periods = await _context.Periods
                .Where(p => !p.IsSoftDeleted)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Selected = model != null && p.Id == model.PeriodId
                })
                .ToListAsync();
        }
    }
}
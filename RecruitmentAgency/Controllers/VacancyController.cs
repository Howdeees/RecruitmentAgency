using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

namespace RecruitmentAgency.Controllers
{
    [Authorize] // По умолчанию всё закрыто
    public class VacancyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VacancyController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, string schedule, decimal? minSalary)
        {
            var vacanciesQuery = _context.Vacancies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                vacanciesQuery = vacanciesQuery.Where(v => v.Title.Contains(searchString)
                                                     || v.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(schedule))
            {
                vacanciesQuery = vacanciesQuery.Where(v => v.Schedule == schedule);
            }

            if (minSalary.HasValue)
            {
                vacanciesQuery = vacanciesQuery.Where(v => v.Salary >= minSalary.Value);
            }

            ViewBag.Schedules = await _context.Vacancies
                .Select(v => v.Schedule)
                .Distinct()
                .Where(s => s != null)
                .ToListAsync();

            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentSchedule"] = schedule;
            ViewData["CurrentMinSalary"] = minSalary;

            var result = await vacanciesQuery.OrderByDescending(v => v.CreatedDate).ToListAsync();
            return View(result);
        }

        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> MyVacancies()
        {
            var userId = _userManager.GetUserId(User);

            var myVacancies = await _context.Vacancies
                .Where(v => v.EmployerId == userId || User.IsInRole("Admin")) // Админ видит всё, работодатель — своё
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            return View(myVacancies);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vacancy = await _context.Vacancies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vacancy == null) return NotFound();

            vacancy.ViewsCount++;
            _context.Update(vacancy);
            await _context.SaveChangesAsync();

            return View(vacancy);
        }

        [Authorize(Roles = "Employer,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Employer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vacancy vacancy, IFormFile? imageFile)
        {
          

            if (ModelState.IsValid)
            {
                vacancy.EmployerId = _userManager.GetUserId(User);

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/vacancies");

                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    vacancy.ImagePath = fileName;
                }

                vacancy.CreatedDate = DateTime.Now;
                vacancy.ViewsCount = 0;

                _context.Add(vacancy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyVacancies));
            }

            return View(vacancy);
        }

        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && vacancy.EmployerId != userId)
            {
                return Forbid();
            }

            return View(vacancy);
        }

        [HttpPost]
        [Authorize(Roles = "Employer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vacancy vacancy, IFormFile? imageFile)
        {
            if (id != vacancy.Id) return NotFound();

            ModelState.Remove("imageFile");
            ModelState.Remove("ImagePath");
            ModelState.Remove("EmployerId");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVacancy = await _context.Vacancies.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                    if (existingVacancy == null) return NotFound();

                    var userId = _userManager.GetUserId(User);
                    if (!User.IsInRole("Admin") && existingVacancy.EmployerId != userId)
                    {
                        return Forbid();
                    }

                    vacancy.EmployerId = existingVacancy.EmployerId;
                    vacancy.CreatedDate = existingVacancy.CreatedDate;
                    vacancy.ViewsCount = existingVacancy.ViewsCount;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/vacancies");

                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        if (!string.IsNullOrEmpty(existingVacancy.ImagePath))
                        {
                            var oldPath = Path.Combine(uploadDir, existingVacancy.ImagePath);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        vacancy.ImagePath = fileName;
                    }
                    else
                    {
                        vacancy.ImagePath = existingVacancy.ImagePath;
                    }

                    _context.Update(vacancy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VacancyExists(vacancy.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(MyVacancies));
            }
            return View(vacancy);
        }

        [HttpPost]
        [Authorize(Roles = "Employer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var vacancy = await _context.Vacancies.FindAsync(id);

            if (vacancy == null) return NotFound();

            if (!User.IsInRole("Admin") && vacancy.EmployerId != currentUserId)
            {
                TempData["ErrorMessage"] = "Ошибка доступа.";
                return RedirectToAction(nameof(MyVacancies));
            }

            var relatedApplications = _context.Applications.Where(a => a.VacancyId == id);
            _context.Applications.RemoveRange(relatedApplications);

            if (!string.IsNullOrEmpty(vacancy.ImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/vacancies", vacancy.ImagePath);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Vacancies.Remove(vacancy);
            await _context.SaveChangesAsync();

            TempData["Info"] = "Вакансия удалена.";
            return RedirectToAction(nameof(MyVacancies));
        }

        private bool VacancyExists(int id) => _context.Vacancies.Any(e => e.Id == id);
    }
}
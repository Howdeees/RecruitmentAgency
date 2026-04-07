using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RecruitmentAgency.Controllers
{
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
            var vacanciesQuery = _context.Vacancies.Include(v => v.Employer).AsQueryable();

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

            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentSchedule"] = schedule;
            ViewData["CurrentMinSalary"] = minSalary;

            ViewBag.Schedules = await _context.Vacancies
                .Select(v => v.Schedule)
                .Distinct()
                .Where(s => s != null)
                .ToListAsync();

            var result = await vacanciesQuery.OrderByDescending(v => v.CreatedDate).ToListAsync();
            return View(result);
        }
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> MyVacancies()
        {
            var userId = _userManager.GetUserId(User);

            var myVacancies = await _context.Vacancies
                .Where(v => v.EmployerId == userId)
                .Include(v => v.Applications)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            return View(myVacancies);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.Employer)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vacancy == null) return NotFound();

            return View(vacancy);
        }

        [Authorize(Roles = "Employer,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vacancy vacancy)
        {
            vacancy.EmployerId = _userManager.GetUserId(User);

            ModelState.Remove("Employer");
            ModelState.Remove("EmployerId");

            if (ModelState.IsValid)
            {
                _context.Add(vacancy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, Vacancy vacancy)
        {
            if (id != vacancy.Id) return NotFound();

            var existingVacancy = await _context.Vacancies
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (existingVacancy == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && existingVacancy.EmployerId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vacancy.EmployerId = existingVacancy.EmployerId;
                    vacancy.CreatedDate = existingVacancy.CreatedDate;

                    _context.Update(vacancy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vacancies.Any(e => e.Id == vacancy.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
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

            if (vacancy == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && vacancy.EmployerId != currentUserId)
            {
                TempData["ErrorMessage"] = "Ошибка доступа: вы можете удалять только свои вакансии.";
                return RedirectToAction(nameof(MyVacancies));
            }

            var relatedApplications = _context.Applications.Where(a => a.VacancyId == id);
            _context.Applications.RemoveRange(relatedApplications);

            _context.Vacancies.Remove(vacancy);
            await _context.SaveChangesAsync();

            TempData["Info"] = "Вакансия успешно удалена.";
            return RedirectToAction(nameof(MyVacancies));
        }
    }
}
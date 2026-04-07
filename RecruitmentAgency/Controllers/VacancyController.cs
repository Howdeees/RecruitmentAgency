using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

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

        // СПИСОК ВАКАНСИЙ
        public async Task<IActionResult> Index()
        {
            var vacancies = await _context.Vacancies
                .Include(v => v.Employer)
                .ToListAsync();

            return View(vacancies);
        }

        // СОЗДАНИЕ ВАКАНСИИ
        [Authorize(Roles = "Employer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Create(Vacancy vacancy)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                vacancy.EmployerId = user.Id;
                vacancy.CreatedDate = DateTime.Now;

                _context.Add(vacancy);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(vacancy);
        }

        // ДЕТАЛИ
        public async Task<IActionResult> Details(int id)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.Employer)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vacancy == null)
                return NotFound();

            return View(vacancy);
        }

        // УДАЛЕНИЕ
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Delete(int id)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);

            if (vacancy != null)
            {
                _context.Vacancies.Remove(vacancy);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
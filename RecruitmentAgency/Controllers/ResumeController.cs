using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

namespace RecruitmentAgency.Controllers
{
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ResumeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // СПИСОК РЕЗЮМЕ
        public async Task<IActionResult> Index()
        {
            var resumes = await _context.Resumes.ToListAsync();
            return View(resumes);
        }

        // СОЗДАНИЕ (GET)
        public IActionResult Create()
        {
            return View();
        }

        // СОЗДАНИЕ (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Resume resume)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                resume.UserId = user?.Id;

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(resume);
        }

        // ДЕТАЛИ
        public async Task<IActionResult> Details(int id)
        {
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resume == null)
                return NotFound();

            return View(resume);
        }

        // УДАЛЕНИЕ
        public async Task<IActionResult> Delete(int id)
        {
            var resume = await _context.Resumes.FindAsync(id);

            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
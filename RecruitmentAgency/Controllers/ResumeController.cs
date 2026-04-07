using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

namespace RecruitmentAgency.Controllers
{
    [Authorize]
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ResumeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedDate)
                .ToListAsync();
            return View(resumes);
        }

        // Создание
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resume resume)
        {

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId)) return Challenge();

                resume.UserId = userId;
                resume.UpdatedDate = DateTime.Now;

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(resume);
        }

        // Редактирование
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null || resume.UserId != _userManager.GetUserId(User))
                return Forbid();

            return View(resume);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resume resume)
        {
            if (id != resume.Id) return NotFound();

            var existingResume = await _context.Resumes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existingResume == null || existingResume.UserId != _userManager.GetUserId(User))
                return Forbid();

            if (ModelState.IsValid)
            {
                resume.UserId = existingResume.UserId;
                resume.UpdatedDate = DateTime.Now;
                _context.Update(resume);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(resume);
        }

        public async Task<IActionResult> Details(int id)
        {
            var resume = await _context.Resumes.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (resume == null) return NotFound();

            return View(resume);
        }
    }
}
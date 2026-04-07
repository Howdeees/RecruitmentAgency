using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

[Authorize]
public class ApplicationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ApplicationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<IActionResult> MyApplications()
    {
        var userId = _userManager.GetUserId(User);

        var applications = await _context.Applications
            .Include(a => a.Vacancy)
            .Include(a => a.Resume) 
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();

        return View(applications);
    }

    [HttpPost]
    [Authorize(Roles = "Employer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int applicationId, ApplicationStatus newStatus)
    {
        var application = await _context.Applications
            .Include(a => a.Vacancy)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (application.Vacancy.EmployerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        application.Status = newStatus;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Incoming), new { vacancyId = application.VacancyId });
    }

    [HttpGet]
    public async Task<IActionResult> Apply(int vacancyId)
    {
        if (User.IsInRole("Employer"))
        {
            TempData["ErrorMessage"] = "Ошибка доступа: Учетная запись работодателя не может отправлять отклики на вакансии.";

            return RedirectToAction("Details", "Vacancy", new { id = vacancyId });
        }
        var vacancy = await _context.Vacancies.FindAsync(vacancyId);
        if (vacancy == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        var userResumes = await _context.Resumes
            .Where(r => r.UserId == userId)
            .ToListAsync();

        if (!userResumes.Any())
        {
            TempData["Info"] = "Для отклика на вакансию необходимо создать хотя бы одно резюме.";
            return RedirectToAction("Create", "Resume");
        }

        ViewBag.VacancyTitle = vacancy.Title;
        ViewBag.Resumes = userResumes;

        return View(new Application { VacancyId = vacancyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(Application application)
    {
        if (User.IsInRole("Employer")) return Forbid();
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();


        bool alreadyApplied = await _context.Applications
            .AnyAsync(a => a.VacancyId == application.VacancyId && a.UserId == userId);

        if (alreadyApplied)
        {
            ModelState.AddModelError("", "Вы уже подали заявку на эту вакансию.");
        }

        if (ModelState.IsValid)
        {
            application.UserId = userId;
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Vacancy");
        }

        ViewBag.Resumes = await _context.Resumes.Where(r => r.UserId == userId).ToListAsync();
        return View(application);
    }

    [Authorize(Roles = "Employer,Admin")]
    public async Task<IActionResult> Incoming(int? vacancyId)
    {
        var userId = _userManager.GetUserId(User);

        var myVacancies = await _context.Vacancies
            .Where(v => v.EmployerId == userId || User.IsInRole("Admin"))
            .ToListAsync();

        ViewBag.Vacancies = myVacancies;
        ViewBag.SelectedVacancyId = vacancyId;

        var query = _context.Applications
            .Include(a => a.Vacancy)
            .Include(a => a.User)
            .Include(a => a.Resume)
            .Where(a => a.Vacancy.EmployerId == userId || User.IsInRole("Admin"))
            .AsQueryable();

        if (vacancyId.HasValue)
        {
            query = query.Where(a => a.VacancyId == vacancyId.Value);
        }

        var applications = await query.OrderByDescending(a => a.AppliedDate).ToListAsync();
        return View(applications);
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);

        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (application == null)
        {
            TempData["ErrorMessage"] = "Отклик не найден.";
            return RedirectToAction("MyApplications");
        }

        _context.Applications.Remove(application);
        await _context.SaveChangesAsync();

        TempData["Info"] = "Отклик успешно отозван.";
        return RedirectToAction("MyApplications");
    }
}
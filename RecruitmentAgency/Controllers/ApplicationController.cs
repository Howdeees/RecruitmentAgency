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
    [Authorize(Roles = "Admin,Recruiter,Employer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int applicationId, RecruitmentAgency.Models.ApplicationStatus status, string? note, int? currentVacancyId)
    {
        var application = await _context.Applications
            .Include(a => a.Vacancy)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("Recruiter");
        bool isOwner = application.Vacancy.EmployerId == userId;

        if (!isStaff && !isOwner)
        {
            return Forbid();
        }

        application.Status = status;

        application.RecruiterNotes = note;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Incoming), new { vacancyId = currentVacancyId });
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

    [Authorize(Roles = "Employer,Admin,Recruiter")]
    public async Task<IActionResult> Incoming(int? vacancyId)
    {
        var userId = _userManager.GetUserId(User);
        bool isStaff = User.IsInRole("Admin") || User.IsInRole("Recruiter");

        var vacancyQuery = _context.Vacancies.AsQueryable();

        if (!isStaff)
        {
            vacancyQuery = vacancyQuery.Where(v => v.EmployerId == userId);
        }

        ViewBag.Vacancies = await vacancyQuery.ToListAsync();
        ViewBag.SelectedVacancyId = vacancyId;


        var query = _context.Applications
            .Include(a => a.Vacancy)
            .Include(a => a.User)
            .Include(a => a.Resume)
            .AsQueryable();


        if (!isStaff)
        {
            query = query.Where(a => a.Vacancy.EmployerId == userId);
        }

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
    [HttpPost]
    [Authorize(Roles = "Employer,Recruiter,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNote(int applicationId, string note, int? vacancyId)
    {
        var application = await _context.Applications
            .Include(a => a.Vacancy)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (User.IsInRole("Employer") && !User.IsInRole("Admin") && !User.IsInRole("Recruiter"))
        {
            if (application.Vacancy.EmployerId != userId)
            {
                return Forbid();
            }
        }

        application.RecruiterNotes = note;
        await _context.SaveChangesAsync();

        TempData["Info"] = "Комментарий обновлен";

        return RedirectToAction(nameof(Incoming), new { vacancyId = vacancyId });
    }
}
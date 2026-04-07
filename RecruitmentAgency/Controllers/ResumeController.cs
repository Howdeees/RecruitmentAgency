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

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resume resume, IFormFile? imageFile)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("ProfilePicture");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId)) return Challenge();

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");

                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    resume.ProfilePicture = fileName;
                }

                resume.UserId = userId;
                resume.UpdatedDate = DateTime.Now;

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(resume);
        }

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
        public async Task<IActionResult> Edit(int id, Resume resume, IFormFile? imageFile)
        {
            if (id != resume.Id) return NotFound();

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    // Находим оригинальную запись в базе без отслеживания (AsNoTracking), 
                    // чтобы подтянуть старое имя фото и UserId
                    var existingResume = await _context.Resumes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

                    if (existingResume == null) return NotFound();

                    resume.UserId = existingResume.UserId; // Сохраняем владельца
                    resume.UpdatedDate = DateTime.Now;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Логика загрузки нового фото
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Удаляем старый файл с диска, если он был
                        if (!string.IsNullOrEmpty(existingResume.ProfilePicture))
                        {
                            var oldPath = Path.Combine(uploadDir, existingResume.ProfilePicture);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        resume.ProfilePicture = fileName;
                    }
                    else
                    {
                        // Если файл не выбран, оставляем старое имя фото
                        resume.ProfilePicture = existingResume.ProfilePicture;
                    }

                    _context.Update(resume);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResumeExists(resume.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(resume);
        }
        private bool ResumeExists(int id)
        {
            return _context.Resumes.Any(e => e.Id == id);
        }
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var resume = await _context.Resumes
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (resume == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            bool isOwner = resume.UserId == userId;
            bool isStaff = User.IsInRole("Employer") || User.IsInRole("Admin") || User.IsInRole("Recruiter");

            if (!isOwner && !isStaff)
            {
                return Forbid();
            }

            return View(resume);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (resume == null)
            {
                TempData["ErrorMessage"] = "Резюме не найдено или у вас нет прав на его удаление.";
                return RedirectToAction(nameof(Index));
            }

            var hasApplications = await _context.Applications.AnyAsync(a => a.ResumeId == id);
            if (hasApplications)
            {
                TempData["ErrorMessage"] = "Нельзя удалить резюме, по которому есть активные отклики. Сначала отозовите отклики.";
                return RedirectToAction(nameof(Index));
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();

            TempData["Info"] = "Резюме успешно удалено.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Recruiter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToTalentPool(int resumeId, string? comment, string returnUrl)
        {
            var resume = await _context.Resumes.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == resumeId);
            if (resume == null) return NotFound();

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ResumeId == resumeId);

            if (application == null)
            {
                var defaultVacancy = await _context.Vacancies.FirstOrDefaultAsync();
                if (defaultVacancy == null) return BadRequest("В системе должна быть хотя бы одна вакансия.");

                application = new Application
                {
                    ResumeId = resumeId,
                    UserId = resume.UserId,
                    AppliedDate = DateTime.Now,
                    Status = ApplicationStatus.Recommended,
                    RecruiterNotes = comment ?? "Рекомендован из общего списка",
                    VacancyId = defaultVacancy.Id,
                    CoverLetter = "Добавлен рекрутером в золотой фонд"
                };
                _context.Applications.Add(application);
            }
            else
            {
                application.Status = ApplicationStatus.Recommended;
                if (!string.IsNullOrEmpty(comment)) application.RecruiterNotes = comment;
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> AllResumes()
        {
            var resumes = await _context.Resumes
                .Include(r => r.User)
                .OrderByDescending(r => r.UpdatedDate)
                .ToListAsync();

            return View(resumes);
        }
    }
}
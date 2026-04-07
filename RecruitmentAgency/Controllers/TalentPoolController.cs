using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

namespace RecruitmentAgency.Controllers
{
    public class TalentPoolController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TalentPoolController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin,Recruiter,Employer")]
        public async Task<IActionResult> VerifiedList()
        {
            var talents = await _context.Applications
                .Include(a => a.Resume)
                .Include(a => a.Vacancy)
                .Include(a => a.User)
                .Where(a => a.Status == ApplicationStatus.Recommended)
                .ToListAsync();

            return View(talents);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;
using RecruitmentAgency.Models;

public class AdminController : Controller
{
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> Dashboard()
    {
        var stats = new StatsViewModel
        {
            TotalVacancies = await _context.Vacancies.CountAsync(),
            TotalApplications = await _context.Applications.CountAsync(),
            HiredCount = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Accepted),

            ApplicationsByStatus = await _context.Applications
                .GroupBy(a => a.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),

            TopVacancies = await _context.Applications
                .GroupBy(a => a.Vacancy.Title)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new VacancyStats { Title = g.Key, Count = g.Count() })
                .ToListAsync()
        };

        return View(stats);
    }


    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    public AdminController(UserManager<IdentityUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRolesViewModel = new List<UserRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRolesViewModel.Add(new UserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles
            });
        }

        ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return View(userRolesViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRole(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(newRole))
        {
            await _userManager.AddToRoleAsync(user, newRole);
        }

        TempData["Info"] = $"Роль пользователя {user.Email} изменена на {newRole}";
        return RedirectToAction(nameof(Users));
    }

}

public class UserRolesViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; }
}
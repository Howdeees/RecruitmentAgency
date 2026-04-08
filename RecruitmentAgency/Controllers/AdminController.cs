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
        var allVacancies = await _context.Vacancies.ToListAsync();
        var allApplications = await _context.Applications.Include(a => a.Vacancy).ToListAsync();

        var stats = new StatsViewModel
        {
            TotalVacancies = allVacancies.Count,
            TotalApplications = allApplications.Count,

            HiredCount = allApplications.Count(a => a.Status == ApplicationStatus.Accepted),

            TotalViews = allVacancies.Sum(v => v.ViewsCount),

            ApplicationsByStatus = allApplications
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),

            TopVacanciesByApps = allApplications
                .GroupBy(a => a.Vacancy.Title)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToList(),

            TopVacanciesByViews = allVacancies
                .OrderByDescending(v => v.ViewsCount)
                .Take(5)
                .Select(v => new KeyValuePair<string, int>(v.Title, v.ViewsCount))
                .ToList()
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
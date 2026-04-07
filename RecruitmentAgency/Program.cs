using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=RecruitmentAgencyDB;Trusted_Connection=True;MultipleActiveResultSets=true"));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "Employer", "Recruiter" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    string recruiterEmail = "david.aloyan.00@mail.ru";
    var recruiterUser = await userManager.FindByEmailAsync(recruiterEmail);
    if (recruiterUser != null && !await userManager.IsInRoleAsync(recruiterUser, "Recruiter"))
    {
        await userManager.AddToRoleAsync(recruiterUser, "Recruiter");
    }
    string employerEmail = "david.aloyan.03@mail.ru";
    var employerUser = await userManager.FindByEmailAsync(employerEmail);
    if (employerUser != null && !await userManager.IsInRoleAsync(employerUser, "Employer"))
    {
        await userManager.AddToRoleAsync(employerUser, "Employer");
    }
    string recruiterEmail2 = "david.aloyan.04@mail.ru";
    var recruiterUser2 = await userManager.FindByEmailAsync(recruiterEmail2);
    if (recruiterUser2 != null && !await userManager.IsInRoleAsync(recruiterUser2, "Recruiter"))
    {
        await userManager.AddToRoleAsync(recruiterUser2, "Recruiter");
    }
}

app.Run();
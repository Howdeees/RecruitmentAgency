using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecruitmentAgency.Data;

var builder = WebApplication.CreateBuilder(args);
// Подключение к БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=RecruitmentAgencyDB;Trusted_Connection=True;MultipleActiveResultSets=true"));

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Роутинг
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string email = "david.aloyan.00@mail.ru";
    var user = await userManager.FindByEmailAsync(email);

    if (user != null)
    {
        if (!await roleManager.RoleExistsAsync("Employer"))
        {
            await roleManager.CreateAsync(new IdentityRole("Employer"));
        }

        await userManager.AddToRoleAsync(user, "Employer");
    }
}
app.Run();
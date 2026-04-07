using Microsoft.AspNetCore.Mvc;

namespace RecruitmentAgency.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

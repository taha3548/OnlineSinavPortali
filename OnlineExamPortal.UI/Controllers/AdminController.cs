using Microsoft.AspNetCore.Mvc;

namespace OnlineExamPortal.UI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Exams()
        {
            return View();
        }

        public IActionResult Questions()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult RoleManagement()
        {
            return View();
        }

        public IActionResult Results()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}

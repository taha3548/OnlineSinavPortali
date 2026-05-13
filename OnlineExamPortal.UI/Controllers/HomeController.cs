using Microsoft.AspNetCore.Mvc;

namespace OnlineExamPortal.UI.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Exam(int id)
    {
        ViewBag.ExamId = id;
        return View();
    }

    public IActionResult Leaderboard()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}

using Microsoft.AspNetCore.Mvc;

namespace OnlineExamPortal.UI.Controllers;

public class StudentController : Controller
{
    public IActionResult MyResults()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }
}

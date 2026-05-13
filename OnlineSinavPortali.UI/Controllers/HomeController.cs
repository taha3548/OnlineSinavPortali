using Microsoft.AspNetCore.Mvc;

namespace OnlineSinavPortali.UI.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Sinav(int id)
    {
        ViewBag.SinavId = id;
        return View();
    }

    public IActionResult Liderlik()
    {
        return View();
    }
}

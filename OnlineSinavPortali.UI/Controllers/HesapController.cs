using Microsoft.AspNetCore.Mvc;

namespace OnlineSinavPortali.UI.Controllers;

public class HesapController : Controller
{
    public IActionResult Giris()
    {
        return View();
    }

    public IActionResult Kayit()
    {
        return View();
    }
}

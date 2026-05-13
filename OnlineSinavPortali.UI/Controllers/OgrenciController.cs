using Microsoft.AspNetCore.Mvc;

namespace OnlineSinavPortali.UI.Controllers
{
    public class OgrenciController : Controller
    {
        public IActionResult Profil()
        {
            return View();
        }

        public IActionResult Sonuclarim()
        {
            return View();
        }
    }
}

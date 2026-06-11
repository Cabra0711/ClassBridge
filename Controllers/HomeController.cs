using Microsoft.AspNetCore.Mvc;

namespace IUE.DesatrasadorMVP.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Profesor")) return RedirectToAction("Index", "Profesor");
            return RedirectToAction("Dashboard", "Clase", new { id = 1 });
        }

        return RedirectToAction("Login", "Account");
    }
}

using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IUE.DesatrasadorMVP.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Profesor")) return RedirectToAction("Index", "Profesor");

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var estudiante = await _db.Estudiantes.FirstOrDefaultAsync(e => e.Correo == user.Email);
                if (estudiante == null)
                {
                    estudiante = new Estudiante
                    {
                        Nombre = user.NombreCompleto ?? user.UserName ?? user.Email ?? "Estudiante",
                        Correo = user.Email ?? user.UserName ?? string.Empty
                    };
                    _db.Estudiantes.Add(estudiante);
                    await _db.SaveChangesAsync();
                }

                return RedirectToAction("Dashboard", "Clase", new { id = estudiante.Id });
            }
        }

        return RedirectToAction("Login", "Account");
    }
}

using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IUE.DesatrasadorMVP.Controllers;

[Authorize(Roles = "Admin,Profesor")]
public class ProfesorController : Controller
{
    private readonly AppDbContext _db;

    public ProfesorController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var clases = await _db.Clases
            .Include(c => c.Materia)
            .OrderBy(c => c.Materia.Nombre)
            .ThenBy(c => c.Titulo)
            .ToListAsync();

        ViewBag.Clases = clases;
        return View();
    }
}

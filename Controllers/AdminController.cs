using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IUE.DesatrasadorMVP.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.OrderBy(u => u.NombreCompleto).ToListAsync();
        var estudiantes = await _userManager.GetUsersInRoleAsync("Estudiante");
        var profesores = await _userManager.GetUsersInRoleAsync("Profesor");
        var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        var materias = await _db.Materias.Include(m => m.Profesor).OrderBy(m => m.Nombre).ToListAsync();

        ViewBag.Roles = roles;
        ViewBag.Profesores = profesores;
        ViewBag.Estudiantes = estudiantes;
        ViewBag.Materias = materias;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarRol(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, roleName);

        TempData["Exito"] = $"Rol actualizado para {user.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProfesor(string nombreCompleto, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(nombreCompleto))
        {
            TempData["Error"] = "Completa nombre, correo y contraseña.";
            return RedirectToAction(nameof(Index));
        }

        var exists = await _userManager.FindByEmailAsync(email);
        if (exists != null)
        {
            TempData["Error"] = "Ese correo ya existe.";
            return RedirectToAction(nameof(Index));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            NombreCompleto = nombreCompleto
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Profesor");
            TempData["Exito"] = "Profesor creado correctamente.";
        }
        else
        {
            TempData["Error"] = string.Join(" | ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEstudiante(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Estudiante") || roles.Contains("Admin"))
        {
            await _userManager.RemoveFromRolesAsync(user, roles);
            await _userManager.DeleteAsync(user);
            TempData["Exito"] = "Estudiante eliminado.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarProfesorMateria(int materiaId, string? profesorId)
    {
        var materia = await _db.Materias.FindAsync(materiaId);
        if (materia == null) return NotFound();

        materia.ProfesorId = profesorId;
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Profesor asignado a la materia.";
        return RedirectToAction(nameof(Index));
    }
}

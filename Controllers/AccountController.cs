using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IUE.DesatrasadorMVP.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(AppDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);

        if (user != null)
        {
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");

                if (await _userManager.IsInRoleAsync(user, "Profesor"))
                    return RedirectToAction("Index", "Profesor");

                var estudiante = await AsegurarEstudianteAsync(user);
                return RedirectToAction("Dashboard", "Clase", new { id = estudiante?.Id ?? 1 });
            }
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña inválidos.");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string nombreCompleto, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Todos los campos son obligatorios.");
            return View();
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
            await _userManager.AddToRoleAsync(user, "Estudiante");
            var estudiante = await AsegurarEstudianteAsync(user);
            await _signInManager.SignInAsync(user, isPersistent: true);
            return RedirectToAction("Dashboard", "Clase", new { id = estudiante?.Id ?? 1 });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }

    [HttpGet]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private async Task<Estudiante?> AsegurarEstudianteAsync(ApplicationUser user)
    {
        var estudiante = await _db.Estudiantes.FirstOrDefaultAsync(e => e.Correo == user.Email);
        if (estudiante != null)
            return estudiante;

        estudiante = new Estudiante
        {
            Nombre = user.NombreCompleto ?? user.UserName ?? user.Email ?? "Estudiante",
            Correo = user.Email ?? user.UserName ?? string.Empty
        };

        _db.Estudiantes.Add(estudiante);
        await _db.SaveChangesAsync();
        return estudiante;
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}

using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IUE.DesatrasadorMVP.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
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
        var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");
                if (await _userManager.IsInRoleAsync(user, "Profesor"))
                    return RedirectToAction("Index", "Profesor");
            }
            return RedirectToLocal(returnUrl);
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
            await _signInManager.SignInAsync(user, isPersistent: true);
            return RedirectToAction("Dashboard", "Clase", new { id = 1 });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}

using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace IUE.DesatrasadorMVP.Controllers;

public class ClaseController : Controller
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClaseController(AppDbContext db, IHttpClientFactory http, IConfiguration config, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _http = http;
        _config = config;
        _userManager = userManager;
    }

    // ─────────────────────────────────────────────────────────
    // VISTA PROFESOR: Formulario para subir video
    // GET /Clase/CreateVideo
    // ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Profesor")]
    public async Task<IActionResult> CreateVideo()
    {
        await CargarSelectClases();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Profesor")]
    public async Task<IActionResult> CreateVideo(int claseId, string videoUrl, string? resumen)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            ModelState.AddModelError("", "La URL del video es requerida.");
            await CargarSelectClases();
            return View();
        }

        // Verificar si ya existe un video para esa clase y actualizarlo
        var videoExistente = await _db.VideoClases.FirstOrDefaultAsync(v => v.ClaseId == claseId);
        if (videoExistente != null)
        {
            videoExistente.VideoUrl  = videoUrl;
            videoExistente.SubidoEn  = DateTime.Now;
            _db.VideoClases.Update(videoExistente);
        }
        else
        {
            _db.VideoClases.Add(new VideoClase
            {
                ClaseId  = claseId,
                VideoUrl = videoUrl,
                SubidoEn = DateTime.Now
            });
        }

        // Guardar resumen en la clase
        var clase = await _db.Clases.FindAsync(claseId);
        if (clase != null && !string.IsNullOrWhiteSpace(resumen))
            clase.Resumen = resumen;

        await _db.SaveChangesAsync();

        // Disparar notificación a n8n (sin bloquear)
        _ = NotificarN8nAsync(claseId, videoUrl);

        TempData["Exito"] = "✅ Video registrado correctamente. Estudiantes notificados.";
        return RedirectToAction(nameof(CreateVideo));
    }

    // ─────────────────────────────────────────────────────────
    // VISTA ESTUDIANTE: Dashboard con materias
    // GET /Clase/Dashboard/{estudianteId}
    // ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Estudiante")]
    public async Task<IActionResult> Dashboard(int? id = null)
    {
        var estudianteId = id ?? 0;

        if (estudianteId <= 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var estudiantePorUsuario = await _db.Estudiantes.FirstOrDefaultAsync(e => e.Correo == user.Email);
                if (estudiantePorUsuario != null)
                    estudianteId = estudiantePorUsuario.Id;
            }
        }

        if (estudianteId <= 0)
            estudianteId = 1;

        var estudiantes = await _db.Estudiantes.ToListAsync();
        ViewBag.Estudiantes = new SelectList(estudiantes, "Id", "Nombre", estudianteId);

        var estudiante = await _db.Estudiantes
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Materia)
                    .ThenInclude(m => m.Clases)
                        .ThenInclude(c => c.VideoClase)
            .Include(e => e.Excusas)
            .FirstOrDefaultAsync(e => e.Id == estudianteId);

        if (estudiante == null) return NotFound("Estudiante no encontrado.");
        return View(estudiante);
    }

    // ─────────────────────────────────────────────────────────
    // DETALLE: Reproduce el video si la excusa está aprobada
    // GET /Clase/Details/{claseId}?estudianteId=1
    // ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Estudiante")]
    public async Task<IActionResult> Details(int id, int estudianteId = 1)
    {
        var clase = await _db.Clases
            .Include(c => c.Materia)
            .Include(c => c.VideoClase)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (clase == null) return NotFound();

        var excusaAprobada = await _db.Excusas
            .AnyAsync(e => e.ClaseId == id
                        && e.EstudianteId == estudianteId
                        && e.Estado == EstadoExcusa.Aprobada);

        ViewBag.ExcusaAprobada = excusaAprobada;
        ViewBag.EstudianteId   = estudianteId;

        return View(clase);
    }

    [Authorize(Roles = "Admin,Estudiante")]
    [HttpGet]
    public async Task<IActionResult> EnviarExcusa(int claseId, int? estudianteId = null)
    {
        var resolvedEstudianteId = estudianteId;
        if (resolvedEstudianteId == null || resolvedEstudianteId <= 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var estudiantePorUsuario = await _db.Estudiantes.FirstOrDefaultAsync(e => e.Correo == user.Email);
                if (estudiantePorUsuario != null)
                    resolvedEstudianteId = estudiantePorUsuario.Id;
            }
        }

        var clase = await _db.Clases.Include(c => c.Materia).FirstOrDefaultAsync(c => c.Id == claseId);
        var estudiante = resolvedEstudianteId.HasValue
            ? await _db.Estudiantes.FindAsync(resolvedEstudianteId.Value)
            : null;

        if (clase == null || estudiante == null)
            return NotFound();

        ViewBag.Clase = clase;
        ViewBag.Estudiante = estudiante;
        return View();
    }

    [Authorize(Roles = "Admin,Estudiante")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarExcusa(int claseId, int? estudianteId = null, string? descripcion = null)
    {
        var resolvedEstudianteId = estudianteId;
        if (resolvedEstudianteId == null || resolvedEstudianteId <= 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var estudiantePorUsuario = await _db.Estudiantes.FirstOrDefaultAsync(e => e.Correo == user.Email);
                if (estudiantePorUsuario != null)
                    resolvedEstudianteId = estudiantePorUsuario.Id;
            }
        }

        var clase = await _db.Clases.FindAsync(claseId);
        var estudiante = resolvedEstudianteId.HasValue
            ? await _db.Estudiantes.FindAsync(resolvedEstudianteId.Value)
            : null;

        if (clase == null || estudiante == null)
            return NotFound();

        var excusa = await _db.Excusas.FirstOrDefaultAsync(e => e.ClaseId == claseId && e.EstudianteId == estudiante.Id);
        if (excusa == null)
        {
            excusa = new Excusa
            {
                ClaseId = claseId,
                EstudianteId = estudiante.Id,
                Descripcion = descripcion,
                Estado = EstadoExcusa.Pendiente,
                FechaEnvio = DateTime.Now
            };
            _db.Excusas.Add(excusa);
        }
        else
        {
            excusa.Descripcion = descripcion;
            excusa.Estado = EstadoExcusa.Pendiente;
            excusa.FechaEnvio = DateTime.Now;
            _db.Excusas.Update(excusa);
        }

        await _db.SaveChangesAsync();

        TempData["Exito"] = "✅ Tu excusa fue enviada correctamente y está pendiente de revisión.";
        return RedirectToAction(nameof(Dashboard), new { id = estudiante.Id });
    }

    // ─────────────────────────────────────────────────────────
    // EXCUSAS: Listar y gestionar (panel admin simple)
    // GET /Clase/Excusas
    // ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excusas()
    {
        var excusas = await _db.Excusas
            .Include(e => e.Estudiante)
            .Include(e => e.Clase)
                .ThenInclude(c => c.Materia)
            .OrderByDescending(e => e.FechaEnvio)
            .ToListAsync();

        return View(excusas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CambiarEstadoExcusa(int excusaId, EstadoExcusa estado)
    {
        var excusa = await _db.Excusas.FindAsync(excusaId);
        if (excusa != null)
        {
            excusa.Estado = estado;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Excusas));
    }

    // ─────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────
    private async Task CargarSelectClases()
    {
        var clases = await _db.Clases
            .Include(c => c.Materia)
            .Select(c => new { c.Id, Texto = c.Materia.Nombre + " — " + c.Titulo })
            .ToListAsync();

        ViewBag.Clases = new SelectList(clases, "Id", "Texto");
    }

    private async Task NotificarN8nAsync(int claseId, string videoUrl)
    {
        try
        {
            var webhookUrl = _config["N8n:WebhookUrl"];
            if (string.IsNullOrWhiteSpace(webhookUrl) || webhookUrl.Contains("TU_WEBHOOK")) return;

            var clase = await _db.Clases
                .Include(c => c.Materia)
                    .ThenInclude(m => m.Inscripciones)
                        .ThenInclude(i => i.Estudiante)
                .FirstOrDefaultAsync(c => c.Id == claseId);

            if (clase == null) return;

            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            foreach (var ins in clase.Materia.Inscripciones)
            {
                var payload = new
                {
                    estudianteNombre = ins.Estudiante.Nombre,
                    estudianteCorreo = ins.Estudiante.Correo,
                    materia          = clase.Materia.Nombre,
                    claseId          = claseId,
                    claseTitulo      = clase.Titulo,
                    videoUrl         = videoUrl,
                    fechaNotificacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                await client.PostAsync(webhookUrl, content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[n8n webhook] Error: {ex.Message}");
        }
    }
}

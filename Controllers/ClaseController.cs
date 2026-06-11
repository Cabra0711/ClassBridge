using IUE.DesatrasadorMVP.Models;
using Microsoft.AspNetCore.Authorization;
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

    public ClaseController(AppDbContext db, IHttpClientFactory http, IConfiguration config)
    {
        _db     = db;
        _http   = http;
        _config = config;
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
    public async Task<IActionResult> Dashboard(int id = 1)
    {
        var estudiantes = await _db.Estudiantes.ToListAsync();
        ViewBag.Estudiantes = new SelectList(estudiantes, "Id", "Nombre", id);

        var estudiante = await _db.Estudiantes
            .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Materia)
                    .ThenInclude(m => m.Clases)
                        .ThenInclude(c => c.VideoClase)
            .Include(e => e.Excusas)
            .FirstOrDefaultAsync(e => e.Id == id);

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

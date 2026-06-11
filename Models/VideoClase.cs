using System.ComponentModel.DataAnnotations;

namespace IUE.DesatrasadorMVP.Models;

public class VideoClase
{
    public int Id { get; set; }

    [Required]
    public string VideoUrl { get; set; } = string.Empty;

    public DateTime SubidoEn { get; set; } = DateTime.Now;

    public int ClaseId { get; set; }
    public Clase Clase { get; set; } = null!;
}

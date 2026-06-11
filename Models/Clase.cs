using System.ComponentModel.DataAnnotations;

namespace IUE.DesatrasadorMVP.Models;

public class Clase
{
    public int Id { get; set; }

    [Required]
    public string Titulo { get; set; } = string.Empty;

    public string? Resumen { get; set; }

    public DateTime FechaClase { get; set; } = DateTime.Now;

    public int MateriaId { get; set; }
    public Materia Materia { get; set; } = null!;

    public VideoClase? VideoClase { get; set; }
    public ICollection<Excusa> Excusas { get; set; } = new List<Excusa>();
}

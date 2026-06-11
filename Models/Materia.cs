using System.ComponentModel.DataAnnotations;

namespace IUE.DesatrasadorMVP.Models;

public class Materia
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? ProfesorId { get; set; }
    public ApplicationUser? Profesor { get; set; }

    public ICollection<Clase> Clases { get; set; } = new List<Clase>();
    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
}

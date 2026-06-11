using System.ComponentModel.DataAnnotations;

namespace IUE.DesatrasadorMVP.Models;

public class Estudiante
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    public ICollection<Excusa> Excusas { get; set; } = new List<Excusa>();
}

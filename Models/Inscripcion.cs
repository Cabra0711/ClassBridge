namespace IUE.DesatrasadorMVP.Models;

public class Inscripcion
{
    public int Id { get; set; }
    public int EstudianteId { get; set; }
    public int MateriaId { get; set; }

    public Estudiante Estudiante { get; set; } = null!;
    public Materia Materia { get; set; } = null!;
}

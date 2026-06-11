namespace IUE.DesatrasadorMVP.Models;

public enum EstadoExcusa
{
    Pendiente,
    Aprobada,
    Rechazada
}

public class Excusa
{
    public int Id { get; set; }
    public string? Descripcion { get; set; }
    public EstadoExcusa Estado { get; set; } = EstadoExcusa.Pendiente;
    public DateTime FechaEnvio { get; set; } = DateTime.Now;

    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = null!;

    public int ClaseId { get; set; }
    public Clase Clase { get; set; } = null!;
}

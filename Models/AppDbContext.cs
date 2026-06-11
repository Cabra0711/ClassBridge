using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IUE.DesatrasadorMVP.Models;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
    public DbSet<Clase> Clases => Set<Clase>();
    public DbSet<VideoClase> VideoClases => Set<VideoClase>();
    public DbSet<Excusa> Excusas => Set<Excusa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Materia>()
            .HasOne(m => m.Profesor)
            .WithMany()
            .HasForeignKey(m => m.ProfesorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Evitar cascade delete conflicts
        modelBuilder.Entity<Excusa>()
            .HasOne(e => e.Clase)
            .WithMany(c => c.Excusas)
            .HasForeignKey(e => e.ClaseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Excusa>()
            .HasOne(e => e.Estudiante)
            .WithMany(es => es.Excusas)
            .HasForeignKey(e => e.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── SEED DATA ──────────────────────────────────────────────
        modelBuilder.Entity<Materia>().HasData(
            new Materia { Id = 1, Nombre = "Programación I",  Descripcion = "Fundamentos de programación" },
            new Materia { Id = 2, Nombre = "Bases de Datos",  Descripcion = "SQL y modelado relacional" },
            new Materia { Id = 3, Nombre = "Cálculo Diferencial", Descripcion = "Límites, derivadas e integrales" }
        );

        modelBuilder.Entity<Estudiante>().HasData(
            new Estudiante { Id = 1, Nombre = "Juan Pérez",   Correo = "juan.perez@iue.edu.co" },
            new Estudiante { Id = 2, Nombre = "María López",  Correo = "maria.lopez@iue.edu.co" }
        );

        modelBuilder.Entity<Inscripcion>().HasData(
            new Inscripcion { Id = 1, EstudianteId = 1, MateriaId = 1 },
            new Inscripcion { Id = 2, EstudianteId = 1, MateriaId = 2 },
            new Inscripcion { Id = 3, EstudianteId = 2, MateriaId = 1 },
            new Inscripcion { Id = 4, EstudianteId = 2, MateriaId = 3 }
        );

        modelBuilder.Entity<Clase>().HasData(
            new Clase { Id = 1, Titulo = "Introducción a C#",      MateriaId = 1, Resumen = "Variables, tipos y control de flujo.", FechaClase = new DateTime(2025, 5, 10) },
            new Clase { Id = 2, Titulo = "POO en C#",              MateriaId = 1, Resumen = "Clases, objetos y herencia.",           FechaClase = new DateTime(2025, 5, 17) },
            new Clase { Id = 3, Titulo = "Normalización",          MateriaId = 2, Resumen = "1FN, 2FN y 3FN explicadas.",            FechaClase = new DateTime(2025, 5, 12) },
            new Clase { Id = 4, Titulo = "Límites y continuidad",  MateriaId = 3, Resumen = "Definición epsilon-delta.",             FechaClase = new DateTime(2025, 5, 14) }
        );

        modelBuilder.Entity<VideoClase>().HasData(
            new VideoClase { Id = 1, ClaseId = 1, VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", SubidoEn = new DateTime(2025, 5, 10) }
        );

        modelBuilder.Entity<Excusa>().HasData(
            // Juan tiene excusa aprobada para clase 1 (puede ver video)
            new Excusa { Id = 1, EstudianteId = 1, ClaseId = 1, Estado = EstadoExcusa.Aprobada,  Descripcion = "Incapacidad médica", FechaEnvio = new DateTime(2025, 5, 11) },
            // Juan tiene excusa pendiente para clase 2 (no puede ver video aún)
            new Excusa { Id = 2, EstudianteId = 1, ClaseId = 2, Estado = EstadoExcusa.Pendiente, Descripcion = "Problema familiar",  FechaEnvio = new DateTime(2025, 5, 18) },
            // María tiene excusa aprobada para clase 3
            new Excusa { Id = 3, EstudianteId = 2, ClaseId = 3, Estado = EstadoExcusa.Aprobada,  Descripcion = "Viaje académico",    FechaEnvio = new DateTime(2025, 5, 13) }
        );
    }
}

using Microsoft.EntityFrameworkCore;
using EduAPI.Models;

namespace EduAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Leccion> Lecciones => Set<Leccion>();
        public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
        public DbSet<ProgresoLeccion> ProgresoLecciones => Set<ProgresoLeccion>();
        public DbSet<Visualizacion> Visualizaciones => Set<Visualizacion>();
        public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inscripcion>()
                .HasIndex(i => new { i.UsuarioId, i.CursoId }).IsUnique();
            modelBuilder.Entity<ProgresoLeccion>()
                .HasIndex(p => new { p.UsuarioId, p.LeccionId }).IsUnique();
            modelBuilder.Entity<Calificacion>()
                .HasIndex(c => new { c.UsuarioId, c.CursoId }).IsUnique();
        }
    }
}

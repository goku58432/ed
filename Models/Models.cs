using System.ComponentModel.DataAnnotations;
namespace EduAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required] public string Nombre { get; set; } = "";
        [Required] public string Correo { get; set; } = "";
        [Required] public string Contrasena { get; set; } = "";
        public string Rol { get; set; } = "usuario"; // "admin" | "profesor" | "usuario"
        public string? FotoUrl { get; set; }
        public string? Especialidad { get; set; } // solo para profesores
        public string Tema { get; set; } = "oscuro"; // "oscuro" | "claro"
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    }

    public class Curso
    {
        public int Id { get; set; }
        [Required] public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public int ProfesorId { get; set; }
        public Usuario? Profesor { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public ICollection<Leccion> Lecciones { get; set; } = new List<Leccion>();
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
    }

    public class Leccion
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        [Required] public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string VideoUrl { get; set; } = "";
        public int Orden { get; set; } = 0;
        public int DuracionSegundos { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public Curso? Curso { get; set; }
        public ICollection<Visualizacion> Visualizaciones { get; set; } = new List<Visualizacion>();
        public ICollection<ProgresoLeccion> Progresos { get; set; } = new List<ProgresoLeccion>();
    }

    public class Inscripcion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CursoId { get; set; }
        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;
        public bool Completado { get; set; } = false;
        public DateTime? FechaCompletado { get; set; }
        public string? CertificadoUrl { get; set; }
        public Usuario? Usuario { get; set; }
        public Curso? Curso { get; set; }
    }

    public class ProgresoLeccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int LeccionId { get; set; }
        public bool Completada { get; set; } = false;
        public DateTime? FechaCompletada { get; set; }
        public Usuario? Usuario { get; set; }
        public Leccion? Leccion { get; set; }
    }

    public class Visualizacion
    {
        public int Id { get; set; }
        public int LeccionId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaVista { get; set; } = DateTime.UtcNow;
        public Leccion? Leccion { get; set; }
        public Usuario? Usuario { get; set; }
    }

    public class Calificacion
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public int UsuarioId { get; set; }
        public int Estrellas { get; set; } = 5; // 1-5
        public string Comentario { get; set; } = "";
        public DateTime FechaCalificacion { get; set; } = DateTime.UtcNow;
        public Curso? Curso { get; set; }
        public Usuario? Usuario { get; set; }
    }
}

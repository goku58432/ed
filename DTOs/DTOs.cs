namespace EduAPI.DTOs
{
    // Auth
    public class RegisterDto { public string Nombre{get;set;}=""; public string Correo{get;set;}=""; public string Contrasena{get;set;}=""; }
    public class LoginDto { public string Correo{get;set;}=""; public string Contrasena{get;set;}=""; }
    public class AuthResponseDto { public string Token{get;set;}=""; public int Id{get;set;} public string Nombre{get;set;}=""; public string Correo{get;set;}=""; public string Rol{get;set;}=""; public string? FotoUrl{get;set;} public string Tema{get;set;}="oscuro"; }

    // Usuarios
    public class UsuarioDto { public int Id{get;set;} public string Nombre{get;set;}=""; public string Correo{get;set;}=""; public string Rol{get;set;}=""; public string? FotoUrl{get;set;} public string? Especialidad{get;set;} public string Tema{get;set;}="oscuro"; public DateTime FechaRegistro{get;set;} }
    public class ProfesorDto { public string Nombre{get;set;}=""; public string Correo{get;set;}=""; public string Especialidad{get;set;}=""; }
    public class ActualizarPerfilDto { public string? Nombre{get;set;} public string? ContrasenaActual{get;set;} public string? ContrasenaNueva{get;set;} public string? Tema{get;set;} }

    // Cursos
    public class CursoDto { public int Id{get;set;} public string Nombre{get;set;}=""; public string Descripcion{get;set;}=""; public string? ImagenUrl{get;set;} public int ProfesorId{get;set;} public string ProfesorNombre{get;set;}=""; public int TotalLecciones{get;set;} public double PromedioCalificacion{get;set;} public int TotalInscritos{get;set;} public DateTime FechaCreacion{get;set;} }
    public class CrearCursoDto { public string Nombre{get;set;}=""; public string Descripcion{get;set;}=""; public int ProfesorId{get;set;} }
    public class CursoDetalleDto : CursoDto { public List<LeccionDto> Lecciones{get;set;}=new(); public bool EstaInscrito{get;set;} public int LeccionActual{get;set;} }

    // Lecciones
    public class LeccionDto { public int Id{get;set;} public int CursoId{get;set;} public string Titulo{get;set;}=""; public string Descripcion{get;set;}=""; public string VideoUrl{get;set;}=""; public int Orden{get;set;} public int DuracionSegundos{get;set;} public bool Completada{get;set;} }
    public class CrearLeccionDto { public int CursoId{get;set;} public string Titulo{get;set;}=""; public string Descripcion{get;set;}=""; public int Orden{get;set;} }

    // Progreso
    public class ProgresoDto { public int CursoId{get;set;} public int TotalLecciones{get;set;} public int LeccionesCompletadas{get;set;} public bool Completado{get;set;} public string? CertificadoUrl{get;set;} }

    // Calificaciones
    public class CalificacionDto { public int Id{get;set;} public int CursoId{get;set;} public int UsuarioId{get;set;} public string UsuarioNombre{get;set;}=""; public string? UsuarioFoto{get;set;} public int Estrellas{get;set;} public string Comentario{get;set;}=""; public DateTime FechaCalificacion{get;set;} }
    public class CrearCalificacionDto { public int CursoId{get;set;} public int Estrellas{get;set;} public string Comentario{get;set;}=""; }

    // Reportes
    public class ReporteCursoDto { public int CursoId{get;set;} public string NombreCurso{get;set;}=""; public string ProfesorNombre{get;set;}=""; public int TotalInscritos{get;set;} public int TotalCompletados{get;set;} public double PromedioCalificacion{get;set;} public int TotalCalificaciones{get;set;} public long TotalVisualizaciones{get;set;} }
    public class VisualizacionDto { public int LeccionId{get;set;} public string TituloLeccion{get;set;}=""; public string NombreCurso{get;set;}=""; public long TotalVistas{get;set;} }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EduAPI.Data;
using EduAPI.DTOs;
using EduAPI.Models;
using EduAPI.Services;

namespace EduAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CursosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CloudinaryService _cloudinary;
        private readonly CertificadoService _certificado;
        private int MyId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private string MyRol => User.FindFirst(ClaimTypes.Role)!.Value;

        public CursosController(AppDbContext db, CloudinaryService cloudinary, CertificadoService certificado)
        { _db = db; _cloudinary = cloudinary; _certificado = certificado; }

        // ── Listar todos los cursos ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetCursos()
        {
            var cursos = await _db.Cursos
                .Include(c => c.Profesor)
                .Include(c => c.Lecciones)
                .Include(c => c.Inscripciones)
                .Include(c => c.Calificaciones)
                .Where(c => c.Activo)
                .Select(c => new CursoDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    ImagenUrl = c.ImagenUrl,
                    ProfesorId = c.ProfesorId,
                    ProfesorNombre = c.Profesor!.Nombre,
                    TotalLecciones = c.Lecciones.Count(l => l.Activo),
                    PromedioCalificacion = c.Calificaciones.Any() ? c.Calificaciones.Average(x => x.Estrellas) : 0,
                    TotalInscritos = c.Inscripciones.Count,
                    FechaCreacion = c.FechaCreacion
                }).ToListAsync();
            return Ok(cursos);
        }

        // ── Detalle de un curso ────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCurso(int id)
        {
            var c = await _db.Cursos
                .Include(x => x.Profesor)
                .Include(x => x.Lecciones.Where(l => l.Activo))
                .Include(x => x.Calificaciones)
                .Include(x => x.Inscripciones)
                .FirstOrDefaultAsync(x => x.Id == id && x.Activo);
            if (c == null) return NotFound();

            var inscripcion = await _db.Inscripciones.FirstOrDefaultAsync(i => i.UsuarioId == MyId && i.CursoId == id);
            var progresosIds = await _db.ProgresoLecciones
                .Where(p => p.UsuarioId == MyId && p.Completada)
                .Select(p => p.LeccionId).ToListAsync();

            return Ok(new CursoDetalleDto
            {
                Id = c.Id, Nombre = c.Nombre, Descripcion = c.Descripcion,
                ImagenUrl = c.ImagenUrl, ProfesorId = c.ProfesorId,
                ProfesorNombre = c.Profesor!.Nombre,
                TotalLecciones = c.Lecciones.Count,
                PromedioCalificacion = c.Calificaciones.Any() ? c.Calificaciones.Average(x => x.Estrellas) : 0,
                TotalInscritos = c.Inscripciones.Count,
                FechaCreacion = c.FechaCreacion,
                EstaInscrito = inscripcion != null,
                LeccionActual = progresosIds.Count,
                Lecciones = c.Lecciones.OrderBy(l => l.Orden).Select(l => new LeccionDto
                {
                    Id = l.Id, CursoId = l.CursoId, Titulo = l.Titulo,
                    Descripcion = l.Descripcion, VideoUrl = l.VideoUrl,
                    Orden = l.Orden, DuracionSegundos = l.DuracionSegundos,
                    Completada = progresosIds.Contains(l.Id)
                }).ToList()
            });
        }

        // ── Crear curso (admin) ────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CrearCurso([FromForm] CrearCursoDto dto, [FromForm] IFormFile? imagen)
        {
            string? imgUrl = null;
            if (imagen != null) imgUrl = await _cloudinary.SubirImagenAsync(imagen);

            var curso = new Curso
            {
                Nombre = dto.Nombre, Descripcion = dto.Descripcion,
                ProfesorId = dto.ProfesorId, ImagenUrl = imgUrl
            };
            _db.Cursos.Add(curso);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Curso creado", id = curso.Id });
        }

        // ── Agregar lección con video (admin) ─────────────────────────────────
        [HttpPost("lecciones")]
        [Authorize(Roles = "admin")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(524_288_000)] // 500 MB
        public async Task<IActionResult> AgregarLeccion([FromForm] CrearLeccionDto dto, [FromForm] IFormFile video)
        {
            var videoUrl = await _cloudinary.SubirVideoAsync(video);
            var leccion = new Leccion
            {
                CursoId = dto.CursoId, Titulo = dto.Titulo,
                Descripcion = dto.Descripcion, VideoUrl = videoUrl,
                Orden = dto.Orden
            };
            _db.Lecciones.Add(leccion);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Lección agregada", id = leccion.Id });
        }

        // ── Inscribirse a un curso ────────────────────────────────────────────
        [HttpPost("{id}/inscribirse")]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var yaInscrito = await _db.Inscripciones.AnyAsync(i => i.UsuarioId == MyId && i.CursoId == id);
            if (yaInscrito) return BadRequest(new { message = "Ya estás inscrito" });
            _db.Inscripciones.Add(new Inscripcion { UsuarioId = MyId, CursoId = id });
            await _db.SaveChangesAsync();
            return Ok(new { message = "Inscripción exitosa" });
        }

        // ── Marcar lección como completada ────────────────────────────────────
        [HttpPost("lecciones/{leccionId}/completar")]
        public async Task<IActionResult> CompletarLeccion(int leccionId)
        {
            var leccion = await _db.Lecciones.Include(l => l.Curso).FirstOrDefaultAsync(l => l.Id == leccionId);
            if (leccion == null) return NotFound();

            // Verificar que la lección anterior esté completada (no saltar lecciones)
            if (leccion.Orden > 0)
            {
                var anterior = await _db.Lecciones
                    .FirstOrDefaultAsync(l => l.CursoId == leccion.CursoId && l.Orden == leccion.Orden - 1);
                if (anterior != null)
                {
                    var anteriorCompletada = await _db.ProgresoLecciones
                        .AnyAsync(p => p.UsuarioId == MyId && p.LeccionId == anterior.Id && p.Completada);
                    if (!anteriorCompletada)
                        return BadRequest(new { message = "Debes completar la lección anterior primero" });
                }
            }

            var progreso = await _db.ProgresoLecciones
                .FirstOrDefaultAsync(p => p.UsuarioId == MyId && p.LeccionId == leccionId);
            if (progreso == null)
            {
                _db.ProgresoLecciones.Add(new ProgresoLeccion
                {
                    UsuarioId = MyId, LeccionId = leccionId,
                    Completada = true, FechaCompletada = DateTime.UtcNow
                });
            }
            else { progreso.Completada = true; progreso.FechaCompletada = DateTime.UtcNow; }

            // Registrar visualización
            _db.Visualizaciones.Add(new Visualizacion { LeccionId = leccionId, UsuarioId = MyId });

            await _db.SaveChangesAsync();

            // Verificar si completó todo el curso
            var totalLecciones = await _db.Lecciones.CountAsync(l => l.CursoId == leccion.CursoId && l.Activo);
            var completadas = await _db.ProgresoLecciones
                .CountAsync(p => p.UsuarioId == MyId && p.Completada &&
                    _db.Lecciones.Any(l => l.Id == p.LeccionId && l.CursoId == leccion.CursoId));

            bool cursosCompletado = completadas >= totalLecciones;
            string? certUrl = null;

            if (cursosCompletado)
            {
                var inscripcion = await _db.Inscripciones
                    .FirstOrDefaultAsync(i => i.UsuarioId == MyId && i.CursoId == leccion.CursoId);
                if (inscripcion != null && !inscripcion.Completado)
                {
                    inscripcion.Completado = true;
                    inscripcion.FechaCompletado = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Lección completada", cursoCompletado = cursosCompletado });
        }

        // ── Obtener progreso del curso ─────────────────────────────────────────
        [HttpGet("{id}/progreso")]
        public async Task<IActionResult> GetProgreso(int id)
        {
            var total = await _db.Lecciones.CountAsync(l => l.CursoId == id && l.Activo);
            var completadas = await _db.ProgresoLecciones
                .CountAsync(p => p.UsuarioId == MyId && p.Completada &&
                    _db.Lecciones.Any(l => l.Id == p.LeccionId && l.CursoId == id));
            var inscripcion = await _db.Inscripciones
                .FirstOrDefaultAsync(i => i.UsuarioId == MyId && i.CursoId == id);
            return Ok(new ProgresoDto
            {
                CursoId = id, TotalLecciones = total,
                LeccionesCompletadas = completadas,
                Completado = inscripcion?.Completado ?? false,
                CertificadoUrl = inscripcion?.CertificadoUrl
            });
        }

        // ── Descargar certificado PDF ──────────────────────────────────────────
        [HttpGet("{id}/certificado")]
        public async Task<IActionResult> DescargarCertificado(int id)
        {
            var inscripcion = await _db.Inscripciones
                .Include(i => i.Curso).ThenInclude(c => c!.Profesor)
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.UsuarioId == MyId && i.CursoId == id && i.Completado);
            if (inscripcion == null) return BadRequest(new { message = "No has completado este curso" });

            var pdf = _certificado.GenerarCertificado(
                inscripcion.Usuario!.Nombre,
                inscripcion.Curso!.Nombre,
                inscripcion.Curso.Profesor!.Nombre,
                inscripcion.FechaCompletado ?? DateTime.UtcNow
            );

            return File(pdf, "application/pdf", $"certificado_{id}.pdf");
        }

        // ── Calificar curso ────────────────────────────────────────────────────
        [HttpPost("calificaciones")]
        public async Task<IActionResult> Calificar([FromBody] CrearCalificacionDto dto)
        {
            var inscripcion = await _db.Inscripciones
                .FirstOrDefaultAsync(i => i.UsuarioId == MyId && i.CursoId == dto.CursoId && i.Completado);
            if (inscripcion == null)
                return BadRequest(new { message = "Debes completar el curso para calificarlo" });

            var yaCalifico = await _db.Calificaciones.AnyAsync(c => c.UsuarioId == MyId && c.CursoId == dto.CursoId);
            if (yaCalifico) return BadRequest(new { message = "Ya calificaste este curso" });

            if (dto.Estrellas < 1 || dto.Estrellas > 5) return BadRequest(new { message = "Calificación entre 1 y 5" });
            if (string.IsNullOrWhiteSpace(dto.Comentario)) return BadRequest(new { message = "El comentario es obligatorio" });

            _db.Calificaciones.Add(new Calificacion
            {
                CursoId = dto.CursoId, UsuarioId = MyId,
                Estrellas = dto.Estrellas, Comentario = dto.Comentario
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "Calificación enviada" });
        }

        // ── Calificaciones de un curso ─────────────────────────────────────────
        [HttpGet("{id}/calificaciones")]
        public async Task<IActionResult> GetCalificaciones(int id)
        {
            var cals = await _db.Calificaciones
                .Include(c => c.Usuario)
                .Where(c => c.CursoId == id)
                .OrderByDescending(c => c.FechaCalificacion)
                .Select(c => new CalificacionDto
                {
                    Id = c.Id, CursoId = c.CursoId, UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario!.Nombre, UsuarioFoto = c.Usuario.FotoUrl,
                    Estrellas = c.Estrellas, Comentario = c.Comentario,
                    FechaCalificacion = c.FechaCalificacion
                }).ToListAsync();
            return Ok(cals);
        }

        // ── Mis cursos inscritos ───────────────────────────────────────────────
        [HttpGet("mis-cursos")]
        public async Task<IActionResult> MisCursos()
        {
            var cursos = await _db.Inscripciones
                .Include(i => i.Curso).ThenInclude(c => c!.Profesor)
                .Include(i => i.Curso).ThenInclude(c => c!.Lecciones)
                .Where(i => i.UsuarioId == MyId)
                .Select(i => new
                {
                    curso = new CursoDto
                    {
                        Id = i.Curso!.Id, Nombre = i.Curso.Nombre,
                        Descripcion = i.Curso.Descripcion, ImagenUrl = i.Curso.ImagenUrl,
                        ProfesorId = i.Curso.ProfesorId, ProfesorNombre = i.Curso.Profesor!.Nombre,
                        TotalLecciones = i.Curso.Lecciones.Count(l => l.Activo),
                        FechaCreacion = i.Curso.FechaCreacion
                    },
                    completado = i.Completado,
                    fechaInscripcion = i.FechaInscripcion
                }).ToListAsync();
            return Ok(cursos);
        }
    }
}

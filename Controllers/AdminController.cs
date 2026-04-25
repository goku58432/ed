using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduAPI.Data;
using EduAPI.DTOs;
using EduAPI.Models;
using EduAPI.Services;

namespace EduAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CloudinaryService _cloudinary;

        public AdminController(AppDbContext db, CloudinaryService cloudinary)
        { _db = db; _cloudinary = cloudinary; }

        // ── Dar de alta profesor ───────────────────────────────────────────────
        [HttpPost("profesores")]
        public async Task<IActionResult> CrearProfesor([FromBody] ProfesorDto dto)
        {
            if (await _db.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
                return BadRequest(new { message = "El correo ya está registrado" });

            var profesor = new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Contrasena = BCrypt.Net.BCrypt.HashPassword("Temporal123!"),
                Rol = "profesor",
                Especialidad = dto.Especialidad
            };
            _db.Usuarios.Add(profesor);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Profesor creado", id = profesor.Id, contrasenaTemp = "Temporal123!" });
        }

        // ── Listar profesores ──────────────────────────────────────────────────
        [HttpGet("profesores")]
        public async Task<IActionResult> GetProfesores()
        {
            var profs = await _db.Usuarios
                .Where(u => u.Rol == "profesor" && u.Activo)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id, Nombre = u.Nombre, Correo = u.Correo,
                    Rol = u.Rol, FotoUrl = u.FotoUrl,
                    Especialidad = u.Especialidad, FechaRegistro = u.FechaRegistro
                }).ToListAsync();
            return Ok(profs);
        }

        // ── Reporte de cursos ──────────────────────────────────────────────────
        [HttpGet("reportes/cursos")]
        public async Task<IActionResult> ReporteCursos()
        {
            var reportes = await _db.Cursos
                .Include(c => c.Profesor)
                .Include(c => c.Inscripciones)
                .Include(c => c.Calificaciones)
                .Include(c => c.Lecciones).ThenInclude(l => l.Visualizaciones)
                .Where(c => c.Activo)
                .Select(c => new ReporteCursoDto
                {
                    CursoId = c.Id,
                    NombreCurso = c.Nombre,
                    ProfesorNombre = c.Profesor!.Nombre,
                    TotalInscritos = c.Inscripciones.Count,
                    TotalCompletados = c.Inscripciones.Count(i => i.Completado),
                    PromedioCalificacion = c.Calificaciones.Any() ? c.Calificaciones.Average(x => x.Estrellas) : 0,
                    TotalCalificaciones = c.Calificaciones.Count,
                    TotalVisualizaciones = c.Lecciones.SelectMany(l => l.Visualizaciones).Count()
                })
                .OrderByDescending(r => r.PromedioCalificacion)
                .ToListAsync();
            return Ok(reportes);
        }

        // ── Reporte de visualizaciones ────────────────────────────────────────
        [HttpGet("reportes/visualizaciones")]
        public async Task<IActionResult> ReporteVisualizaciones()
        {
            var vistas = await _db.Visualizaciones
                .Include(v => v.Leccion).ThenInclude(l => l!.Curso)
                .GroupBy(v => v.LeccionId)
                .Select(g => new VisualizacionDto
                {
                    LeccionId = g.Key,
                    TituloLeccion = g.First().Leccion!.Titulo,
                    NombreCurso = g.First().Leccion!.Curso!.Nombre,
                    TotalVistas = g.Count()
                })
                .OrderByDescending(v => v.TotalVistas)
                .ToListAsync();
            return Ok(vistas);
        }

        // ── Listar todos los usuarios ──────────────────────────────────────────
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _db.Usuarios
                .Where(u => u.Rol == "usuario")
                .Select(u => new UsuarioDto
                {
                    Id = u.Id, Nombre = u.Nombre, Correo = u.Correo,
                    Rol = u.Rol, FotoUrl = u.FotoUrl, FechaRegistro = u.FechaRegistro
                }).ToListAsync();
            return Ok(usuarios);
        }

        // ── Eliminar/desactivar curso ─────────────────────────────────────────
        [HttpDelete("cursos/{id}")]
        public async Task<IActionResult> EliminarCurso(int id)
        {
            var curso = await _db.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            curso.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Curso eliminado" });
        }
    }
}

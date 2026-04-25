using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EduAPI.Data;
using EduAPI.DTOs;
using EduAPI.Services;

namespace EduAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CloudinaryService _cloudinary;
        private int MyId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public UsuariosController(AppDbContext db, CloudinaryService cloudinary)
        { _db = db; _cloudinary = cloudinary; }

        [HttpGet("perfil")]
        public async Task<IActionResult> GetPerfil()
        {
            var u = await _db.Usuarios.FindAsync(MyId);
            if (u == null) return NotFound();
            return Ok(new UsuarioDto
            {
                Id = u.Id, Nombre = u.Nombre, Correo = u.Correo,
                Rol = u.Rol, FotoUrl = u.FotoUrl,
                Especialidad = u.Especialidad, Tema = u.Tema,
                FechaRegistro = u.FechaRegistro
            });
        }

        [HttpPut("perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto dto)
        {
            var u = await _db.Usuarios.FindAsync(MyId);
            if (u == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Nombre)) u.Nombre = dto.Nombre;
            if (!string.IsNullOrWhiteSpace(dto.Tema)) u.Tema = dto.Tema;

            if (!string.IsNullOrWhiteSpace(dto.ContrasenaNueva))
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.ContrasenaActual, u.Contrasena))
                    return BadRequest(new { message = "Contraseña actual incorrecta" });
                u.Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.ContrasenaNueva);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Perfil actualizado" });
        }

        [HttpPost("foto")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirFoto([FromForm] IFormFile foto)
        {
            var u = await _db.Usuarios.FindAsync(MyId);
            if (u == null) return NotFound();
            u.FotoUrl = await _cloudinary.SubirFotoPerfilAsync(foto);
            await _db.SaveChangesAsync();
            return Ok(new { fotoUrl = u.FotoUrl });
        }
    }
}

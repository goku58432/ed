using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduAPI.Data;
using EduAPI.DTOs;
using EduAPI.Models;

namespace EduAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        { _db = db; _config = config; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrWhiteSpace(dto.Contrasena))
                return BadRequest(new { message = "Todos los campos son requeridos" });
            if (await _db.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
                return BadRequest(new { message = "El correo ya está registrado" });

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
                Rol = "usuario"
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return Ok(BuildResponse(usuario));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == dto.Correo && u.Activo);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Contrasena, usuario.Contrasena))
                return Unauthorized(new { message = "Correo o contraseña incorrectos" });
            return Ok(BuildResponse(usuario));
        }

        private AuthResponseDto BuildResponse(Usuario u) => new()
        {
            Token = GenerarToken(u),
            Id = u.Id,
            Nombre = u.Nombre,
            Correo = u.Correo,
            Rol = u.Rol,
            FotoUrl = u.FotoUrl,
            Tema = u.Tema
        };

        private string GenerarToken(Usuario u)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new Claim(ClaimTypes.Name, u.Nombre),
                new Claim(ClaimTypes.Email, u.Correo),
                new Claim(ClaimTypes.Role, u.Rol)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"], _config["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

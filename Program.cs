using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EduAPI.Data;
using EduAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// MySQL
var mysqlHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
var mysqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT");
var mysqlDb   = Environment.GetEnvironmentVariable("MYSQL_DB");
var mysqlUser = Environment.GetEnvironmentVariable("MYSQL_USER");
var mysqlPass = Environment.GetEnvironmentVariable("MYSQL_PASS");
var connStr   = (mysqlHost != null)
    ? $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDb};User={mysqlUser};Password={mysqlPass};"
    : builder.Configuration.GetConnectionString("Default")!;

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddSingleton<CertificadoService>();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Límite de 500 MB para videos
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    o.MultipartBodyLengthLimit = 524_288_000);
builder.WebHost.ConfigureKestrel(o =>
    o.Limits.MaxRequestBodySize = 524_288_000);

builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.WithOrigins(
        "http://localhost:3000",
        "http://localhost:5173",
        "https://TU_SITIO_NETLIFY.netlify.app"  // ← cambiar después del deploy
    )
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        // Crear admin por defecto si no existe
        if (!db.Usuarios.Any(u => u.Rol == "admin"))
        {
            db.Usuarios.Add(new EduAPI.Models.Usuario
            {
                Nombre = "Administrador",
                Correo = "admin@eduplatform.com",
                Contrasena = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Rol = "admin"
            });
            db.SaveChanges();
            Console.WriteLine("✅ Admin creado: admin@eduplatform.com / Admin123!");
        }
        Console.WriteLine("✅ Base de datos lista");
    }
    catch (Exception ex) { Console.WriteLine($"❌ Error BD: {ex.Message}"); }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

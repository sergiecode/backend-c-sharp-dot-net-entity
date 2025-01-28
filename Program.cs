using BackendUsuarios.Data; // Importación del contexto de datos (base de datos)
using Microsoft.EntityFrameworkCore; // Librería para trabajar con Entity Framework Core
using Microsoft.AspNetCore.Authentication.JwtBearer; // Librería para la autenticación JWT
using Microsoft.IdentityModel.Tokens; // Librería para trabajar con validación de tokens JWT
using System.Text; // Librería para manejar la codificación de texto (usada para claves JWT)

var builder = WebApplication.CreateBuilder(args); // Construcción del objeto principal de la aplicación
DotNetEnv.Env.Load(); // Carga variables de entorno desde un archivo .env

// Configuración de la cadena de conexión (existente)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString == null)
{
    // Verificación para asegurarse de que la cadena de conexión está configurada
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Reemplazo de valores en la cadena de conexión con variables de entorno
connectionString = connectionString
    .Replace("${POSTGRES_HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST"))
    .Replace("${POSTGRES_PORT}", Environment.GetEnvironmentVariable("POSTGRES_PORT"))
    .Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB"))
    .Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER"))
    .Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"));

// Configuración del servicio DbContext con PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // Usa la cadena de conexión generada para conectar con la base de datos PostgreSQL

// Configuración del servicio de autenticación con JWT
builder.Services.AddAuthentication(options => {
    // Define el esquema de autenticación por defecto como JWT
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    // Configuración de validación del token JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Valida el emisor del token
        ValidateAudience = true, // Valida el público objetivo del token
        ValidateLifetime = true, // Verifica que el token no haya expirado
        ValidateIssuerSigningKey = true, // Valida la clave de firma del token
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Emisor válido (configurado en appsettings.json o variables de entorno)
        ValidAudience = builder.Configuration["Jwt:Audience"], // Público válido
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Clave de firma simétrica
    };
});

builder.Services.AddAuthorization(); // Servicio para manejar autorizaciones basadas en políticas

builder.Services.AddControllers(); // Registra los controladores para manejar solicitudes HTTP
builder.Services.AddEndpointsApiExplorer(); // Habilita la documentación de los endpoints

var app = builder.Build(); // Construye la aplicación con la configuración definida

// Middleware personalizado para manejar excepciones
app.UseMiddleware<BackendUsuarios.Middleware.ExceptionMiddleware>();

// Middleware pipeline (el orden es crucial para el correcto funcionamiento)

// Redirige automáticamente las solicitudes HTTP a HTTPS
app.UseHttpsRedirection();
app.UseRouting(); // Habilita el enrutamiento de solicitudes

// Middleware de autenticación y autorización
app.UseAuthentication(); // Debe estar antes de UseAuthorization para autenticar las solicitudes primero
app.UseAuthorization(); // Verifica las políticas de autorización definidas

// Mapeo de controladores para manejar las rutas
app.MapControllers();

// Logs de depuración (imprime las variables de conexión a PostgreSQL)
var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

Console.WriteLine($"Host: {postgresHost}, Port: {postgresPort}, DB: {postgresDb}, User: {postgresUser}");

// Ejecuta la aplicación
app.Run();

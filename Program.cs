using BackendUsuarios.Data;  // Importa el espacio de nombres donde se encuentra el contexto de datos (DbContext).
using Microsoft.EntityFrameworkCore;  // Importa Entity Framework Core, necesario para trabajar con bases de datos.

var builder = WebApplication.CreateBuilder(args);  // Crea el builder para la aplicación web.
DotNetEnv.Env.Load();  // Carga las variables de entorno desde un archivo .env, útil para no hardcodear configuraciones sensibles.


// Reemplaza las variables de entorno en la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");  // Obtiene la cadena de conexión desde la configuración (appsettings.json o variables de entorno).

if (connectionString == null)  // Si no se encuentra la cadena de conexión, lanza un error.
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Reemplaza las partes de la cadena de conexión con las variables de entorno
connectionString = connectionString
	.Replace("${POSTGRES_HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST"))  // Sustituye el host de PostgreSQL.
	.Replace("${POSTGRES_PORT}", Environment.GetEnvironmentVariable("POSTGRES_PORT"))  // Sustituye el puerto de PostgreSQL.
	.Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB"))  // Sustituye el nombre de la base de datos de PostgreSQL.
	.Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER"))  // Sustituye el usuario de PostgreSQL.
	.Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"));  // Sustituye la contraseña de PostgreSQL.

// Configura el DbContext para conectarse a PostgreSQL usando la cadena de conexión generada.
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(connectionString));  // Usa Npgsql para conectar a una base de datos PostgreSQL.

// Agrega soporte para controladores de API REST (para crear rutas que devuelvan datos en formato JSON, por ejemplo).
builder.Services.AddControllers();

// Configura OpenAPI (Swagger) para generar documentación automática de la API, si es necesario.
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger (comentada por si se necesita habilitar en algún momento).
// builder.Services.AddSwaggerGen();

var app = builder.Build();  // Construye la aplicación web a partir de la configuración.

// Usa un middleware personalizado para manejar excepciones globalmente en la aplicación.
app.UseMiddleware<BackendUsuarios.Middleware.ExceptionMiddleware>();


// Configura Swagger UI solo si el entorno es de desarrollo (para facilitar la prueba de la API en el navegador).
// if (app.Environment.IsDevelopment())
// {
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }

app.UseHttpsRedirection();  // Asegura que todas las solicitudes se redirijan a HTTPS (para mayor seguridad).
app.UseAuthorization();  // Habilita la autorización para las rutas que lo necesiten (protege la API con JWT, roles, etc.).

// Mapea las rutas de los controladores (esto expone las rutas de la API REST).
app.MapControllers();

// Obtiene las variables de entorno relacionadas con PostgreSQL para imprimirlas en la consola (útil para depuración).
var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

// Muestra las variables de entorno en la consola para asegurar que se hayan cargado correctamente.
Console.WriteLine($"Host: {postgresHost}, Port: {postgresPort}, DB: {postgresDb}, User: {postgresUser}");

// Inicia la aplicación y la pone en funcionamiento.
app.Run();

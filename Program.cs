using BackendUsuarios.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();


// Reemplaza las variables de entorno en la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString == null)
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

connectionString = connectionString
	.Replace("${POSTGRES_HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST"))
	.Replace("${POSTGRES_PORT}", Environment.GetEnvironmentVariable("POSTGRES_PORT"))
	.Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB"))
	.Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER"))
	.Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"));

// Configurar DbContext para PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(connectionString));

// Agregar soporte para controladores API (REST)
builder.Services.AddControllers();

// Configurar OpenAPI (Swagger) si es necesario
builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Usar middleware de excepciones personalizado
app.UseMiddleware<BackendUsuarios.Middleware.ExceptionMiddleware>();


// Usar Swagger UI si está en desarrollo
// if (app.Environment.IsDevelopment())
// {
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseAuthorization();

// Mapear controladores (con API REST)
app.MapControllers();

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

Console.WriteLine($"Host: {postgresHost}, Port: {postgresPort}, DB: {postgresDb}, User: {postgresUser}");

app.Run();

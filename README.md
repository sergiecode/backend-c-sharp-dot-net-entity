**Backend en C# con .NET y Entity Framework - Gestión de Usuarios**

Este repositorio contiene un backend desarrollado en C# utilizando el framework .NET y Entity Framework, diseñado para gestionar una tabla de usuarios. Proporciona las funcionalidades esenciales para la creación, lectura, actualización y eliminación (CRUD) de usuarios en una base de datos, facilitando la integración con aplicaciones frontend.

**Características:**

-   Implementación de Entity Framework para la interacción con la base de datos.
-   CRUD completo para la gestión de usuarios.
-   Configuración de la conexión a base de datos.
-   Diseño modular y escalable.



# Instalaciones necesarias para curso C# desde Cero 

 - [GOOGLE CHROME (NAVEGADOR)](https://www.google.com/intl/es_es/chrome/)
 - [VISUAL STUDIO CODE (Editor Código)](https://code.visualstudio.com/download)
 - [GIT (manejador de versiones)](https://git-scm.com/)
 - [C# SDK .NET 9](https://dotnet.microsoft.com/es-es/download/dotnet)
 - [POSTMAN](https://www.postman.com/downloads/)
 - [MONGO COMPAS](https://www.mongodb.com/try/download/compass)
 - [TABLE PLUS](https://tableplus.com/)
 - [DOCKER](https://www.docker.com/get-started/)

# Extensiones para VSCODE requeridas para perfil C#:
 - .NET Install Tool
 - C#
 - C# Curly Formatter
 - C# Dev Kit
 - C# Format Usings

## Pasos:

### Instalar SDK de NET y ASPNET core desde:
 - [C# SDK .NET 9](https://dotnet.microsoft.com/es-es/download/dotnet)
### **1. Crear el proyecto**

Abre una terminal y ejecuta:

```bash
dotnet new webapi -n BackendUsuarios
cd BackendUsuarios
```

Esto creará un proyecto ASP.NET Core llamado `BackendUsuarios`.

----------

### **2. Instalar los paquetes necesarios**

1.  **Instala el paquete de Entity Framework Core para PostgreSQL**:
    
```bash    
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL.Design
dotnet add package Newtonsoft.Json
dotnet add package DotNetEnv
```
    
2.  **Instala las herramientas de Entity Framework Core** (si no las tienes):
    
```bash
dotnet tool install --global dotnet-ef
```
    

----------

### **3. Configurar variables de entorno**

#### **3.1. Crear un archivo `.env`**

En el directorio raíz del proyecto, crea un archivo llamado `.env` y agrega las siguientes variables:

#### .dotenv
```dotenv
POSTGRES_USER=admin
POSTGRES_PASSWORD=admin123
POSTGRES_DB=usersdb
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
```

----------

### **4. Configurar `docker-compose.yml` para PostgreSQL**

Crea un archivo `docker-compose.yml` en el directorio raíz del proyecto con el siguiente contenido:


```yaml
version: "3.9"

services:
  postgres-db:
    image: postgres:latest
    container_name: postgres-db
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-admin}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-admin123}
      POSTGRES_DB: ${POSTGRES_DB:-usersdb}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres-data:
```

Levanta el contenedor ejecutando:


```bash
docker-compose up -d
```

----------

### **5. Configurar el proyecto ASP.NET Core**

#### **5.1. Agregar las variables de entorno en el proyecto**

Modifica el archivo `appsettings.json` para definir la conexión a la base de datos de manera genérica:


```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${POSTGRES_HOST};Port=${POSTGRES_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
  }
}
```

#### **5.2. Leer variables de entorno en `Program.cs`**

En el archivo `Program.cs`, ajusta el código para reemplazar las variables de entorno en la conexión:

```csharp
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
```

----------

### **6. Crear el modelo y `DbContext`**

#### **6.1. Crear el modelo de usuario**

En la carpeta raíz del proyecto, crea una carpeta llamada `Models` y dentro un archivo `User.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BackendUsuarios.Models;

public class User
{
	public int Id { get; set; }

	[Required(ErrorMessage = "The name is required.")]
	[MaxLength(50, ErrorMessage = "The name can't exceed 50 characters.")]
	public string Name { get; set; }

	[Required(ErrorMessage = "The email is required.")]
	[EmailAddress(ErrorMessage = "The email format is invalid.")]
	public string Email { get; set; }
	
	[Required(ErrorMessage = "The email is required.")]
	[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = "The password must have at least one lowercase letter, one uppercase letter, and one number.")]
	public string Password { get; set; }
}
```

#### **6.2. Crear el `DbContext`**

En la carpeta raíz del proyecto, crea una carpeta llamada `Data` y dentro un archivo `AppDbContext.cs`:

```csharp
using BackendUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}
```

----------

### **7. Configurar las migraciones**

1.  **Crear la primera migración**:
    
```bash
dotnet ef migrations add InitialCreate
```
    
2.  **Aplicar las migraciones**:
    
```bash
dotnet ef database update
```
    

----------

### **8. Crear un controlador de usuarios**

En la carpeta `Controllers`, crea un archivo `UsersController.cs`:

```csharp
using BackendUsuarios.Data;
using BackendUsuarios.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _context.Users.ToList();
        return Ok(users);
    }

    // GET: api/users/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetUserById(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }
        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Devuelve errores de validación automáticamente.
        }

        // Verificar si el correo electrónico ya existe
        var emailExists = _context.Users.Any(u => u.Email == user.Email);
        if (emailExists)
        {
            return BadRequest(new { Message = "The email is already in use." });
        }

        _context.Users.Add(user);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    // PUT: api/users/{id}
    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }

        // Verificar si el correo electrónico ya existe en otro usuario (no en el mismo usuario)
        var emailExists = _context.Users
                                  .Where(u => u.Id != id)  // Excluir al usuario actual
                                  .Any(u => u.Email == updatedUser.Email);
        if (emailExists)
        {
            return BadRequest(new { Message = "The email is already in use." });
        }

        // Actualizar los datos
        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
	user.Password = updatedUser.Password;

        _context.SaveChanges();

        return Ok(new { Message = $"User with ID {id} updated successfully.", User = user });
    }


    // DELETE: api/users/{id}
    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        return Ok(new { Message = $"User with ID {id} deleted successfully." });
    }
}
```

----------

2. Middleware para manejo global de errores
El middleware te permite capturar y manejar excepciones en toda la aplicación. Esto es útil para devolver respuestas personalizadas o logs si algo falla inesperadamente.

Crear una clase para el middleware de excepciones
Crea una nueva clase llamada ExceptionMiddleware.cs:

```csharp
using System.Net;
using Newtonsoft.Json;

namespace BackendUsuarios.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred. Please try again later.",
            Details = exception.Message // Puedes ocultar esto en producción.
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
```

## Comandos para ejecutar

## Chequear migraciones
- docker ps

## Averiguar:

- dotnet ef migrations list
- dotnet ef database update 0
- dotnet ef database update

## Hacer migración cuando cambia modelo o agregar otros modelos
- dotnet ef migrations add nombreNuevoMigracion
- dotnet ef database update

## Ejecución general:

- docker-compose up -d
- dotnet run

 
## CANAL DE YOUTUBE:
 - [CANAL SERGIE CODE](https://www.youtube.com/@sergiecode)

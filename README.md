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
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Authorization
dotnet add package BCrypt.Net-Next
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
  },
  "Jwt": {
    "Key": "TuClaveSuperSecretaDeAlMenos32Caracteres",
    "Issuer": "TuIssuer",
    "Audience": "TuAudience",
    "ExpireMinutes": 60
  }
}
```

#### **5.2. Leer variables de entorno en `Program.cs`**

En el archivo `Program.cs`, ajusta el código para reemplazar las variables de entorno en la conexión:

```csharp
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
```

----------

### **6. Crear el modelo y `DbContext`**

#### **6.1. Crear el modelo de usuario**

En la carpeta raíz del proyecto, crea una carpeta llamada `Models` y dentro un archivo `User.cs`:

```csharp
using System.ComponentModel.DataAnnotations;  // Importa las clases necesarias para realizar validaciones de datos, como Required, MaxLength, etc.

namespace BackendUsuarios.Models;  // Define el espacio de nombres para los modelos de datos.

public class User  // Declara la clase User que representa el modelo de usuario en la aplicación.
{
    public int Id { get; set; }  // Propiedad Id que representa el identificador único del usuario en la base de datos.

    // La propiedad Name, que es un campo de texto con validaciones de longitud y obligatoriedad.
    [Required(ErrorMessage = "The name is required.")]  // La validación asegura que el nombre sea obligatorio.
    [MaxLength(50, ErrorMessage = "The name can't exceed 50 characters.")]  // Limita la longitud del nombre a 50 caracteres.
    public string Name { get; set; }  // Propiedad Name que almacena el nombre del usuario.

    // La propiedad Email, que representa la dirección de correo electrónico del usuario con validaciones.
    [Required(ErrorMessage = "The email is required.")]  // La validación asegura que el correo electrónico sea obligatorio.
    [EmailAddress(ErrorMessage = "The email format is invalid.")]  // Valida que el correo tenga un formato correcto (ej. usuario@dominio.com).
    public string Email { get; set; }  // Propiedad Email que almacena el correo electrónico del usuario.
    
    // La propiedad Password, que representa la contraseña del usuario con validaciones.
    [Required(ErrorMessage = "The email is required.")]  // La validación asegura que la contraseña sea obligatoria.
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = "The password must have at least one lowercase letter, one uppercase letter, and one number.")]  // La contraseña debe tener al menos una letra minúscula, una mayúscula, un número y entre 6 y 50 caracteres.
    public string Password { get; set; }  // Propiedad Password que almacena la contraseña del usuario.
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

1.  **Crear la primera migración (el nombre puede ser cualquiera)**:
    
```bash
dotnet ef migrations add nombrePrimeraMigracion
```
    
2.  **Aplicar las migraciones**:
    
```bash
dotnet ef database update
```
    

----------

### **8. Crear un controlador de usuarios**

En la carpeta `Controllers`, crea un archivo `UsersController.cs`:

```csharp
using BackendUsuarios.Data; // Importa el espacio de nombres donde se encuentra el contexto de base de datos (AppDbContext).
using BackendUsuarios.Models; // Importa los modelos de datos (por ejemplo, User).
using Microsoft.AspNetCore.Mvc; // Proporciona clases para crear controladores y manejar solicitudes HTTP.
using Microsoft.AspNetCore.Authorization; // Habilita la autorización para proteger controladores o endpoints.
using System.Security.Claims; // Proporciona clases para manejar claims (información del usuario autenticado).
using Microsoft.IdentityModel.Tokens; // Permite trabajar con tokens de seguridad (e.g., claves de firma).
using System.IdentityModel.Tokens.Jwt; // Proporciona funcionalidad para crear y manejar tokens JWT.
using System.Text; // Permite trabajar con codificación de texto.
using BCrypt.Net; // Biblioteca para realizar el hash y verificación de contraseñas.

namespace BackendUsuarios.Controllers; // Define el espacio de nombres del controlador.

[ApiController] // Especifica que esta clase es un controlador API.
[Route("api/[controller]")] // Configura la ruta base para este controlador (e.g., "api/users").
[Authorize] // Requiere autenticación para todos los métodos del controlador, a menos que se anule.
public class UsersController : ControllerBase // Define un controlador base para manejar solicitudes HTTP.
{
	private readonly AppDbContext _context; // Inyección del contexto de base de datos para interactuar con la DB.
	private readonly IConfiguration _configuration; // Inyección de la configuración para acceder a las claves de appsettings.json.

	// Constructor del controlador que inicializa las dependencias inyectadas.
	public UsersController(AppDbContext context, IConfiguration configuration)
	{
		_context = context; // Inicializa el contexto de base de datos.
		_configuration = configuration; // Inicializa la configuración.
	}

	// Endpoint para iniciar sesión.
	[HttpPost("login")] // Define una ruta POST en "api/users/login".
	[AllowAnonymous] // Permite el acceso sin autenticación.
	public IActionResult Login([FromBody] LoginRequest request) // Recibe un objeto con las credenciales de login.
	{
		// Busca el usuario en la base de datos por su email.
		var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
		
		// Verifica si el usuario existe y si la contraseña proporcionada es válida.
		if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
		{
			return Unauthorized(new { Message = "Credenciales inválidas" }); // Retorna un error 401 si las credenciales son incorrectas.
		}

		// Crea un conjunto de claims para el usuario autenticado.
		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Agrega el ID del usuario como claim.
			new Claim(ClaimTypes.Email, user.Email) // Agrega el email del usuario como claim.
		};

		// Genera una clave simétrica a partir de la configuración (clave secreta para firmar el token).
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
		// Crea credenciales de firma para el token usando la clave.
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		// Genera el token JWT con los claims, la configuración y el tiempo de expiración.
		var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"], // Emisor del token.
			audience: _configuration["Jwt:Audience"], // Audiencia del token.
			claims: claims, // Claims del usuario.
			expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])), // Tiempo de expiración.
			signingCredentials: creds // Credenciales de firma.
		);

		// Retorna el token generado junto con información adicional (expiración e ID del usuario).
		return Ok(new {
			Token = new JwtSecurityTokenHandler().WriteToken(token), // Escribe el token como una cadena.
			Expiration = token.ValidTo, // Fecha de expiración del token.
			UserId = user.Id // ID del usuario autenticado.
		});
	}

	// Endpoint para obtener todos los usuarios.
	[HttpGet] // Define una ruta GET en "api/users".
	public IActionResult GetUsers()
	{
		// Obtiene la lista de usuarios desde la base de datos.
		var users = _context.Users.ToList();
		return Ok(users); // Retorna la lista en la respuesta HTTP.
	}

	// Endpoint para obtener un usuario por su ID.
	[HttpGet("{id:int}")] // Define una ruta GET con un parámetro de ID entero.
	public IActionResult GetUserById(int id)
	{
		// Busca al usuario en la base de datos por su ID.
		var user = _context.Users.FirstOrDefault(u => u.Id == id);
		// Retorna 404 si el usuario no existe; de lo contrario, retorna el usuario.
		return user == null ? NotFound(new { Message = $"User with ID {id} not found." }) : Ok(user);
	}

	// Endpoint para crear un nuevo usuario.
	[HttpPost] // Define una ruta POST en "api/users".
	[AllowAnonymous] // Permite el acceso sin autenticación.
	public IActionResult CreateUser([FromBody] User user) // Recibe un objeto con los datos del usuario a crear.
	{
		// Valida el modelo recibido; si no es válido, retorna un error 400.
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		// Verifica si el correo ya está en uso en la base de datos.
		if (_context.Users.Any(u => u.Email == user.Email))
		{
			return BadRequest(new { Message = "El correo ya está en uso" }); // Retorna un error 400 si ya existe.
		}

		// Hashea la contraseña del usuario antes de guardarla.
		user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
		
		// Agrega el usuario a la base de datos y guarda los cambios.
		_context.Users.Add(user);
		_context.SaveChanges();

		// Retorna un 201 con la información del usuario creado.
		return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
	}

	// Endpoint para actualizar un usuario existente.
	[HttpPut("{id:int}")] // Define una ruta PUT con un parámetro de ID entero.
	public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
	{
		// Valida el modelo recibido; si no es válido, retorna un error 400.
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		// Busca al usuario en la base de datos por su ID.
		var user = _context.Users.FirstOrDefault(u => u.Id == id);
		if (user == null)
		{
			return NotFound(new { Message = $"Usuario no encontrado" }); // Retorna 404 si el usuario no existe.
		}

		// Verifica si el correo ya está en uso por otro usuario.
		if (_context.Users.Any(u => u.Id != id && u.Email == updatedUser.Email))
		{
			return BadRequest(new { Message = "El correo ya está en uso" }); // Retorna un error 400 si ya existe.
		}

		// Actualiza la contraseña si es diferente de la existente.
		if (!BCrypt.Net.BCrypt.Verify(updatedUser.Password, user.Password))
		{
			user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
		}

		// Actualiza los datos del usuario.
		user.Name = updatedUser.Name;
		user.Email = updatedUser.Email;

		// Guarda los cambios en la base de datos.
		_context.SaveChanges();
		return Ok(new { Message = $"Usuario actualizado", User = user }); // Retorna el usuario actualizado.
	}

	// Endpoint para eliminar un usuario por su ID.
	[HttpDelete("{id:int}")] // Define una ruta DELETE con un parámetro de ID entero.
	public IActionResult DeleteUser(int id)
	{
		// Busca al usuario en la base de datos por su ID.
		var user = _context.Users.FirstOrDefault(u => u.Id == id);
		if (user == null)
		{
			return NotFound(new { Message = $"User with ID {id} not found." }); // Retorna 404 si no existe.
		}

		// Elimina al usuario de la base de datos y guarda los cambios.
		_context.Users.Remove(user);
		_context.SaveChanges();
		return Ok(new { Message = $"User with ID {id} deleted successfully." }); // Retorna un mensaje de éxito.
	}

	// Clase DTO (Data Transfer Object) para recibir los datos de login.
	public class LoginRequest
	{
		public string Email { get; set; } // Email del usuario.
		public string Password { get; set; } // Contraseña del usuario.
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


### Flujo Típico de Trabajo con Docker y DotNet

**Ejemplo práctico cuando te bajás el proyecto**:

1.  `docker-compose up -d`  → Levantas servicios (DB)
    
2.  `dotnet restore`  → Restauras dependencias
    
3.  `dotnet ef migrations list`  → Verificas migraciones existentes
    
4.  `dotnet ef database update 0`  → (Opcional) Reseteas DB si es necesario
    
5.  `dotnet ef database update`  → Aplicas todas las migraciones
    
6.  `dotnet run`  → Inicias la aplicación
    

**Si modificas modelos / o recién iniciás**:

1.  Haces cambios en tus clases de entidad
    
2.  `dotnet ef migrations add MiCambio`
    
3.  `dotnet ef database update`
    
4.  `dotnet run`



 
## CANAL DE YOUTUBE:
 - [CANAL SERGIE CODE](https://www.youtube.com/@sergiecode)

using BackendUsuarios.Data; // Importa el espacio de nombres donde se encuentra el contexto de base de datos (AppDbContext).
using BackendUsuarios.Models; // Importa los modelos de datos (por ejemplo, User).
using Microsoft.AspNetCore.Mvc; // Proporciona clases para crear controladores y manejar solicitudes HTTP.
using Microsoft.AspNetCore.Authorization; // Habilita la autorización para proteger controladores o endpoints.
using System.Security.Claims; // Proporciona clases para manejar claims (información del usuario autenticado).
using Microsoft.IdentityModel.Tokens; // Permite trabajar con tokens de seguridad (e.g., claves de firma).
using System.IdentityModel.Tokens.Jwt; // Proporciona funcionalidad para crear y manejar tokens JWT.
using System.Text; // Permite trabajar con codificación de texto.
using BCrypt.Net;
using Microsoft.EntityFrameworkCore; // Biblioteca para realizar el hash y verificación de contraseñas.

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
		var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Email == request.Email);

		if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
		{
			return Unauthorized(new { Message = "Credenciales inválidas" });
		}

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Agrega el ID del usuario como claim.
			new Claim(ClaimTypes.Email, user.Email), // Agrega el email del usuario como claim.
			new Claim(ClaimTypes.Role, user.Role?.Name ?? "User") // Agrega el rol al token
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
		return Ok(new
		{
			Token = new JwtSecurityTokenHandler().WriteToken(token), // Escribe el token como una cadena.
			Expiration = token.ValidTo, // Fecha de expiración del token.
			UserId = user.Id, // ID del usuario autenticado.
			Role = user.Role?.Name // Retorna el rol
		});
	}


	// Endpoint para obtener todos los usuarios.
	[HttpGet]// Define una ruta GET en "api/users".
	public IActionResult GetUsers()
	{
		// Obtiene la lista de usuarios desde la base de datos.
		var users = _context.Users
			.Include(u => u.Role) // Incluye el rol en la respuesta
			.Select(u => new
			{
				u.Id,
				u.Name,
				u.Email,
				Role = u.Role != null ? u.Role.Name : "User" // Muestra el nombre del rol
			})
			.ToList();

		return Ok(users);
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
	[HttpPost]
	[AllowAnonymous]
	public IActionResult CreateUser([FromBody] User user)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		if (_context.Users.Any(u => u.Email == user.Email))
		{
			return BadRequest(new { Message = "El correo ya está en uso" });
		}

		// Verifica que el RoleId sea válido
		if (!_context.Roles.Any(r => r.Id == user.RoleId))
		{
			return BadRequest(new { Message = "El rol especificado no existe" });
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

	[HttpGet("admin")]
	[Authorize(Roles = "Admin")]
	public IActionResult GetAdminData()
	{
		return Ok(new { Message = "Solo los administradores pueden ver esto." });
	}

	// Clase DTO (Data Transfer Object) para recibir los datos de login.
	public class LoginRequest
	{
		public string Email { get; set; } = string.Empty; // Email del usuario.
		public string Password { get; set; } = string.Empty; // Contraseña del usuario.
	}
}

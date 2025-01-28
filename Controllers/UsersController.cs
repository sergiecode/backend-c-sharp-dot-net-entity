using BackendUsuarios.Data;
using BackendUsuarios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims; 
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Text; 
using BCrypt.Net;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todo el controlador por defecto
public class UsersController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration; // Nueva inyección

	// Constructor actualizado
	public UsersController(AppDbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	// Endpoint de Login (nuevo)
[HttpPost("login")]
	[AllowAnonymous]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
		
		if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
		{
			return Unauthorized(new { Message = "Credenciales inválidas" });
		}

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email)
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
			signingCredentials: creds
		);

		return Ok(new {
			Token = new JwtSecurityTokenHandler().WriteToken(token),
			Expiration = token.ValidTo,
			UserId = user.Id
		});
	}

	// Métodos existentes con modificaciones de seguridad

	[HttpGet]
	public IActionResult GetUsers()
	{
		var users = _context.Users.ToList();
		return Ok(users);
	}

	[HttpGet("{id:int}")]
	public IActionResult GetUserById(int id)
	{
		var user = _context.Users.FirstOrDefault(u => u.Id == id);
		return user == null ? NotFound(new { Message = $"User with ID {id} not found." }) : Ok(user);
	}

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

		// Hash de la contraseña
		user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
		
		_context.Users.Add(user);
		_context.SaveChanges();

		return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
	}

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
			return NotFound(new { Message = $"Usuario no encontrado" });
		}

		if (_context.Users.Any(u => u.Id != id && u.Email == updatedUser.Email))
		{
			return BadRequest(new { Message = "El correo ya está en uso" });
		}

		// Actualizar contraseña con hash si es necesario
		if (!BCrypt.Net.BCrypt.Verify(updatedUser.Password, user.Password))
		{
			user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
		}

		user.Name = updatedUser.Name;
		user.Email = updatedUser.Email;

		_context.SaveChanges();
		return Ok(new { Message = $"Usuario actualizado", User = user });
	}


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

	// Clase DTO para el login
	public class LoginRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
using BackendUsuarios.Data;
using BackendUsuarios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Nuevo using
using System.Security.Claims; // Nuevo using
using Microsoft.IdentityModel.Tokens; // Nuevo using
using System.IdentityModel.Tokens.Jwt; // Nuevo using
using System.Text; // Nuevo using

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
    [AllowAnonymous] // Excepción a la autenticación
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);
        
        if (user == null)
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
    [AllowAnonymous] // Permitir registro sin autenticación
    public IActionResult CreateUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (_context.Users.Any(u => u.Email == user.Email))
        {
            return BadRequest(new { Message = "The email is already in use." });
        }

        // ¡IMPORTANTE! En producción deberías hashear la contraseña aquí
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
            return NotFound(new { Message = $"User with ID {id} not found." });
        }

        if (_context.Users.Any(u => u.Id != id && u.Email == updatedUser.Email))
        {
            return BadRequest(new { Message = "The email is already in use." });
        }

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        user.Password = updatedUser.Password; // ¡Deberías actualizar el hash si usas hashing!

        _context.SaveChanges();
        return Ok(new { Message = $"User with ID {id} updated successfully.", User = user });
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
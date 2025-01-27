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
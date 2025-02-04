using BackendUsuarios.Data;
using BackendUsuarios.Models.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using BackendUsuarios.Models.Users;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;

	public UsersController(AppDbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	/// <summary>
	/// Authenticates a user and generates a JWT token.
	/// </summary>
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		try
		{
			var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);

			if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
			{
				return Unauthorized(new { Message = "Invalid credentials" });
			}

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
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

			return Ok(new
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiration = token.ValidTo,
				UserId = user.Id,
				Role = user.Role?.Name
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "An internal error occurred", Error = ex.Message });
		}
	}

	/// <summary>
	/// Retrieves all users.
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetUsers()
	{
		try
		{
			var users = await _context.Users
				.Include(u => u.Role)
				.Select(u => new
				{
					u.Id,
					u.Name,
					u.Email,
					Role = u.Role != null ? u.Role.Name : "User"
				})
				.ToListAsync();

			return Ok(users);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "Error retrieving users", Error = ex.Message });
		}
	}

	/// <summary>
	/// Retrieves a user by ID.
	/// </summary>
	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetUserById(Guid id)
	{
		try
		{
			var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
			if (user == null)
			{
				return NotFound(new { Message = $"User with ID {id} not found." });
			}

			var response = new UserResponseDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				Role = user.Role?.Name ?? "User"
			};

			return Ok(response);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "Error retrieving user", Error = ex.Message });
		}
	}


	/// <summary>
	/// Creates a new user.
	/// </summary>
	[HttpPost]
	[AllowAnonymous]
	public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
	{
		try
		{
			if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
			{
				return BadRequest(new { Message = "The email is already in use." });
			}

			var user = new User
			{
				Id = Guid.NewGuid(),
				Name = userDto.Name,
				Email = userDto.Email,
				Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
				RoleId = userDto.RoleId
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			var response = new UserResponseDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				Role = (await _context.Roles.FindAsync(user.RoleId))?.Name ?? "User"
			};

			return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "An error occurred while creating the user.", Error = ex.Message });
		}
	}


	/// <summary>
	/// Updates an existing user.
	/// </summary>
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto updatedUserDto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound(new { Message = "User not found" });
			}

			if (await _context.Users.AnyAsync(u => u.Id != id && u.Email == updatedUserDto.Email))
			{
				return BadRequest(new { Message = "Email is already in use" });
			}

			user.Name = updatedUserDto.Name;
			user.Email = updatedUserDto.Email;

			if (!string.IsNullOrEmpty(updatedUserDto.Password))
			{
				user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUserDto.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
			}

			await _context.SaveChangesAsync();

			var response = new UserResponseDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				Role = (await _context.Roles.FindAsync(user.RoleId))?.Name ?? "User"
			};

			return Ok(new { Message = "User updated successfully", User = response });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "Error updating user", Error = ex.Message });
		}
	}


	/// <summary>
	/// Deletes a user by ID.
	/// </summary>
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> DeleteUser(Guid id)
	{
		try
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound(new { Message = $"User with ID {id} not found." });
			}

			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
			return Ok(new { Message = $"User with ID {id} successfully deleted." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "Error deleting user", Error = ex.Message });
		}
	}

	/// <summary>
	/// Returns data accessible only to admins.
	/// </summary>
	[HttpGet("admin")]
	[Authorize(Roles = "Admin")]
	public IActionResult GetAdminData()
	{
		return Ok(new { Message = "Only administrators can view this." });
	}

	public class LoginRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
}

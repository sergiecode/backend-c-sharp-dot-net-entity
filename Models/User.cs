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
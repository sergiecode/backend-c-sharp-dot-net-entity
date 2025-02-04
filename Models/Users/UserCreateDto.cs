using System.ComponentModel.DataAnnotations;

namespace BackendUsuarios.Models.Users
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "The name is required.")]
        [MaxLength(50, ErrorMessage = "The name can't exceed 50 characters.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "The email is required.")]
        [EmailAddress(ErrorMessage = "The email format is invalid.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "The password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$",
            ErrorMessage = "The password must have at least one lowercase letter, one uppercase letter, and one number.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "The role ID is required.")]
        public Guid RoleId { get; set; }
    }
}

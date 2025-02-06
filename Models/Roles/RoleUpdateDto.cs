using System.ComponentModel.DataAnnotations;

namespace BackendUsuarios.Models.Roles
{
    public class RoleUpdateDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public required string Name { get; set; }
    }
}
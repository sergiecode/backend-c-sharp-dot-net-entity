using System.ComponentModel.DataAnnotations;

namespace BackendUsuarios.Models.Roles
{
    public class RoleCreateDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public required string Name { get; set; }
    }
}
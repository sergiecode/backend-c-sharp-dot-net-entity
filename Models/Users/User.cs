using System.ComponentModel.DataAnnotations;
using BackendUsuarios.Models.Roles;

namespace BackendUsuarios.Models.Users;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}

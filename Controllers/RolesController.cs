using BackendUsuarios.Data;
using BackendUsuarios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Solo los administradores pueden gestionar roles
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // Obtener todos los roles
        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _context.Roles.ToList();
            return Ok(roles);
        }

        // Obtener un rol por ID
        [HttpGet("{id:guid}")]
        public IActionResult GetRoleById(Guid id)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
            {
                return NotFound(new { Message = "Rol no encontrado." });
            }
            return Ok(role);
        }

        // Crear un nuevo rol
        [HttpPost]
        public IActionResult CreateRole([FromBody] Role role)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                return BadRequest(new { Message = "El nombre del rol es requerido." });
            }

            if (_context.Roles.Any(r => r.Name == role.Name))
            {
                return BadRequest(new { Message = "El rol ya existe." });
            }

            role.Id = Guid.NewGuid(); // Genera un nuevo ID para el rol
            _context.Roles.Add(role);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        // Actualizar un rol existente
        [HttpPut("{id:guid}")]
        public IActionResult UpdateRole(Guid id, [FromBody] Role updatedRole)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
            {
                return NotFound(new { Message = "Rol no encontrado." });
            }

            if (string.IsNullOrWhiteSpace(updatedRole.Name))
            {
                return BadRequest(new { Message = "El nombre del rol es requerido." });
            }

            role.Name = updatedRole.Name;
            _context.SaveChanges();
            return Ok(new { Message = "Rol actualizado exitosamente.", Role = role });
        }

        // Eliminar un rol
        [HttpDelete("{id:guid}")]
        public IActionResult DeleteRole(Guid id)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
            {
                return NotFound(new { Message = "Rol no encontrado." });
            }

            _context.Roles.Remove(role);
            _context.SaveChanges();
            return Ok(new { Message = "Rol eliminado exitosamente." });
        }
    }
}

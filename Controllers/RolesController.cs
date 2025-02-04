// RolesController.cs
using BackendUsuarios.Data;
using BackendUsuarios.Models.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto roleDto)
        {
            if (await _context.Roles.AnyAsync(r => r.Name == roleDto.Name))
                return BadRequest(new { Message = "The role already exists." });

            var role = new Role { Id = Guid.NewGuid(), Name = roleDto.Name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleUpdateDto updatedRoleDto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            if (await _context.Roles.AnyAsync(r => r.Name == updatedRoleDto.Name && r.Id != id))
                return BadRequest(new { Message = "The role name is already in use." });

            role.Name = updatedRoleDto.Name;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Role updated successfully.", Role = role });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Role deleted successfully." });
        }
    }
}
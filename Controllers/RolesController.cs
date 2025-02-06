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
    public class RolesController : BaseController<Role, RoleCreateDto, RoleUpdateDto>
    {
        private readonly IConfiguration _configuration;

        public RolesController(AppDbContext context, IConfiguration configuration)
            : base(context)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto roleDto)
        {
            try
            {
                // Check if the role already exists
                if (await _context.Roles.AnyAsync(r => r.Name == roleDto.Name))
                {
                    return BadRequest(new { Message = "The role already exists." });
                }

                // Create the new role
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleDto.Name
                };

                // Add the role to the context and save changes
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                // Prepare the response
                var response = new
                {
                    role.Id,
                    role.Name
                };

                // Return the created role
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "An error occurred while creating the role.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleUpdateDto updatedRoleDto)
        {
            try
            {
                // Find the role by ID
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new { Message = "Role not found." });
                }

                // Check if the role name is already in use
                if (await _context.Roles.AnyAsync(r => r.Name == updatedRoleDto.Name && r.Id != id))
                {
                    return BadRequest(new { Message = "The role name is already in use." });
                }

                // Update the role name
                role.Name = updatedRoleDto.Name;

                // Save changes
                await _context.SaveChangesAsync();

                // Prepare the response
                var response = new
                {
                    role.Id,
                    role.Name
                };

                // Return success message and updated role
                return Ok(new { Message = "Role updated successfully.", Role = response });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error updating role.");
            }
        }

    }
}
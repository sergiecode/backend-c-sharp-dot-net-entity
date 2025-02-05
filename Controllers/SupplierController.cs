using BackendUsuarios.Data;
using BackendUsuarios.Models.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public SupplierController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Retrieves all suppliers.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
        try
        {
            var suppliers = await _context.Suppliers.ToListAsync();

            var response = suppliers.Select(s => new
            {
                s.Id,
                s.Name,
                s.Phone,
                s.Email,
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving suppliers", Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a supplier by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplierById(Guid id)
    {
        try
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound(new { Message = $"Supplier with ID {id} not found." });
            }

            var response = new
            {
                supplier.Id,
                supplier.Name,
                supplier.Phone,
                supplier.Email,
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving supplier", Error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierCreateDto supplierDto)
    {
        try
        {
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = supplierDto.Name,
                Phone = supplierDto.Phone,
                Email = supplierDto.Email
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            var response = new
            {
                supplier.Id,
                supplier.Name,
                supplier.Phone,
                supplier.Email
            };

            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error creating supplier", Error = ex.Message });
        }
    }


    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] SupplierUpdateDto updatedSupplierDto)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(new { Message = "Supplier not found" });
            }

            supplier.Name = updatedSupplierDto.Name;
            supplier.Phone = updatedSupplierDto.Phone;
            supplier.Email = updatedSupplierDto.Email;

            await _context.SaveChangesAsync();

            var response = new
            {
                supplier.Id,
                supplier.Name,
                supplier.Phone,
                supplier.Email
            };

            return Ok(new { Message = "Supplier updated successfully", Supplier = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error updating supplier", Error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a supplier by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound(new { Message = $"Supplier with ID {id} not found." });
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Supplier with ID {id} successfully deleted." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error deleting supplier", Error = ex.Message });
        }
    }



}
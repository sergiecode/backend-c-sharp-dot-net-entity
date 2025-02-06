using BackendUsuarios.Data;
using BackendUsuarios.Models.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : BaseController<Supplier, SupplierCreateDto, SupplierUpdateDto>
{
    public SuppliersController(AppDbContext context, IConfiguration configuration) : base(context)
    {
    }


    [HttpPost]
    public async Task<IActionResult> CreateSupplier(SupplierCreateDto supplierDto)
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

            _context.Set<Supplier>().Add(supplier);
            await _context.SaveChangesAsync();

            var response = new
            {
                supplier.Id,
                supplier.Name,
                supplier.Phone,
                supplier.Email
            };

            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating supplier");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, SupplierUpdateDto updatedSupplierDto)
    {
        try
        {
            var supplier = await _context.Set<Supplier>().FindAsync(id);
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
            return HandleException(ex, "Error updating supplier");
        }
    }

}

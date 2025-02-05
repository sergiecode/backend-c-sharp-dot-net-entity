using BackendUsuarios.Data;
using BackendUsuarios.Models.ProductTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductTypeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public ProductTypeController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Retrieves all product types.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProductTypes()
    {
        try
        {
            var productTypes = await _context.ProductTypes.ToListAsync();

            var response = productTypes.Select(pt => new
            {
                pt.Id,
                pt.Name,
                pt.Description,
                pt.isActive,
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving product types", Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a product type by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductTypeById(Guid id)
    {
        try
        {
            var productType = await _context.ProductTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (productType == null)
            {
                return NotFound(new { Message = $"Product type with ID {id} not found." });
            }

            var response = new
            {
                productType.Id,
                productType.Name,
                productType.Description,
                productType.isActive,
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving product type", Error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new product type.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProductType([FromBody] ProductTypeCreateDto productTypeDto)
    {
        try
        {
            var productType = new ProductType
            {
                Id = Guid.NewGuid(),
                Name = productTypeDto.Name,
                Description = productTypeDto.Description,
                isActive = productTypeDto.isActive,
            };

            _context.ProductTypes.Add(productType);
            await _context.SaveChangesAsync();

            var response = new
            {
                productType.Id,
                productType.Name,
                productType.Description,
                productType.isActive,
            };

            return CreatedAtAction(nameof(GetProductTypeById), new { id = productType.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error creating product type", Error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing product type.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProductType(Guid id, [FromBody] ProductTypeUpdateDto updatedProductTypeDto)
    {
        try
        {
            var productType = await _context.ProductTypes.FindAsync(id);
            if (productType == null)
            {
                return NotFound(new { Message = "Product type not found" });
            }

            productType.Name = updatedProductTypeDto.Name;
            productType.Description = updatedProductTypeDto.Description;
            productType.isActive = updatedProductTypeDto.isActive;

            await _context.SaveChangesAsync();

            var response = new
            {
                productType.Id,
                productType.Name,
                productType.Description,
                productType.isActive,
            };

            return Ok(new { Message = "Product type updated successfully", ProductType = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error updating product type", Error = ex.Message });
        }
    }


    /// <summary>
    /// Deletes a product type by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProductType(Guid id)
    {
        try
        {
            var productType = await _context.ProductTypes.FindAsync(id);
            if (productType == null)
            {
                return NotFound(new { Message = $"Product type with ID {id} not found." });
            }

            _context.ProductTypes.Remove(productType);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Product type with ID {id} successfully deleted." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error deleting product type", Error = ex.Message });
        }
    }




}
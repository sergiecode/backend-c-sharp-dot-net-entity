using BackendUsuarios.Data;
using BackendUsuarios.Models.ProductTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductTypesController : BaseController<ProductType, ProductTypeCreateDto, ProductTypeUpdateDto>
{
    private readonly IConfiguration _configuration;

    public ProductTypesController(AppDbContext context, IConfiguration configuration)
        : base(context) // Llamada al constructor de la clase base
    {
        _configuration = configuration;
    }

    // Si necesitas personalizar algún método heredado, como GetAll o GetById,
    // puedes sobrescribirlo aquí.

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

            return CreatedAtAction(nameof(GetById), new { id = productType.Id }, response); // Usar GetById heredado
        }
        catch (Exception ex)
        {
            return HandleException(ex, "An error occurred while creating the product type.");
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
            return HandleException(ex, "Error updating product type");
        }
    }
}

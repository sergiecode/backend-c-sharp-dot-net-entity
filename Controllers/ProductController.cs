using BackendUsuarios.Data;
using BackendUsuarios.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public ProductController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductType)
                .Include(p => p.Stock)
                .Include(p => p.Supplier)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    StockQuantity = p.Stock != null ? p.Stock.Quantity : 0,
                    Category = p.Category != null ? p.Category.Name : "No Category",
                    ProductType = p.ProductType != null ? p.ProductType.Name : "No Type",
                    Supplier = p.Supplier != null ? p.Supplier.Name : "No Supplier",
                    p.CreationDate
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving products", Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductType)
                .Include(p => p.Stock)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            var response = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                StockQuantity = product.Stock?.Quantity ?? 0,
                Category = product.Category?.Name ?? "No Category",
                ProductType = product.ProductType?.Name ?? "No Type",
                Supplier = product.Supplier?.Name ?? "No Supplier",
                product.CreationDate
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving product", Error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productDto)
    {
        try
        {
            // Verificar si el nombre del producto ya está en uso
            if (await _context.Products.AnyAsync(p => p.Name == productDto.Name))
            {
                return BadRequest(new { Message = "The product name is already in use." });
            }

            // Crear un nuevo producto
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CategoryId = productDto.CategoryId,
                ProductTypeId = productDto.ProductTypeId,
                StockId = productDto.StockId,
                SupplierId = productDto.SupplierId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Respuesta
            var response = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                Category = (await _context.Categories.FindAsync(product.CategoryId))?.Name ?? "No Category",
                ProductType = (await _context.ProductTypes.FindAsync(product.ProductTypeId))?.Name ?? "No Type",
                StockQuantity = (await _context.Stocks.FindAsync(product.StockId))?.Quantity ?? 0,
                Supplier = (await _context.Suppliers.FindAsync(product.SupplierId))?.Name ?? "No Supplier",
                product.CreationDate
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while creating the product.", Error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDto updatedProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar el producto por ID
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }

            // Verificar si el nombre del producto ya está en uso (si el nombre ha cambiado)
            if (await _context.Products.AnyAsync(p => p.Id != id && p.Name == updatedProductDto.Name))
            {
                return BadRequest(new { Message = "Product name is already in use" });
            }

            // Actualizar los datos del producto
            product.Name = updatedProductDto.Name;
            product.Description = updatedProductDto.Description;
            product.Price = updatedProductDto.Price;
            product.CategoryId = updatedProductDto.CategoryId;
            product.ProductTypeId = updatedProductDto.ProductTypeId;
            product.StockId = updatedProductDto.StockId;
            product.SupplierId = updatedProductDto.SupplierId;

            // Guardar los cambios
            await _context.SaveChangesAsync();

            // Crear una respuesta
            var response = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                Category = (await _context.Categories.FindAsync(product.CategoryId))?.Name ?? "No Category",
                ProductType = (await _context.ProductTypes.FindAsync(product.ProductTypeId))?.Name ?? "No Type",
                StockQuantity = (await _context.Stocks.FindAsync(product.StockId))?.Quantity ?? 0,
                Supplier = (await _context.Suppliers.FindAsync(product.SupplierId))?.Name ?? "No Supplier",
                product.CreationDate
            };

            return Ok(new { Message = "Product updated successfully", Product = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error updating product", Error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a product by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            // Buscar el producto por ID
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found." });
            }

            // Eliminar el producto
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Product with ID {id} successfully deleted." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error deleting product", Error = ex.Message });
        }
    }



}

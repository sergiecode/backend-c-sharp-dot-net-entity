using BackendUsuarios.Data;
using BackendUsuarios.Models.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public CategoryController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();

            var response = categories.Select(c => new
            {
                c.Id,
                c.Name,
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving categories", Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a category by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { Message = $"Category with ID {id} not found." });
            }

            var response = new
            {
                category.Id,
                category.Name,
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving category", Error = ex.Message });
        }
    }


    /// <summary>
    /// Creates a new category.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto categoryDto)
    {
        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryDto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var response = new
            {
                category.Id,
                category.Name
            };

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error creating category", Error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto updatedCategoryDto)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { Message = "Category not found" });
            }

            category.Name = updatedCategoryDto.Name;

            await _context.SaveChangesAsync();

            var response = new
            {
                category.Id,
                category.Name
            };

            return Ok(new { Message = "Category updated successfully", Category = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error updating category", Error = ex.Message });
        }
    }


    /// <summary>
    /// Deletes a category by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { Message = $"Category with ID {id} not found." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Category with ID {id} successfully deleted." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error deleting category", Error = ex.Message });
        }
    }



}
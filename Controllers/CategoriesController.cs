using BackendUsuarios.Data;
using BackendUsuarios.Models.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : BaseController<Category, CategoryCreateDto, CategoryUpdateDto>
{
    private readonly IConfiguration _configuration;

    public CategoriesController(AppDbContext context, IConfiguration configuration)
        : base(context)
    {
        _configuration = configuration;
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

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "An error occurred while creating the category.");
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
            return HandleException(ex, "Error updating category");
        }
    }
}

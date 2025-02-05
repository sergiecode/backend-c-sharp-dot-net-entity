using BackendUsuarios.Data;
using BackendUsuarios.Models.Stocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public StockController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Retrieves all stocks.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllStocks()
    {
        try
        {
            var stocks = await _context.Stocks.ToListAsync();

            var response = stocks.Select(s => new
            {
                s.Id,
                s.Quantity,
                s.UpdateDate,
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving stocks", Error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a stock by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStockById(Guid id)
    {
        try
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);

            if (stock == null)
            {
                return NotFound(new { Message = $"Stock with ID {id} not found." });
            }

            var response = new
            {
                stock.Id,
                stock.Quantity,
                stock.UpdateDate,
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving stock", Error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new stock.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateStock([FromBody] StockCreateDto stockDto)
    {
        try
        {
            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                Quantity = stockDto.Quantity,
                UpdateDate = DateTime.UtcNow
            };

            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            var response = new
            {
                stock.Id,
                stock.Quantity,
                stock.UpdateDate,
            };

            return CreatedAtAction(nameof(GetStockById), new { id = stock.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error creating stock", Error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing stock.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] StockUpdateDto updatedStockDto)
    {
        try
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound(new { Message = "Stock not found" });
            }

            stock.Quantity = updatedStockDto.Quantity;
            stock.UpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new
            {
                stock.Id,
                stock.Quantity,
                stock.UpdateDate,
            };

            return Ok(new { Message = "Stock updated successfully", Stock = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error updating stock", Error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a stock by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStock(Guid id)
    {
        try
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound(new { Message = $"Stock with ID {id} not found." });
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Stock with ID {id} successfully deleted." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error deleting stock", Error = ex.Message });
        }
    }



}
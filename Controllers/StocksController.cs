using BackendUsuarios.Data;
using BackendUsuarios.Models.Stocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StocksController : BaseController<Stock, StockCreateDto, StockUpdateDto>
{
    private readonly IConfiguration _configuration;

    public StocksController(AppDbContext context, IConfiguration configuration)
        : base(context)
    {
        _configuration = configuration;
    }

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

            return CreatedAtAction(nameof(GetById), new { id = stock.Id }, response);
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
}

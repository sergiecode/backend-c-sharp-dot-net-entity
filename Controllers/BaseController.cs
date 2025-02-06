using BackendUsuarios.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendUsuarios.Controllers
{
    public abstract class BaseController<T, TCreateDto, TUpdateDto> : ControllerBase
    where T : class
    {
        protected readonly AppDbContext _context;

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/[Controller]
        [HttpGet]
        public virtual async Task<IActionResult> GetAll()
        {
            try
            {
                var entities = await _context.Set<T>().ToListAsync();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error retrieving {typeof(T).Name}s", Error = ex.Message });
            }
        }

        // GET: api/[Controller]/{id}
        [HttpGet("{id:guid}")]
        public virtual async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var entity = await _context.Set<T>().FindAsync(id);

                if (entity == null)
                {
                    return NotFound(new { Message = $"{typeof(T).Name} with ID {id} not found." });
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error retrieving {typeof(T).Name}", Error = ex.Message });
            }
        }


        // DELETE: api/[Controller]/{id}
        [HttpDelete("{id:guid}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var entity = await _context.Set<T>().FindAsync(id);
                if (entity == null)
                {
                    return NotFound(new { Message = $"{typeof(T).Name} with ID {id} not found." });
                }

                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"{typeof(T).Name} with ID {id} successfully deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error deleting {typeof(T).Name}", Error = ex.Message });
            }
        }
        protected IActionResult HandleException(Exception ex, string customMessage)
        {

            return StatusCode(500, new { Message = customMessage, Error = ex.Message });
        }
    }



}

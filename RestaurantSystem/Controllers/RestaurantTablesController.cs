using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantTablesController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public RestaurantTablesController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: api/RestaurantTables?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> GetTables(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Tables
                .Include(t => t.Orders)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var tables = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());
            Response.Headers.Add("X-Current-Page", pageNumber.ToString());

            return Ok(tables);
        }

        // GET: api/RestaurantTables/search?number=5&zone=тераса&minCapacity=4&isAvailable=true
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> SearchTables(
            [FromQuery] int? number,
            [FromQuery] string? zone,
            [FromQuery] int? minCapacity,
            [FromQuery] bool? isAvailable,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Tables
                .Include(t => t.Orders)
                .AsQueryable();

            if (number.HasValue)
            {
                query = query.Where(t => t.Number == number.Value);
            }

            if (!string.IsNullOrWhiteSpace(zone))
            {
                query = query.Where(t => t.Zone != null && t.Zone.Contains(zone));
            }

            if (minCapacity.HasValue)
            {
                query = query.Where(t => t.Capacity >= minCapacity.Value);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(t => t.IsAvailable == isAvailable.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var tables = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());

            return Ok(tables);
        }

        // GET: api/RestaurantTables/available?forDate=2024-01-20&capacity=4
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> GetAvailableTables(
            [FromQuery] DateTime? forDate,
            [FromQuery] int? capacity)
        {
            var query = _context.Tables
                .Where(t => t.IsAvailable)
                .AsQueryable();

            if (capacity.HasValue)
            {
                query = query.Where(t => t.Capacity >= capacity.Value);
            }

            if (forDate.HasValue)
            {
                var busyTableIds = await _context.Orders
                    .Where(o => o.OrderTime.Date == forDate.Value.Date)
                    .Select(o => o.TableId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(t => !busyTableIds.Contains(t.Id));
            }

            return await query.ToListAsync();
        }

        // GET: api/RestaurantTables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantTable>> GetTable(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Orders)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound($"Маса с ID {id} не съществува.");
            }

            return Ok(table);
        }

        // POST: api/RestaurantTables
        [HttpPost]
        public async Task<ActionResult<RestaurantTable>> CreateTable(RestaurantTable table)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _context.Tables
                .AnyAsync(t => t.Number == table.Number);

            if (exists)
            {
                return BadRequest($"Маса с номер {table.Number} вече съществува.");
            }

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTable), new { id = table.Id }, table);
        }

        // PUT: api/RestaurantTables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, RestaurantTable table)
        {
            if (id != table.Id)
            {
                return BadRequest("ID в URL-а не съвпада с ID на масата.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _context.Tables
                .AnyAsync(t => t.Number == table.Number && t.Id != id);

            if (exists)
            {
                return BadRequest($"Маса с номер {table.Number} вече съществува.");
            }

            _context.Entry(table).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TableExists(id))
                {
                    return NotFound($"Маса с ID {id} не съществува.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/RestaurantTables/5/toggle-availability
        [HttpPatch("{id}/toggle-availability")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null)
            {
                return NotFound($"Маса с ID {id} не съществува.");
            }

            table.IsAvailable = !table.IsAvailable;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Наличността на маса {table.Number} е променена на {(table.IsAvailable ? "свободна" : "заета")}",
                isAvailable = table.IsAvailable
            });
        }

        // PATCH: api/RestaurantTables/5/clean
        [HttpPatch("{id}/clean")]
        public async Task<IActionResult> MarkAsCleaned(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null)
            {
                return NotFound($"Маса с ID {id} не съществува.");
            }

            table.LastCleaned = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Маса {table.Number} е маркирана като почистена.",
                lastCleaned = table.LastCleaned
            });
        }

        // DELETE: api/RestaurantTables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Orders)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound($"Маса с ID {id} не съществува.");
            }

            if (table.Orders != null && table.Orders.Any())
            {
                return BadRequest($"Не можете да изтриете маса {table.Number}, защото има {table.Orders.Count} свързани поръчки.");
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Маса {table.Number} беше изтрита успешно." });
        }

        private async Task<bool> TableExists(int id)
        {
            return await _context.Tables.AnyAsync(e => e.Id == id);
        }
    }
}
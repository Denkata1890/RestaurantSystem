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
    public class OrdersController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public OrdersController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                .OrderByDescending(o => o.OrderTime)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());
            Response.Headers.Add("X-Current-Page", pageNumber.ToString());

            return Ok(orders);
        }

        // GET: api/Orders/search?fromDate=2024-01-01&toDate=2024-01-31&status=Нова&tableId=5&employeeId=3&minTotal=20&maxTotal=100
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Order>>> SearchOrders(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? status,
            [FromQuery] int? tableId,
            [FromQuery] int? employeeId,
            [FromQuery] decimal? minTotal,
            [FromQuery] decimal? maxTotal,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderTime <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status.Contains(status));
            }

            if (tableId.HasValue)
            {
                query = query.Where(o => o.TableId == tableId.Value);
            }

            if (employeeId.HasValue)
            {
                query = query.Where(o => o.EmployeeId == employeeId.Value);
            }

            if (minTotal.HasValue)
            {
                query = query.Where(o => o.TotalPrice >= minTotal.Value);
            }

            if (maxTotal.HasValue)
            {
                query = query.Where(o => o.TotalPrice <= maxTotal.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());

            return Ok(orders);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Поръчка с ID {id} не съществува.");
            }

            return Ok(order);
        }

        // GET: api/Orders/table/5
        [HttpGet("table/{tableId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByTable(int tableId)
        {
            var orders = await _context.Orders
                .Where(o => o.TableId == tableId)
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders/employee/5
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByEmployee(int employeeId)
        {
            var orders = await _context.Orders
                .Where(o => o.EmployeeId == employeeId)
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders/status/Нова
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByStatus(string status)
        {
            var orders = await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders/date/2024-01-20
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByDate(DateTime date)
        {
            var orders = await _context.Orders
                .Where(o => o.OrderTime.Date == date.Date)
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return Ok(orders);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tableExists = await _context.Tables.AnyAsync(t => t.Id == order.TableId);
            if (!tableExists)
            {
                return BadRequest($"Маса с ID {order.TableId} не съществува.");
            }

            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == order.EmployeeId);
            if (!employeeExists)
            {
                return BadRequest($"Служител с ID {order.EmployeeId} не съществува.");
            }

            order.OrderTime = DateTime.Now;
            order.TotalPrice = 0;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _context.Entry(order).Reference(o => o.Table).LoadAsync();
            await _context.Entry(order).Reference(o => o.Waiter).LoadAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // POST: api/Orders/5/add-item
        [HttpPost("{id}/add-item")]
        public async Task<IActionResult> AddItemToOrder(int id, OrderDetail orderDetail)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Поръчка с ID {id} не съществува.");
            }

            var menuItem = await _context.MenuItems.FindAsync(orderDetail.MenuItemId);
            if (menuItem == null)
            {
                return BadRequest($"Ястие с ID {orderDetail.MenuItemId} не съществува.");
            }

            orderDetail.OrderId = id;
            orderDetail.SubTotal = menuItem.Price * orderDetail.Quantity;

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            // Обновяване на TotalPrice
            order.TotalPrice = order.OrderDetails.Sum(od => od.SubTotal);
            await _context.SaveChangesAsync();

            return Ok(orderDetail);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest("ID в URL-а не съвпада с ID на поръчката.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrderExists(id))
                {
                    return NotFound($"Поръчка с ID {id} не съществува.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Orders/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound($"Поръчка с ID {id} не съществува.");
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Статусът на поръчка {id} е променен на '{status}'",
                status = order.Status
            });
        }

        // PATCH: api/Orders/5/calculate-total
        [HttpPatch("{id}/calculate-total")]
        public async Task<IActionResult> CalculateOrderTotal(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Поръчка с ID {id} не съществува.");
            }

            order.TotalPrice = order.OrderDetails.Sum(od => od.SubTotal);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Общата сума на поръчка {id} е преизчислена",
                totalPrice = order.TotalPrice
            });
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Поръчка с ID {id} не съществува.");
            }

            // Първо изтриваме детайлите (Cascade ще ги изтрие, но за по-сигурно)
            _context.OrderDetails.RemoveRange(order.OrderDetails);

            // После изтриваме поръчката
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Поръчка с ID {id} беше изтрита успешно." });
        }

        private async Task<bool> OrderExists(int id)
        {
            return await _context.Orders.AnyAsync(e => e.Id == id);
        }
    }
}
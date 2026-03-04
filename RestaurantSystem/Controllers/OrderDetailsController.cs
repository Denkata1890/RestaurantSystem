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
    public class OrderDetailsController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public OrderDetailsController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: api/OrderDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.OrderDetails
                .Include(od => od.MenuItem)
                .Include(od => od.Order)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orderDetails = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());

            return Ok(orderDetails);
        }

        // GET: api/OrderDetails/order/5
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetByOrder(int orderId)
        {
            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Include(od => od.MenuItem)
                .ToListAsync();

            return Ok(orderDetails);
        }

        // GET: api/OrderDetails/menuitem/5
        [HttpGet("menuitem/{menuItemId}")]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetByMenuItem(int menuItemId)
        {
            var orderDetails = await _context.OrderDetails
                .Where(od => od.MenuItemId == menuItemId)
                .Include(od => od.Order)
                .ToListAsync();

            return Ok(orderDetails);
        }

        // GET: api/OrderDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails
                .Include(od => od.MenuItem)
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.Id == id);

            if (orderDetail == null)
            {
                return NotFound($"Детайл на поръчка с ID {id} не съществува.");
            }

            return Ok(orderDetail);
        }

        // POST: api/OrderDetails
        [HttpPost]
        public async Task<ActionResult<OrderDetail>> CreateOrderDetail(OrderDetail orderDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
            if (order == null)
            {
                return BadRequest($"Поръчка с ID {orderDetail.OrderId} не съществува.");
            }

            var menuItem = await _context.MenuItems.FindAsync(orderDetail.MenuItemId);
            if (menuItem == null)
            {
                return BadRequest($"Ястие с ID {orderDetail.MenuItemId} не съществува.");
            }

            // Изчисляване на SubTotal
            orderDetail.SubTotal = menuItem.Price * orderDetail.Quantity;

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            // Обновяване на TotalPrice на поръчката
            order.TotalPrice = await _context.OrderDetails
                .Where(od => od.OrderId == order.Id)
                .SumAsync(od => od.SubTotal);
            await _context.SaveChangesAsync();

            await _context.Entry(orderDetail).Reference(od => od.MenuItem).LoadAsync();
            await _context.Entry(orderDetail).Reference(od => od.Order).LoadAsync();

            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.Id }, orderDetail);
        }

        // PUT: api/OrderDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderDetail(int id, OrderDetail orderDetail)
        {
            if (id != orderDetail.Id)
            {
                return BadRequest("ID в URL-а не съвпада с ID на детайла.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var menuItem = await _context.MenuItems.FindAsync(orderDetail.MenuItemId);
            if (menuItem == null)
            {
                return BadRequest($"Ястие с ID {orderDetail.MenuItemId} не съществува.");
            }

            // Преизчисляване на SubTotal
            orderDetail.SubTotal = menuItem.Price * orderDetail.Quantity;

            _context.Entry(orderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Обновяване на TotalPrice на поръчката
                var order = await _context.Orders.FindAsync(orderDetail.OrderId);
                if (order != null)
                {
                    order.TotalPrice = await _context.OrderDetails
                        .Where(od => od.OrderId == order.Id)
                        .SumAsync(od => od.SubTotal);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrderDetailExists(id))
                {
                    return NotFound($"Детайл на поръчка с ID {id} не съществува.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/OrderDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.Id == id);

            if (orderDetail == null)
            {
                return NotFound($"Детайл на поръчка с ID {id} не съществува.");
            }

            var orderId = orderDetail.OrderId;

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            // Обновяване на TotalPrice на поръчката
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.TotalPrice = await _context.OrderDetails
                    .Where(od => od.OrderId == order.Id)
                    .SumAsync(od => od.SubTotal);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = $"Детайл на поръчка с ID {id} беше изтрит успешно." });
        }

        private async Task<bool> OrderDetailExists(int id)
        {
            return await _context.OrderDetails.AnyAsync(e => e.Id == id);
        }
    }
}
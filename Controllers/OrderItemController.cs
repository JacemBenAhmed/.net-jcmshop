using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projTP.Data;
using projTP.Models;

namespace projTP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderItemController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<OrderItem>> AddOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderItems), new { id = orderItem.Id }, orderItem);
        }
    }
}

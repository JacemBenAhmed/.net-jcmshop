using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projTP.Data;
using projTP.Models;

namespace projTP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartItemController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                {
                    return NotFound("No cart items found.");
                }

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CartItem>> AddCartItem(CartItem cartItem)
        {
            try
            {
                if (cartItem == null)
                {
                    return BadRequest("Invalid cart item.");
                }

                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCartItems), new { id = cartItem.Id }, cartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int userId, int productId, int quantity = 1)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return NotFound($"Cart for user {userId} not found.");
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return NotFound($"Product with ID {productId} not found.");
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price 
                    };
                    cart.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Product added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

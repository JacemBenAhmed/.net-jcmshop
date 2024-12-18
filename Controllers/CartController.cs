using Microsoft.AspNetCore.Mvc;
using projTP.Models;
using projTP.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace projTP.Controllers
{
    public class CartController : BaseController
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userID=-1;

            //var userID = HttpContext.Session.GetInt32("userId");


            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                 userID = int.Parse(userIdString); 
                
            }

            if (userID == null)
            {
                return RedirectToAction("Login", "Account"); 
            }

            var cart = _context.Carts
                               .FirstOrDefault(c => c.UserId == userID);

            if (cart == null)
            {
                return View(new List<CartItem>()); 
            }

            var cartItems = _context.CartItems
                                    .Where(ci => ci.CartId == cart.Id)
                                    .Include(ci => ci.Product)
                                    .ToList();

            var cartId = _context.Carts
                                 .Where(c => c.UserId == userID)
                                 .Select(c => c.Id) 
                                 .FirstOrDefault();
            var totalAmount = cartItems
                              .Where(ci => ci.CartId == cartId) 
                              .Sum(ci => ci.Price ); 

            ViewBag.TotalAmount = totalAmount;

            return View(cartItems); 
        }








        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var userId = -1;

            //var userID = HttpContext.Session.GetInt32("userId");


            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                userId = int.Parse(userIdString);

            }

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var productPrice = _context.Products
                                       .Where(p => p.Id == productId)
                                       .Select(p => p.Price)
                                       .FirstOrDefault();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                Price = productPrice,
            };

            _context.CartItems.Add(cartItem);
            _context.SaveChanges();

          
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult UpdateCart(int qte, int productId)
        {
            var userId = -1;

            //var userID = HttpContext.Session.GetInt32("userId");


            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                userId = int.Parse(userIdString);

            }

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var cartItem = _context.CartItems
                           .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (cartItem == null)
            {
                return RedirectToAction("Index", "Cart"); // Cart item not found
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product != null)
            {
                // Check if the requested quantity is available
                if (qte > product.Quantity)  // Assuming `Stock` is the property for available quantity
                {
                    TempData["ErrorMessage"] = "Insufficient stock for this product.";
                    return RedirectToAction("Index", "Cart"); // Redirect back to the cart page
                }

                // Update the quantity of the cart item and recalculate the price
                cartItem.Quantity = qte;
                cartItem.Price = product.Price * cartItem.Quantity;

                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Cart"); // Redirect to the cart page to reflect the changes
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var userId = -1;

            //var userID = HttpContext.Session.GetInt32("userId");


            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                userId = int.Parse(userIdString);

            }

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var cartItem = _context.CartItems
                                   .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);  // Remove the cart item
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Cart");  // Redirect to the cart page to reflect the changes
        }











    }
}

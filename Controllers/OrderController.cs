using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projTP.Controllers;
using projTP.Data;
using projTP.Models;
using projTP.ViewModels;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Net;
using System.Net.Mail;


public class OrderController : BaseController
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userId = -1;

        //var userID = HttpContext.Session.GetInt32("userId");


        if (Request.Cookies.TryGetValue("userId", out var userIdString))
        {
            userId = int.Parse(userIdString);

        }
        if (userId == null)
        {
            TempData["ErrorMessage"] = "User not logged in.";
            return RedirectToAction("Index", "Cart");
        }

        var cartId = _context.Carts.FirstOrDefault(c => c.UserId == userId)?.Id;
        var cartItems = _context.CartItems
            .Where(ci => ci.CartId == cartId)
            .Include(ci => ci.Product)
            .ToList();

        var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Price);

        var viewModel = new OrderIndexViewModel
        {
            CartItems = cartItems,
            TotalAmount = totalAmount,
            Adresse = "" 
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderIndexViewModel model)
    {
        if (!Request.Cookies.TryGetValue("userId", out var userIdString) || !int.TryParse(userIdString, out var userId))
        {
            TempData["ErrorMessage"] = "User not logged in.";
            return RedirectToAction("Index", "Cart");
        }

        if (!ValidatePayment(model.CardNumber, model.ExpirationDate, model.CVV))
        {
            TempData["ErrorMessage"] = "Payment failed. Please try again.";
            return RedirectToAction("Index", "Order");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Adresse = model.Adresse,
            TotalAmount = model.TotalAmount,
            Status = "Paid",
            OrderNumber = Guid.NewGuid().ToString().Substring(0, 8)
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var cartId = _context.Carts.FirstOrDefault(c => c.UserId == userId)?.Id;
        var cartItems = _context.CartItems.Where(ci => ci.CartId == cartId).ToList();

        foreach (var item in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            });

            var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
            {
                product.Quantity -= item.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        bool emailSent = false;
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("JCMshop", "chouikhiabdallahpro@gmail.com"));
            message.To.Add(new MailboxAddress("Recipient Name", "jacem0987654321@gmail.com"));
            message.Subject = "Order Confirmation";

            message.Body = new TextPart("html")
            {
                Text = $"<p>Thank you for your order!</p>" +
                       $"<p>Your payment has been successfully processed.</p>" +
                       $"<p>Order Number: <strong>{order.OrderNumber}</strong></p>" +
                       $"<p>Delivery Address: <strong>{order.Adresse}</strong></p>" +
                       $"<p>Your order will arrive in 1 to 2 days.</p>" +
                       $"<p>Total Amount: <strong>${order.TotalAmount}</strong></p>"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("chouikhiabdallahpro@gmail.com", "nlhe fija uodt llkm");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            emailSent = true;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Order created, but failed to send email: {ex.Message}";
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        if (emailSent)
        {
            TempData["SuccessMessage"] = "Order created and confirmation email sent.";
        }
        else
        {
            TempData["WarningMessage"] = "Order created, but confirmation email not sent.";
        }

        return RedirectToAction("Details", new { id = order.Id });
    }


    private bool ValidatePayment(string cardNumber, string expirationDate, string cvv)
    {
        
        return cardNumber == "4111111111111111" && expirationDate == "12/25" && cvv == "123";
    }




    [HttpGet]
    public IActionResult Details(int id)
    {
        var order = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == id);

        return View(order);
    }
}

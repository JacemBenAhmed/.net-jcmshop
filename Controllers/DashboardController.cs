using Microsoft.AspNetCore.Mvc;
using projTP.Data;
using projTP.Models;
using System.Linq;

namespace projTP.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var totalProducts = _context.Products.Count();
            var totalOrders = _context.Orders.Count();
            var totalRevenue = _context.Orders.Sum(o => o.TotalAmount);
            var totalCategories = _context.Categories.Count();

            var recentOrders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.OrderNumber,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    UserName = o.User.UserName
                })
                .ToList();

            var topSellingProducts = _context.Products
                .Select(p => new
                {
                    p.Nom,
                    TotalSold = p.OrderItems.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToList();

            ViewData["TotalProducts"] = totalProducts;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["TotalCategories"] = totalCategories;
            ViewData["RecentOrders"] = recentOrders;
            ViewData["TopSellingProducts"] = topSellingProducts;

            return View();
        }
    }
}

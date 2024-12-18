using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System;
using Microsoft.EntityFrameworkCore;
using projTP.Data;

namespace projTP.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int page = 1, int pageSize = 5, string searchQuery = "", decimal minPrice = 0, decimal maxPrice = 1000)
        {
            string userName = "Anonyme";
            bool isAuthenticated = false;

            var token = Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(token))
            {
                isAuthenticated = true;

                var jwtHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = jwtHandler.ReadJwtToken(token);
                    var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                    if (nameClaim != null)
                    {
                        userName = nameClaim.Value;
                    }
                }
                catch (Exception)
                {
                    isAuthenticated = false;
                }
            }

            ViewData["UserName"] = userName;
            ViewData["IsAuthenticated"] = isAuthenticated;

            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                productsQuery = productsQuery.Where(p => p.Nom.Contains(searchQuery));
            }

            if (minPrice > 0)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice);
            }

            if (maxPrice < 1000)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice);
            }

            var totalProducts = productsQuery.Count(); 
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize); 

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var products = productsQuery
                .Include(p => p.Category)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products);
        }


       
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken");

            return RedirectToAction("Index");
        }
    }
}

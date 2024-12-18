using Microsoft.AspNetCore.Mvc;
using projTP.Models;
using projTP.Data;
using projTP.DTOs;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace projTP.Controllers
{
    public class UserController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly string _jwtSecret;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtSecret = configuration["Jwt:SecretKey"];
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var token = Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");  
            }
            return View();
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            var token = Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home"); 
            }

            var model = new UserRegisterDto();
            return View(model);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (dto.Password.Length < 4)
            {
                ModelState.AddModelError("Password", "Password must be at least 4 characters long.");
                return View(dto);
            }

            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View(dto);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = passwordHash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Adresse = dto.Adresse,
                Role = "user",
                DateCreated = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.Id, 
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "User");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                ModelState.AddModelError("LoginError", "Invalid email or password.");
                return View(dto);
            }

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            Console.WriteLine("JWT Token: " + token);

            // HttpContext.Session.SetInt32("userId", user.Id);

            Response.Cookies.Append("userId", user.Id.ToString(), new CookieOptions
            {
                HttpOnly = false,  
                Secure = true,     
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            Response.Cookies.Append("userRole", user.Role.ToString(), new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            Response.Cookies.Append("userName", user.UserName.ToString(), new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            Console.WriteLine(user.Id);

            if(user.Role=="admin")
            return RedirectToAction("Index", "Dashboard");
            else return RedirectToAction("Index", "Home");


        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var userID = -1;

            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                userID = int.Parse(userIdString);
            }

            if (userID == -1)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userID);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost("edituser")]
        public async Task<IActionResult> Profile(User user)
        {
            var userID = -1;

            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                userID = int.Parse(userIdString);
            }

            

            try
            {
                var existingUser = await _context.Users.FindAsync(userID);

                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Adresse = user.Adresse;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile", "User");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while updating the profile.";
                return RedirectToAction("Profile");
            }
        }








        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken");
            Response.Cookies.Delete("authToken");

            return RedirectToAction("Index", "Home");  
        }




        [HttpGet("historique")]
        public IActionResult Historique()
        {
            var userID = -1;



            if (Request.Cookies.TryGetValue("userId", out var userIdString))
            {
                userID = int.Parse(userIdString);

            }


            var orders = _context.Orders
                .Where(o => o.UserId == userID)
                .OrderByDescending(o => o.OrderDate) 
                .ToList();

            return View(orders); 
        }

    }
}

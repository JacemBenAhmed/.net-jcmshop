using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projTP.Data;
using projTP.Models;
using System.IO;

namespace projTP.Controllers
{
    public class ProductController : BaseController
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();

            return View(products);
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync(Product product, IFormFile imageFile)
        {
            if (imageFile.Length > 0)
            {
                product.ImgPath = Path.GetFileName(imageFile.FileName);
                var fileName = Path.GetFileName(imageFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");


                var filePath = Path.Combine(uploadsFolder, fileName);

                Directory.CreateDirectory(uploadsFolder);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }


            }
            else
            {
                TempData["ErrorMessage"] = "Erreur lors de l'ajout du produit. Vérifiez les champs.";
                return View(product);
            }
                
            // if (product.ImgPath==null)

            //product.ImgPath  = Path.GetFileName(imageFile.FileName);

            product.DateAdded = DateTime.Now;

                _context.Products.Add(product);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Le produit a été ajouté avec succès.";
                return RedirectToAction("Index", "Product");


                ViewBag.Categories = _context.Categories.ToList();
                
                return View(product);
            }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

          
                try
                {
                    existingProduct.Nom = product.Nom;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Quantity = product.Quantity;

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id))
                    {
                        return NotFound("Product not found during concurrency check.");
                    }
                    else
                    {
                        throw; 
                    }
                }
            

            return View(product);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(); 
            }

            
                _context.Products.Remove(product);
                await _context.SaveChangesAsync(); 

                return RedirectToAction(nameof(Index));
            
            
        }









    }
}



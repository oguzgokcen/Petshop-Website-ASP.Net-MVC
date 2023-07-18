using Microsoft.AspNetCore.Mvc;
using Proje.Data;
using Proje.Models;

namespace Proje.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;

        }
        public IActionResult Urunler()
        {
            var products = _context.Products.ToList();
            return View(products);
        }
        [HttpGet]
        public IActionResult Detay(int id)
        {
            var product = _context.Products.Find(id);
            return View(product);
        }

    }
}

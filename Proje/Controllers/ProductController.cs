using Microsoft.AspNetCore.Mvc;
using Proje.Data;
using Proje.Models;
using Proje.Models.DisplayModel;
using Proje.Repositories;

namespace Proje.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;

        }
        public async Task< IActionResult> Urunler( bool stock, decimal maxPrice = 1000, string sterm = "")
        {
            IEnumerable<Product> products = await _productRepository.GetProducts(maxPrice, stock,sterm);
            ProductDisplayModel displayModel = new ProductDisplayModel
            {
                Products = products,
                sterm = sterm,
                maxPrice = maxPrice,
                stock = stock
            };
            return View(displayModel);
        }
        [HttpGet]
        public async Task<IActionResult> Detay(int id)
        {
            var product = await _productRepository.Find(id);
            return View(product);
        }

    }
}

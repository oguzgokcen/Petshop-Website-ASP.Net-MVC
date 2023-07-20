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
        public async Task< IActionResult> Urunler(string sterm="")
        {
            IEnumerable<Product> products = await _productRepository.GetProducts(sterm);
            ProductDisplayModel displayModel = new ProductDisplayModel
            {
                Products = products,
                sterm = sterm
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

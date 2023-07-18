using Microsoft.EntityFrameworkCore;
using TurkcellModel.Models;

namespace Proje.Models
{
    public class ProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<Product>> GetProducts()
        {
            var ProductList = await _db.Products.ToListAsync();
            return ProductList;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Proje.Data;
using Proje.Models;

namespace Proje.Repositories
{
    public class ProductRepository:IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Product>> GetProducts(decimal maxprice,bool stock, string sterm = "")
        {
            if (string.IsNullOrEmpty(sterm))
                sterm = "";
            sterm = sterm.ToLower();
            IEnumerable<Product> products = await 
                (from product in _db.Products
                 where string.IsNullOrWhiteSpace(sterm) ||
                 (product != null && product.Name.ToLower().Contains(sterm))
                 select product).ToListAsync();
            products = products.Where(x =>x.Price<=maxprice).ToList();
            if (stock != false)
                products = products.Where(x =>x.Stock == stock).ToList();
            return products;
        }
        public async Task<Product> Find(int id)
        {
            return _db.Products.Find(id);
        }
    }
}

using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proje.Data;
using Proje.Models;
using System.Security.Claims;

namespace BookShoppingCartMvcUI.Repositories
{
    public class UserInformationRepository : IUserInformationRepository
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public UserInformationRepository(AppDbContext db,
            UserManager<IdentityUser> userManager,
             IHttpContextAccessor httpContextAccessor,
             IConfiguration config)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _config = config;
        }

        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged-in");
            var orders = await _db.Orders
                            .Include(x=>x.OrderStatus)
                            .Include(x => x.OrderDetail)
                            .ThenInclude(x=>x.Product)
                            .Where(a=>a.UserId==userId)
                            .ToListAsync();
            return orders;
        }
        public async Task<bool> SetUserInformation(UserInformation userInformation)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                userInformation.userId = userId;
                if (_db.userInformation.Any(x => x.userId == userId)) // if userInformation exists, update
                {
                    var olduserInformation = _db.userInformation.FirstOrDefault(x=> x.userId == userId);
                    _db.userInformation.Remove(olduserInformation);
                    _db.SaveChanges();
                    var y = _db.userInformation.AnyAsync(x => x.userId == userId);
                }
                var user =_db.userInformation.Add(userInformation);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("Ekleme Başarısız");
            }
        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
        public async Task<Order> GetOrderSummary(String tokenId)
        {
            var Order = await _db.Orders
                .Include(x => x.OrderDetail)
                .ThenInclude(x => x.Product)
                .Where(x => x.tokenId == tokenId).FirstOrDefaultAsync();
            return Order;
        }
        public async Task<Order> GetOrder(int id)
        {
            var Order = await _db.Orders
                .Include(x => x.OrderDetail)
                .ThenInclude(x => x.Product)
                .Where(x => x.Id  == id).FirstOrDefaultAsync();
            if (Order != null)
                return Order;
            else return null;
        }
        // If payment is succesfull set Order attributes :
        public async Task<bool> SetOrderStatus(Order order,int status,String paymentId)
        {
            try
            {
                var Order = await _db.Orders.FindAsync(order.Id);
                if (Order != null)
                {
                    Order.OrderStatusId = status;
                    if(status == 2) 
                        Order.paymentId = paymentId;
                    await _db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw new Exception("Order status ayarlanamadı");
            }
        }
        public async Task<bool> CancelOrder(int id)
        {
            var userId = GetUserId();
            try
            {
                var Order = await _db.Orders.FindAsync(id);
                CreateCancelRequest request = new CreateCancelRequest()
                {
                    Locale = Locale.TR.ToString(),
                    PaymentId = Order.paymentId,
                    Ip = "19.18.17.112"
                };
                Options options = new Options()
                {
                    ApiKey = _config["Auth:Iyzico:Api"],
                    SecretKey = _config["Auth:Iyzico:SecretKey"],
                    BaseUrl = "https://sandbox-api.iyzipay.com"
                };
                Cancel cancel = Cancel.Create(request, options);
                if (cancel.Status == "success")
                {
                    Order.OrderStatusId = 5;
                    Order.IsDeleted = true;
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            { throw new Exception("Ürün İptal Etmede Sorun Yaşandı"); }
        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proje.Data;
using Proje.Models;
using System.Net;

namespace Proje.Repositories
{
    public class CartRepository
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public CartRepository(AppDbContext appDbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _db = appDbContext;
            _userManager = userManager;
            _httpcontextAccessor = contextAccessor;
        }
        public async Task<int> AddItem(int ProductId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                // cart detail section
                var cartDetail = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.ProductId == ProductId);
                if (cartDetail is not null)
                {
                    cartDetail.Quantity += qty;
                }
                else
                {
                    var Product = _db.Products.Find(ProductId);
                    cartDetail = new CartDetail
                    {
                        ProductId = ProductId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = Product.Price
                    };
                    _db.CartDetails.Add(cartDetail); // Create new cartDetail if this product not exist in cart.
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        public async Task<int> RemoveItem(int ProductId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid cart");
                // cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.ProductId == ProductId);
                if (cartItem is null)
                    throw new Exception("No items in cart");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity = cartItem.Quantity - 1;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid userid");
            var shoppingCart = await _db.ShoppingCarts
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.ProductId)
                                  .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;

        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x =>x.UserId == userId);
            return cart;
        }
        public async Task<int> GetCartItemCount(string userId = "")
    {
        if (!string.IsNullOrEmpty(userId))
        {
            userId = GetUserId();
        }
        var data = await (from cart in _db.ShoppingCarts
                          where cart.UserId == userId
                          join cartDetail in _db.CartDetails
                          on cart.Id equals cartDetail.ShoppingCartId
                          select new { cartDetail.Id }
                    ).ToListAsync();
        return data.Count;
    }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                // logic
                // move data from cartDetail to order and order detail then we will remove cart detail
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid cart");
                var cartDetail = _db.CartDetails
                                    .Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new Exception("Cart is empty");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = 1//pending
                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _db.OrderDetails.Add(orderDetail);
                }
                _db.SaveChanges();

                // removing the cartdetails
                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        private string GetUserId()
        {
            var principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}

using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proje.Data;
using Proje.Models;
using Proje.Models.DisplayModel;
using System.Net;

namespace Proje.Repositories
{
    public class CartRepository:ICartRepository
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public CartRepository(AppDbContext appDbContext, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor)
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
                                  .ThenInclude(a => a.Product)
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
        public async Task<bool>CheckUserInformation()
        {
            var userId = GetUserId();
            bool isInformationExists =await _db.userInformation.AnyAsync(x=>
            x.userId == userId);
            return isInformationExists;
        }
        private async Task<UserInformation> GetUserInformation()
        {
            var userId = GetUserId();
            var userInformation = await _db.userInformation.FirstOrDefaultAsync(x => x.userId == userId);
            return userInformation;
        }

        public async Task<CheckoutFormParams> CreateCheckout()
        {
            var userCart = await GetUserCart();
            return new CheckoutFormParams()
            {
                userID = GetUserId(),
                userInformation = await GetUserInformation(),
                cart = userCart,
                price = userCart.CartDetails.Sum(cart => cart.UnitPrice * cart.Quantity).ToString()
            };
        }
        public Payment CheckoutForm(CheckoutFormParams param)
        {
            var price = "1.2";
            var userInformation = param.userInformation;
            var userCart = param.cart;
            var userId = param.userID;
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.Price = price;
            request.PaidPrice = price;
            request.Currency = Currency.TRY.ToString();
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = "ABC";
            paymentCard.CardNumber = "5528790000000008";
            paymentCard.ExpireMonth = "12";
            paymentCard.ExpireYear = "2030";
            paymentCard.Cvc = "123";
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;
            /*request.EnabledInstallments = new List<int>()
                {
                    1,2
                };*/
            request.CallbackUrl = "https://localhost:44340/UserInformation/UserOrders";
            var buyer = new Buyer()
            {
                Id = userId,
                Name = userInformation.name.ToString(),
                Surname = userInformation.surname.ToString(),
                GsmNumber = userInformation.phone.ToString(),
                Email = userInformation.email.ToString(),
                IdentityNumber = userInformation.tckn.ToString(),
                City = userInformation.city.ToString(),
                Country = userInformation.country.ToString(),
                LastLoginDate = "2015-10-05 12:43:35",
                RegistrationDate = "2013-04-21 15:12:09",
                Ip = "19.18.17.112",
                RegistrationAddress = userInformation.adress.ToString(),
                ZipCode = "35410"
            };
            request.Buyer = buyer;
            var shippingAdress = new Address()
            {
                ContactName = userInformation.name.ToString(),
                City = userInformation.city.ToString(),
                Country = userInformation.country.ToString(),
                Description = userInformation.adress.ToString()
            };
            request.ShippingAddress = shippingAdress;
            var billingAdress = new Address()
            {
                ContactName = userInformation.name.ToString(),
                City = userInformation.city.ToString(),
                Country = userInformation.country.ToString(),
                Description = userInformation.adress.ToString()
            };
            request.BillingAddress = billingAdress;
            var basketItems = new List<BasketItem>()
                {
                    new BasketItem()
                    {
                        Id = userCart.Id.ToString(),
                        Name = "PetShop Ürünü",
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Category1 = "PetShop Ürünü",
                        Price = price,
                        Category2 = "Hayvan Ürünü",
                    }
                };
            request.BasketItems = basketItems;
            Options options = new Options()
            {
                ApiKey = "sandbox-NnVHlqFE3pTjMHNtjNuGCwCYc865eACz",
                SecretKey = "CWn9K9kdXKsoWYuwMA8DUcMdSgL9HrQs",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
            Payment checkoutFormInitialize = Payment.Create(request, options);
            return checkoutFormInitialize;
        }
    }
}

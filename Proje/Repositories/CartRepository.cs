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
        private readonly IConfiguration _config;
        public CartRepository(AppDbContext appDbContext, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, IConfiguration config)
        {
            _db = appDbContext;
            _userManager = userManager;
            _httpcontextAccessor = contextAccessor;
            _config = config;
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

        public async Task<bool> DoCheckout(String tokenId)
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
                    OrderStatusId = 1,//işleme Alınıyor
                    tokenId = tokenId
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
            var decimal_comma = userCart.CartDetails.Sum(cart => cart.UnitPrice * cart.Quantity).ToString().Replace(",", ".");
            return new CheckoutFormParams()
            {
                userID = GetUserId(),
                userInformation = await GetUserInformation(),
                cart = userCart,
                price = decimal_comma
            };
        }
        /*public Payment CheckoutForm(CheckoutFormParams param)
        {
            var price = "1.2";
            var userInformation = param.userInformation;
            var userCart = param.cart;
            var userId = param.userID;
            Options options = new Options()
            {
                ApiKey = _config["Auth:Iyzico:Api"],
                SecretKey = _config["Auth:Iyzico:SecretKey"],
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = price;
            request.PaidPrice = price;
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "https://localhost:44340/UserInformation/UserOrders";

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = "John Doe";
            paymentCard.CardNumber = "5528790000000008";
            paymentCard.ExpireMonth = "12";
            paymentCard.ExpireYear = "2030";
            paymentCard.Cvc = "123";
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            request.Buyer = new Buyer()
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
                        Id = "BI102 ", //userCart.Id.ToString(),
                        Name = "PetShop Ürünü",
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Category1 = "PetShop Ürünü",
                        Price = price,
                        Category2 = "Hayvan Ürünü",
                    }
                };
            request.BasketItems = basketItems;

            Payment checkoutFormInitialize = Payment.Create(request, options);
            return checkoutFormInitialize;
        }
        public InstallmentInfo deneme()
        {
            Options options = new Options()
            {
                ApiKey = _config["Auth:Iyzico:Api"],
                SecretKey = _config["Auth:Iyzico:SecretKey"],
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
            RetrieveInstallmentInfoRequest request = new RetrieveInstallmentInfoRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456788";
            request.BinNumber = "554960";
            request.Price = "100";

            InstallmentInfo installmentInfo = InstallmentInfo.Retrieve(request, options);
            return installmentInfo;
        }*/
        public CheckoutFormInitialize paymentForm(CheckoutFormParams param)
        {
            Options options = new Options()
            {
                ApiKey = _config["Auth:Iyzico:Api"],
                SecretKey = _config["Auth:Iyzico:SecretKey"],
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
            var price = param.price;
            var userInformation = param.userInformation;
            var userCart = param.cart;
            var userId = param.userID;
            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.Price = price;
            request.PaidPrice = price;
            request.Currency = Currency.TRY.ToString();
            request.EnabledInstallments = new List<int>(){1};
            request.Buyer = new Buyer()
            {
                Id = userId,
                Name = userInformation.name.ToString(),
                Surname = userInformation.surname.ToString(),
                GsmNumber = userInformation.phone.ToString(),
                Email = userInformation.email.ToString(),
                IdentityNumber = userInformation.tckn.ToString(),
                City = userInformation.city.ToString(),
                Country = userInformation.country.ToString(),
                Ip = "19.18.17.112",
                RegistrationAddress = userInformation.adress.ToString()
            };

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
                        Price = price
                    }
                };
            request.CallbackUrl = "https://localhost:44340/UserInformation/OrderConfirm";
            request.BasketItems = basketItems;
            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);
            return checkoutFormInitialize;
        }
    }
}

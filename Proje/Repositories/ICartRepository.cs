using Iyzipay.Model;
using Iyzipay.Model.V2.Subscription;
using Proje.Models;
using Proje.Models.DisplayModel;

namespace Proje.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty);
        Task<int> RemoveItem(int bookId);
        Task<ShoppingCart> GetUserCart();
        Task<int> GetCartItemCount(string userId = "");
        Task<ShoppingCart> GetCart(string userId);
        Task<bool> DoCheckout(String orderToken);
        Task<CheckoutFormParams> CreateCheckout();
        Task<bool> CheckUserInformation();
        CheckoutFormInitialize paymentForm(CheckoutFormParams param);
        /*
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = price;
            request.PaidPrice = price;
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
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
                        Id = "BI102 ", //userCart.Id.ToString(),
                        Name = "PetShop Ürünü",
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Category1 = "PetShop Ürünü",
                        Price = price,
                        Category2 = "Hayvan Ürünü",
                    }
                };
            request.BasketItems = basketItems;*/
    }
}

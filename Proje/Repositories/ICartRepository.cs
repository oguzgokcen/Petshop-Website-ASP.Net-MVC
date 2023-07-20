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
        Task<bool> DoCheckout();
        Task<CheckoutFormParams> CreateCheckout();
        Payment CheckoutForm(CheckoutFormParams param);
        Task<bool> CheckUserInformation();
    }
}

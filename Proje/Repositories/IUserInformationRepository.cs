using Proje.Models;

namespace BookShoppingCartMvcUI.Repositories
{
    public interface IUserInformationRepository
    {
        Task<IEnumerable<Order>> UserOrders();
        Task<bool> SetUserInformation(UserInformation userInformation);
        Task<Order> GetOrderSummary(String tokenId);
        Task<bool> SetOrderStatus(Order order,int status,String paymentId);
        Task<bool> CancelOrder(int id);
        Task<Order> GetOrder(int id);
    }
}
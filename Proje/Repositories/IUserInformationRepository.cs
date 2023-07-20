using Proje.Models;

namespace BookShoppingCartMvcUI.Repositories
{
    public interface IUserInformationRepository
    {
        Task<IEnumerable<Order>> UserOrders();
        Task<bool> SetUserInformation(UserInformation userInformation);
    }
}
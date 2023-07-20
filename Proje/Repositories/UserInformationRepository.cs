using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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


        public UserInformationRepository(AppDbContext db,
            UserManager<IdentityUser> userManager,
             IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
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

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}

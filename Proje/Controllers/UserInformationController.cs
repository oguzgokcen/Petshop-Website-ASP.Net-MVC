using BookShoppingCartMvcUI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proje.Models;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize]
    public class UserInformationController : Controller
    {
        private readonly IUserInformationRepository _userOrderRepo;

        public UserInformationController(IUserInformationRepository userOrderRepo)
        {
            _userOrderRepo = userOrderRepo;
        }
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepo.UserOrders();
            return View(orders);
        }
        [HttpGet]
        public IActionResult SaveUserInformation()
        {
            return View();
        }
		[HttpPost]
		public IActionResult SaveUserInformation(UserInformation userInformation)
		{
            if (userInformation == null) {
                throw new Exception("userInformatin is null");
            }
            _userOrderRepo.SetUserInformation(userInformation);
            TempData["success"] = "Bilgileriniz Başarıyla Kaydedildi";
			
			return View();
		}
	}
}

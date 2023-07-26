using BookShoppingCartMvcUI.Repositories;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Proje.Models;

namespace BookShoppingCartMvcUI.Controllers
{
    //[Authorize] User ekranından erişiebileceği için auth eklenmedi
    public class UserInformationController : Controller
    {
        private readonly IUserInformationRepository _userOrderRepo;
        private readonly IConfiguration _config;
        public UserInformationController(IUserInformationRepository userOrderRepo, IConfiguration config)
        {
            _userOrderRepo = userOrderRepo;
            _config = config;
        }
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepo.UserOrders();
            return View(orders);
        }
        [HttpPost]
        public async Task<IActionResult> OrderConfirm(String token)
        { 
            //Options options =
            if (token.IsNullOrEmpty())
            {
                throw new Exception("TokenId is null");
            }
            RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest()
            {
                Token = token
            };
            var order = await _userOrderRepo.GetOrderSummary(token);
            if(order == null)
            {
                throw new Exception("Order is null");
            }
            Options options = new Options()
            {
                ApiKey = _config["Auth:Iyzico:Api"],
                SecretKey = _config["Auth:Iyzico:SecretKey"],
                BaseUrl = "https://sandbox-api.iyzipay.com"

            };
            CheckoutForm checkoutForm = CheckoutForm.Retrieve(request, options);
            if(checkoutForm.Status == "failure")
            {
                await _userOrderRepo.SetOrderStatus(order, 5,"");
                RedirectToAction("Failure", checkoutForm.ErrorMessage);
            }else
            {
                await _userOrderRepo.SetOrderStatus(order, 2,checkoutForm.PaymentId);
            }
            return View(order);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order =  await _userOrderRepo.GetOrder(id);
            return View(order);
        }
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _userOrderRepo.GetOrder(id);
            var cancel =await _userOrderRepo.CancelOrder(id);
            return View("~/Views/UserInformation/CancelConfirm.cshtml",order);
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

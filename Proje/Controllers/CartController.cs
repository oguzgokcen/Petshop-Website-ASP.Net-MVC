using Iyzipay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proje.Models.DisplayModel;
using Proje.Repositories;

namespace Proje.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        public CartController(ICartRepository cartRepository)
        {
            _cartRepository= cartRepository;
        }
        public async Task<IActionResult> AddItem(int ProductId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _cartRepository.AddItem(ProductId, qty);
            if (redirect == 0)
            {
				TempData["success"] = "Ürün Başarıyla Sepete Eklendi";
				return RedirectToAction("Detay", "Product",new { id=ProductId });

            }
            return RedirectToAction("UserCart");

        }

        public async Task<IActionResult> RemoveItem(int ProductId)
        {
            var cartCount = await _cartRepository.RemoveItem(ProductId);
            return RedirectToAction("UserCart");
        }
        public async Task<IActionResult> UserCart()
        {
            var cart = await _cartRepository.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartCount = await _cartRepository.GetCartItemCount();
            return Ok(cartCount);
        }

        public async Task<IActionResult> CreateCheckout()
        {
            // Kullanıcı bilgisi yoksa : 
            if (!await _cartRepository.CheckUserInformation())
            {
                TempData["error"] = "Kullanıcı bilgilerinizi Kaydetmediniz.";
                return RedirectToAction("SaveUserInformation", "UserInformation");
            }
            CheckoutFormParams param = await _cartRepository.CreateCheckout();
            CheckoutFormInitialize checkoutForm = _cartRepository.paymentForm(param);
            var url = checkoutForm.PaymentPageUrl;
            await _cartRepository.DoCheckout(checkoutForm.Token);
            return Redirect(url);
        }
    }
}

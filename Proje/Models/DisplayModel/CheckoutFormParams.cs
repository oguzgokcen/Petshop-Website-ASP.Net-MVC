namespace Proje.Models.DisplayModel
{
    public class CheckoutFormParams
    {
        public String userID { get; set; }
        public ShoppingCart cart { get; set; }
        public UserInformation userInformation { get; set; }
        public String price {get; set; }

    }
}

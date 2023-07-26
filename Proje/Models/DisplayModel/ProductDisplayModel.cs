namespace Proje.Models.DisplayModel
{
    public class ProductDisplayModel
    {
        public IEnumerable<Product> Products { get; set;}
        public string sterm { get; set; } = "";
        public decimal maxPrice { get; set; }
        public bool stock { get; set; }
    }
}

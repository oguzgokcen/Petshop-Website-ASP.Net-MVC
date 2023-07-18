using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proje.Models
{
    [Table("CartDetail")]
    public class CartDetail
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
    }
}

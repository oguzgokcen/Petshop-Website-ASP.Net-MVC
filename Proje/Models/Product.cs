using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proje.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string imageUrl { get; set; }
        public decimal Price { get; set; }
        public bool Stock { get; set; }
        public string Type { get; set; }
    }

}

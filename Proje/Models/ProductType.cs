using System.ComponentModel.DataAnnotations.Schema;

namespace Proje.Models
{
    [Table("ProductType")]
    public class ProductType
    {
        public int id;

        public string type;
        public string animal;
    }
}

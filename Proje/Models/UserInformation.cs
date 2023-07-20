using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proje.Models
{
    [Table("UserInformation")]
    public class UserInformation
    {
        public int Id { get; set; }
		[Required]
		public string userId { get; set; }

		[Required(ErrorMessage = "İsim boş bırakılmaz")]
		public string name { get; set; }

		[Required(ErrorMessage = "Soyad boş bırakılmaz")]
		public string surname { get; set; }

		[Required(ErrorMessage = "Email boş bırakılmaz")]
		[EmailAddress(ErrorMessage = "Geçersiz Email")]
		public string email { get; set; }

		[Required(ErrorMessage = "Şehir boş bırakılmaz")]
		public string city { get; set; }

		[Required(ErrorMessage = "Ülke boş bırakılmaz.")]
		public string country { get; set; }

		[Required(ErrorMessage = "Telefon numarası boş bırakılmaz")]
		public string phone { get; set; }

		[Required(ErrorMessage = "Sipariş Adresi Gerekli")]
		public string adress { get; set; }

		[Required(ErrorMessage = "TC kimlik numarası gerekli")]
		public string tckn { get; set; }
	}
}

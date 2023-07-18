using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Proje.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string? adress { get; set; }
        public string? city { get; set; }

    }
}

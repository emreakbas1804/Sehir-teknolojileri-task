using System.ComponentModel.DataAnnotations;

namespace webApi.Models
{
    public class RegisterModel
    {
        [Required]
        public string FullName{ get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
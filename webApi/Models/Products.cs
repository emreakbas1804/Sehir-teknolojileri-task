using System.ComponentModel.DataAnnotations;

namespace webApi.Models
{
    public class Products
    {
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public string ProductPrice { get; set; }

        public string ProductImageUrl { get; set; }
        [Required]
        public string ProductDesciription { get; set; }
        
    }
}
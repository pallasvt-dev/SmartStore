using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }
    }
}

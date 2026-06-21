using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class ProductReviewImage
    {
        public int Id { get; set; }
        public int ProductReviewId { get; set; }
        public ProductReview ProductReview { get; set; } = null!;

        [Required, StringLength(2000)]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}

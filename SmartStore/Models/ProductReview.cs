using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ProductReviewImage> Images { get; set; } = new List<ProductReviewImage>();
    }
}

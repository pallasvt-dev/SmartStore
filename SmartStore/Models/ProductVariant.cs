using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int SizeId { get; set; }

        public Size Size { get; set; } = null!;

        public int ColorId { get; set; }

        public Color Color { get; set; } = null!;

        [Required]
        [StringLength(80)]
        public string Sku { get; set; } = string.Empty;

        [Range(0, 1000000000)]
        public decimal? Price { get; set; }

        [Range(0, 1000000)]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class CartItemEntity
    {
        public int Id { get; set; }

        public int ShoppingCartId { get; set; }

        public ShoppingCart ShoppingCart { get; set; } = null!;

        public int ProductVariantId { get; set; }

        public ProductVariant ProductVariant { get; set; } = null!;

        [Range(1, 20)]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

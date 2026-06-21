using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        [Required, StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string SizeName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string ColorName { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Sku { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}

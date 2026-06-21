using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(120)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Email { get; set; }

        [Required, StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Note { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.ChoXacNhan;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.ChuaThanhToan;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippingAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

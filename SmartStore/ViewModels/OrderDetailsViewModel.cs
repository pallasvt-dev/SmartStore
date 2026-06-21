using SmartStore.Models;

namespace SmartStore.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippingAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public bool CanCancel { get; set; }
        public bool IsAdmin { get; set; }
        public List<OrderStatus> AllowedNextStatuses { get; set; } = new();
    }
}

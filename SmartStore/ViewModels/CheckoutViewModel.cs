using SmartStore.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartStore.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(120, ErrorMessage = "Họ tên không được vượt quá 120 ký tự")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(120, ErrorMessage = "Email không được vượt quá 120 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(300, ErrorMessage = "Địa chỉ nhận hàng không được vượt quá 300 ký tự")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(item => item.LineTotal);
        public decimal ShippingFee => Items.Count == 0 ? 0 : 30000;
        public decimal Discount => SubTotal >= 500000 ? 50000 : 0;
        public decimal Total => SubTotal + ShippingFee - Discount;
    }
}

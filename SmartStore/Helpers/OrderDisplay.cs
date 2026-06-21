using SmartStore.Models;

namespace SmartStore.Helpers
{
    public static class OrderDisplay
    {
        public static string Status(OrderStatus status) => status switch
        {
            OrderStatus.ChoXacNhan => "Chờ xác nhận",
            OrderStatus.DaXacNhan => "Đã xác nhận",
            OrderStatus.DangGiao => "Đang giao",
            OrderStatus.HoanThanh => "Hoàn thành",
            OrderStatus.DaHuy => "Đã hủy",
            _ => "Không xác định"
        };

        public static string Payment(PaymentStatus status) => status switch
        {
            PaymentStatus.ChuaThanhToan => "Chưa thanh toán",
            PaymentStatus.DaThanhToan => "Đã thanh toán",
            PaymentStatus.DaHoanTien => "Đã hoàn tiền",
            _ => "Không xác định"
        };

        public static string Method(PaymentMethod method) => method switch
        {
            PaymentMethod.COD => "Thanh toán khi nhận hàng",
            PaymentMethod.ChuyenKhoan => "Chuyển khoản ngân hàng",
            _ => "Không xác định"
        };

        public static string StatusClass(OrderStatus status) => status switch
        {
            OrderStatus.ChoXacNhan => "status-pending",
            OrderStatus.DaXacNhan => "status-confirmed",
            OrderStatus.DangGiao => "status-shipping",
            OrderStatus.HoanThanh => "status-completed",
            OrderStatus.DaHuy => "status-canceled",
            _ => "status-pending"
        };

        public static string PaymentClass(PaymentStatus status) => status switch
        {
            PaymentStatus.DaThanhToan => "payment-paid",
            PaymentStatus.DaHoanTien => "payment-refunded",
            _ => "payment-unpaid"
        };

        public static bool HasReached(OrderStatus current, OrderStatus step)
        {
            if (current == OrderStatus.DaHuy)
            {
                return false;
            }

            return (int)current >= (int)step;
        }
    }
}

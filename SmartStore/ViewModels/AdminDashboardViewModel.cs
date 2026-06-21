using SmartStore.Models;

namespace SmartStore.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CanceledOrders { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockVariants { get; set; }
        public int OutOfStockVariants { get; set; }
        public int TotalCustomers { get; set; }
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new();
        public List<LowStockVariantViewModel> LowStockVariantsList { get; set; } = new();
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<RevenuePointViewModel> RevenueChart { get; set; } = new();
        public List<OrderStatusPointViewModel> OrderStatusChart { get; set; } = new();
    }

    public class TopSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockVariantViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ProductVariantId { get; set; }
        public string SizeName { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }

    public class RevenuePointViewModel
    {
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderStatusPointViewModel
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
    }
}

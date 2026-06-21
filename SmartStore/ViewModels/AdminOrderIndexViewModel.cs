using SmartStore.Models;

namespace SmartStore.ViewModels
{
    public class AdminOrderIndexViewModel
    {
        public List<OrderListItemViewModel> Items { get; set; } = new();
        public string? Search { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal CompletedRevenue { get; set; }
    }
}

namespace SmartStore.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(item => item.LineTotal);
        public decimal ShippingFee => IsEmpty ? 0 : 30000;
        public decimal Discount => SubTotal >= 500000 ? 50000 : 0;
        public decimal Total => SubTotal + ShippingFee - Discount;
        public bool IsEmpty => Items.Count == 0;
    }
}

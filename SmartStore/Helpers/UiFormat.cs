using System.Globalization;

namespace SmartStore.Helpers
{
    public static class UiFormat
    {
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

        public static string Vnd(decimal value)
        {
            return $"{value.ToString("N0", VietnameseCulture)} ₫";
        }

        public static string StockStatus(int quantity)
        {
            return quantity > 0 ? "Còn hàng" : "Hết hàng";
        }
    }
}

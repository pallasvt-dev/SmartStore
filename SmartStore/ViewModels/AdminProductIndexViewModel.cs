using Microsoft.AspNetCore.Mvc.Rendering;
using SmartStore.Models;

namespace SmartStore.ViewModels
{
    public class AdminProductIndexViewModel
    {
        public List<Product> Products { get; set; } = new();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? StockStatus { get; set; }
        public SelectList? Categories { get; set; }
        public SelectList? Brands { get; set; }
    }
}

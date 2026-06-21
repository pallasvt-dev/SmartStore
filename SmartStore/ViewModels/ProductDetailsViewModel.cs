using SmartStore.Models;

namespace SmartStore.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<ProductReview> Reviews { get; set; } = new();
        public List<OutfitSuggestion> OutfitSuggestions { get; set; } = new();
        public bool IsFavorite { get; set; }
        public bool IsCustomer { get; set; }
        public bool HasPurchased { get; set; }
        public bool HasReviewed { get; set; }
        public bool CanReview => IsCustomer && HasPurchased && !HasReviewed;
    }
}

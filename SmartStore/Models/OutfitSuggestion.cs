using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class OutfitSuggestion
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int SuggestedProductId { get; set; }
        public Product SuggestedProduct { get; set; } = null!;

        [StringLength(120)]
        public string? Title { get; set; }

        [StringLength(300)]
        public string? Note { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

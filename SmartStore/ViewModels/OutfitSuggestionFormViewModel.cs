using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartStore.ViewModels
{
    public class OutfitSuggestionFormViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm chính")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm phối cùng")]
        public int SuggestedProductId { get; set; }

        [StringLength(120, ErrorMessage = "Tiêu đề không được vượt quá 120 ký tự")]
        public string? Title { get; set; }

        [StringLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự")]
        public string? Note { get; set; }

        [Range(0, 1000, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 1000")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public SelectList? Products { get; set; }
    }
}

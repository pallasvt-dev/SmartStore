using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartStore.Models
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm không được vượt quá 150 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(180, ErrorMessage = "Đường dẫn thân thiện không được vượt quá 180 ký tự")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Chất liệu không được vượt quá 100 ký tự")]
        public string? Material { get; set; }

        [StringLength(30, ErrorMessage = "Giới tính không được vượt quá 30 ký tự")]
        public string? Gender { get; set; }

        [Range(1000, 1000000000, ErrorMessage = "Giá bán phải từ 1.000 ₫ đến 1.000.000.000 ₫")]
        public decimal Price { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá gốc không được vượt quá 1.000.000.000 ₫")]
        public decimal? OldPrice { get; set; }

        [StringLength(30, ErrorMessage = "Nhãn hiển thị không được vượt quá 30 ký tự")]
        public string? Badge { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm ảnh chính")]
        public string MainImageUrl { get; set; } = string.Empty;

        public string? ExtraImageUrls { get; set; }

        public List<ProductVariantInputModel> Variants { get; set; } = new();

        public SelectList? Categories { get; set; }

        public SelectList? Brands { get; set; }

        public SelectList? Sizes { get; set; }

        public SelectList? Colors { get; set; }
    }

    public class ProductVariantInputModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn kích cỡ")]
        public int SizeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn màu sắc")]
        public int ColorId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập SKU")]
        [StringLength(80, ErrorMessage = "SKU không được vượt quá 80 ký tự")]
        public string Sku { get; set; } = string.Empty;

        [Range(0, 1000000000, ErrorMessage = "Giá riêng không được vượt quá 1.000.000.000 ₫")]
        public decimal? Price { get; set; }

        [Range(0, 1000000, ErrorMessage = "Số lượng tồn kho không được vượt quá 1.000.000")]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

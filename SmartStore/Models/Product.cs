using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class Product
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

        [Range(0, 5, ErrorMessage = "Đánh giá phải từ 0 đến 5")]
        public double Rating { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public int? BrandId { get; set; }

        public Brand? Brand { get; set; }

        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}

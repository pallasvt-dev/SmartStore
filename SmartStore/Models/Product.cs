using System.ComponentModel.DataAnnotations;

namespace SmartStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(120, ErrorMessage = "Tên sản phẩm tối đa 120 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập danh mục")]
        [StringLength(80, ErrorMessage = "Danh mục tối đa 80 ký tự")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Range(1000, 1000000000, ErrorMessage = "Giá bán phải từ 1,000 VND đến 1,000,000,000 VND")]
        public decimal Price { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá cũ không được vượt quá 1,000,000,000 VND")]
        public decimal? OldPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm hình ảnh sản phẩm")]
        [StringLength(6000000, ErrorMessage = "Dung lượng hình ảnh quá lớn")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "Nhãn hiển thị tối đa 30 ký tự")]
        public string Badge { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Đánh giá phải từ 0 đến 5")]
        public double Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trạng thái kho")]
        [StringLength(60, ErrorMessage = "Trạng thái kho tối đa 60 ký tự")]
        public string StockStatus { get; set; } = string.Empty;
    }
}

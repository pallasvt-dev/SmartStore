namespace SmartStore.Models
{
    public static class ProductRepository
    {
        public static readonly List<Product> Products = new()
        {
            new Product
            {
                Id = 1,
                Name = "Áo thun basic cotton",
                Category = "Thời trang",
                Description = "Áo thun chất cotton mềm, form dễ mặc, phù hợp đi học, đi chơi hoặc mặc hằng ngày.",
                Price = 99000,
                OldPrice = 139000,
                ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80",
                Badge = "Hot",
                Rating = 4.9,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 2,
                Name = "Giày sneaker trắng",
                Category = "Bán chạy",
                Description = "Giày sneaker phong cách tối giản, dễ phối đồ, đế êm và phù hợp sử dụng mỗi ngày.",
                Price = 359000,
                OldPrice = 429000,
                ImageUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?auto=format&fit=crop&w=900&q=80",
                Badge = "-16%",
                Rating = 4.8,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 3,
                Name = "Balo laptop chống nước",
                Category = "Tiện ích",
                Description = "Balo có ngăn laptop riêng, chất liệu chống thấm nhẹ, phù hợp sinh viên và dân văn phòng.",
                Price = 279000,
                OldPrice = 349000,
                ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?auto=format&fit=crop&w=900&q=80",
                Badge = "New",
                Rating = 4.7,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 4,
                Name = "Tai nghe Bluetooth Pro",
                Category = "Công nghệ",
                Description = "Tai nghe không dây nhỏ gọn, âm thanh rõ, hộp sạc tiện lợi và kết nối nhanh với điện thoại.",
                Price = 229000,
                OldPrice = 299000,
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=80",
                Badge = "Sale",
                Rating = 4.8,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 5,
                Name = "Đồng hồ thông minh FitWatch",
                Category = "Công nghệ",
                Description = "Đồng hồ theo dõi vận động, hiển thị thông báo, kiểu dáng hiện đại và dây đeo êm tay.",
                Price = 459000,
                OldPrice = 559000,
                ImageUrl = "https://images.unsplash.com/photo-1546868871-7041f2a55e12?auto=format&fit=crop&w=900&q=80",
                Badge = "Best",
                Rating = 5.0,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 6,
                Name = "Bình giữ nhiệt inox 750ml",
                Category = "Gia dụng",
                Description = "Bình giữ nhiệt dung tích lớn, nắp kín, dễ mang theo khi đi học, đi làm hoặc du lịch.",
                Price = 169000,
                OldPrice = 219000,
                ImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?auto=format&fit=crop&w=900&q=80",
                Badge = "Top",
                Rating = 4.9,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 7,
                Name = "Sổ tay planner 2026",
                Category = "Văn phòng phẩm",
                Description = "Sổ tay bìa cứng, giấy dày, bố cục ghi chú rõ ràng giúp quản lý kế hoạch cá nhân hiệu quả.",
                Price = 79000,
                OldPrice = 99000,
                ImageUrl = "https://images.unsplash.com/photo-1517842645767-c639042777db?auto=format&fit=crop&w=900&q=80",
                Badge = "New",
                Rating = 4.6,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 8,
                Name = "Ly thủy tinh nắp gỗ",
                Category = "Gia dụng",
                Description = "Ly thủy tinh trong suốt kèm nắp gỗ và ống hút, phù hợp pha trà, cà phê hoặc nước ép.",
                Price = 89000,
                OldPrice = 119000,
                ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=900&q=80",
                Badge = "Cute",
                Rating = 4.7,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 9,
                Name = "Túi tote canvas",
                Category = "Thời trang",
                Description = "Túi tote vải canvas dày, thiết kế đơn giản, đựng được sách vở, laptop mỏng và đồ cá nhân.",
                Price = 69000,
                OldPrice = 89000,
                ImageUrl = "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?auto=format&fit=crop&w=900&q=80",
                Badge = "Deal",
                Rating = 4.8,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 10,
                Name = "Kính mát thời trang",
                Category = "Phụ kiện",
                Description = "Kính mát kiểu dáng trẻ trung, tròng tối dễ phối đồ, phù hợp đi chơi và chụp ảnh ngoài trời.",
                Price = 129000,
                OldPrice = 169000,
                ImageUrl = "https://images.unsplash.com/photo-1511499767150-a48a237f0083?auto=format&fit=crop&w=900&q=80",
                Badge = "Style",
                Rating = 4.6,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 11,
                Name = "Nón lưỡi trai unisex",
                Category = "Phụ kiện",
                Description = "Nón lưỡi trai form gọn, có dây điều chỉnh phía sau, phù hợp cả nam và nữ.",
                Price = 59000,
                OldPrice = 79000,
                ImageUrl = "https://images.unsplash.com/photo-1521369909029-2afed882baee?auto=format&fit=crop&w=900&q=80",
                Badge = "Hot",
                Rating = 4.7,
                StockStatus = "Còn hàng"
            },
            new Product
            {
                Id = 12,
                Name = "Chuột không dây Ergonomic",
                Category = "Công nghệ",
                Description = "Chuột không dây thiết kế ôm tay, thao tác mượt, phù hợp học tập, làm việc và giải trí.",
                Price = 149000,
                OldPrice = 199000,
                ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=900&q=80",
                Badge = "Top",
                Rating = 4.9,
                StockStatus = "Còn hàng"
            }
        };

        public static List<Product> GetAll()
        {
            return Products.OrderByDescending(p => p.Id).ToList();
        }

        public static Product? GetById(int id)
        {
            return Products.FirstOrDefault(p => p.Id == id);
        }

        public static void Add(Product product)
        {
            product.Id = Products.Any() ? Products.Max(p => p.Id) + 1 : 1;
            product.Badge = string.IsNullOrWhiteSpace(product.Badge) ? "New" : product.Badge.Trim();
            product.ImageUrl = string.IsNullOrWhiteSpace(product.ImageUrl) ? "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80" : product.ImageUrl.Trim();
            product.StockStatus = string.IsNullOrWhiteSpace(product.StockStatus) ? "Còn hàng" : product.StockStatus.Trim();
            Products.Add(product);
        }

        public static bool Update(Product product)
        {
            var existingProduct = GetById(product.Id);
            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.Name = product.Name.Trim();
            existingProduct.Category = product.Category.Trim();
            existingProduct.Description = product.Description.Trim();
            existingProduct.Price = product.Price;
            existingProduct.OldPrice = product.OldPrice;
            existingProduct.ImageUrl = product.ImageUrl.Trim();
            existingProduct.Badge = string.IsNullOrWhiteSpace(product.Badge) ? "New" : product.Badge.Trim();
            existingProduct.Rating = product.Rating;
            existingProduct.StockStatus = product.StockStatus.Trim();
            return true;
        }

        public static bool Delete(int id)
        {
            var product = GetById(id);
            if (product == null)
            {
                return false;
            }

            Products.Remove(product);
            return true;
        }
    }
}

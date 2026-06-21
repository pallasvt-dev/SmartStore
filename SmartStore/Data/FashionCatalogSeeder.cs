using Microsoft.EntityFrameworkCore;
using SmartStore.Models;

namespace SmartStore.Data
{
    public static class FashionCatalogSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedCategoriesAsync(dbContext);
            await SeedBrandsAsync(dbContext);
            await SeedSizesAsync(dbContext);
            await SeedColorsAsync(dbContext);
            await SeedProductsAsync(dbContext);
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext dbContext)
        {
            var categories = new[]
            {
                new Category { Name = "Áo thun", Slug = "ao-thun" },
                new Category { Name = "Áo sơ mi", Slug = "ao-so-mi" },
                new Category { Name = "Quần jean", Slug = "quan-jean" },
                new Category { Name = "Váy", Slug = "vay" },
                new Category { Name = "Phụ kiện", Slug = "phu-kien" }
            };

            foreach (var category in categories)
            {
                if (!await dbContext.Categories.AnyAsync(item => item.Name == category.Name))
                {
                    dbContext.Categories.Add(category);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedBrandsAsync(ApplicationDbContext dbContext)
        {
            var brands = new[]
            {
                new Brand { Name = "SmartWear" },
                new Brand { Name = "UrbanStyle" },
                new Brand { Name = "BasicLife" }
            };

            foreach (var brand in brands)
            {
                if (!await dbContext.Brands.AnyAsync(item => item.Name == brand.Name))
                {
                    dbContext.Brands.Add(brand);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedSizesAsync(ApplicationDbContext dbContext)
        {
            var sizes = new[] { "S", "M", "L", "XL", "XXL" };

            for (var index = 0; index < sizes.Length; index++)
            {
                var name = sizes[index];
                if (!await dbContext.Sizes.AnyAsync(item => item.Name == name))
                {
                    dbContext.Sizes.Add(new Size { Name = name, DisplayOrder = index + 1 });
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedColorsAsync(ApplicationDbContext dbContext)
        {
            var colors = new[]
            {
                new Color { Name = "Đen", HexCode = "#000000" },
                new Color { Name = "Trắng", HexCode = "#FFFFFF" },
                new Color { Name = "Be", HexCode = "#D8C3A5" },
                new Color { Name = "Xanh navy", HexCode = "#1F2A44" },
                new Color { Name = "Hồng", HexCode = "#F8BBD0" }
            };

            foreach (var color in colors)
            {
                if (!await dbContext.Colors.AnyAsync(item => item.Name == color.Name))
                {
                    dbContext.Colors.Add(color);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Products.AnyAsync())
            {
                return;
            }

            var categories = await dbContext.Categories.ToDictionaryAsync(category => category.Name);
            var brands = await dbContext.Brands.ToDictionaryAsync(brand => brand.Name);
            var sizes = await dbContext.Sizes.ToDictionaryAsync(size => size.Name);
            var colors = await dbContext.Colors.ToDictionaryAsync(color => color.Name);

            var products = new[]
            {
                CreateProduct(
                    "Áo thun basic cotton",
                    "ao-thun-basic-cotton",
                    "Áo thun cotton mềm, form dễ mặc, phù hợp đi học, đi chơi hoặc mặc hằng ngày.",
                    "Cotton",
                    "Unisex",
                    99000,
                    139000,
                    "Hot",
                    4.9,
                    categories["Áo thun"],
                    brands["BasicLife"],
                    "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80",
                    new[] { "Trắng", "Đen" },
                    new[] { "M", "L" },
                    "AOTHUN-BASIC"),
                CreateProduct(
                    "Áo sơ mi linen relaxed",
                    "ao-so-mi-linen-relaxed",
                    "Áo sơ mi linen thoáng nhẹ, có thể mặc riêng hoặc khoác ngoài cho phong cách thanh lịch.",
                    "Linen",
                    "Unisex",
                    249000,
                    319000,
                    "New",
                    4.7,
                    categories["Áo sơ mi"],
                    brands["SmartWear"],
                    "https://images.unsplash.com/photo-1598032895397-b9472444bf93?auto=format&fit=crop&w=900&q=80",
                    new[] { "Trắng", "Be" },
                    new[] { "M", "XL" },
                    "SOMI-LINEN"),
                CreateProduct(
                    "Quần jean slim xanh navy",
                    "quan-jean-slim-xanh-navy",
                    "Quần jean slim fit co giãn nhẹ, dễ phối với áo thun và sneaker.",
                    "Denim",
                    "Nam",
                    399000,
                    499000,
                    "-20%",
                    4.8,
                    categories["Quần jean"],
                    brands["UrbanStyle"],
                    "https://images.unsplash.com/photo-1542272604-787c3835535d?auto=format&fit=crop&w=900&q=80",
                    new[] { "Xanh navy", "Đen" },
                    new[] { "L", "XL" },
                    "JEAN-SLIM"),
                CreateProduct(
                    "Váy midi họa tiết nhẹ",
                    "vay-midi-hoa-tiet-nhe",
                    "Váy midi dáng xòe nhẹ, phù hợp đi làm, dạo phố và các buổi hẹn cuối tuần.",
                    "Polyester",
                    "Nu",
                    329000,
                    429000,
                    "Best",
                    4.9,
                    categories["Váy"],
                    brands["SmartWear"],
                    "https://images.unsplash.com/photo-1595777457583-95e059d581b8?auto=format&fit=crop&w=900&q=80",
                    new[] { "Hồng", "Be" },
                    new[] { "S", "M" },
                    "VAY-MIDI"),
                CreateProduct(
                    "Áo khoác cardigan mỏng",
                    "ao-khoac-cardigan-mong",
                    "Cardigan mỏng, mềm và dễ layer với áo thun hoặc áo hai dây.",
                    "Cotton blend",
                    "Nu",
                    289000,
                    359000,
                    "Sale",
                    4.6,
                    categories["Áo thun"],
                    brands["UrbanStyle"],
                    "https://images.unsplash.com/photo-1434389677669-e08b4cac3105?auto=format&fit=crop&w=900&q=80",
                    new[] { "Be", "Trắng" },
                    new[] { "M", "L" },
                    "CARDIGAN-MONG"),
                CreateProduct(
                    "Túi tote canvas daily",
                    "tui-tote-canvas-daily",
                    "Túi tote vải canvas dày, đựng được sách vở, laptop mỏng và đồ cá nhân.",
                    "Canvas",
                    "Unisex",
                    129000,
                    169000,
                    "Top",
                    4.8,
                    categories["Phụ kiện"],
                    brands["BasicLife"],
                    "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?auto=format&fit=crop&w=900&q=80",
                    new[] { "Be", "Đen" },
                    new[] { "S", "M" },
                    "TOTE-CANVAS")
            };

            dbContext.Products.AddRange(products);
            await dbContext.SaveChangesAsync();

            Product CreateProduct(
                string name,
                string slug,
                string description,
                string material,
                string gender,
                decimal price,
                decimal oldPrice,
                string badge,
                double rating,
                Category category,
                Brand brand,
                string mainImage,
                string[] colorNames,
                string[] sizeNames,
                string skuPrefix)
            {
                var product = new Product
                {
                    Name = name,
                    Slug = slug,
                    Description = description,
                    Material = material,
                    Gender = gender,
                    Price = price,
                    OldPrice = oldPrice,
                    Badge = badge,
                    Rating = rating,
                    Category = category,
                    Brand = brand,
                    ProductImages =
                    {
                        new ProductImage { ImageUrl = mainImage, IsMain = true, DisplayOrder = 1 }
                    }
                };

                var stockSeed = 8;
                foreach (var colorName in colorNames)
                {
                    foreach (var sizeName in sizeNames)
                    {
                        product.ProductVariants.Add(new ProductVariant
                        {
                            Size = sizes[sizeName],
                            Color = colors[colorName],
                            Sku = $"{skuPrefix}-{ToSkuToken(colorName)}-{sizeName}",
                            StockQuantity = stockSeed++,
                            IsActive = true
                        });
                    }
                }

                return product;
            }
        }

        private static string ToSkuToken(string value)
        {
            return value
                .Replace("Đ", "D", StringComparison.OrdinalIgnoreCase)
                .Replace("Ắ", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Ắ", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Ắ", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Ă", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Â", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Á", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("À", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Ả", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Ã", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("Ạ", "A", StringComparison.OrdinalIgnoreCase)
                .Replace("É", "E", StringComparison.OrdinalIgnoreCase)
                .Replace("È", "E", StringComparison.OrdinalIgnoreCase)
                .Replace("Ẻ", "E", StringComparison.OrdinalIgnoreCase)
                .Replace("Ẽ", "E", StringComparison.OrdinalIgnoreCase)
                .Replace("Ẹ", "E", StringComparison.OrdinalIgnoreCase)
                .Replace("Ê", "E", StringComparison.OrdinalIgnoreCase)
                .Replace("Í", "I", StringComparison.OrdinalIgnoreCase)
                .Replace("Ì", "I", StringComparison.OrdinalIgnoreCase)
                .Replace("Ỉ", "I", StringComparison.OrdinalIgnoreCase)
                .Replace("Ĩ", "I", StringComparison.OrdinalIgnoreCase)
                .Replace("Ị", "I", StringComparison.OrdinalIgnoreCase)
                .Replace("Ó", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Ò", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Ỏ", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Õ", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Ọ", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Ô", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Ơ", "O", StringComparison.OrdinalIgnoreCase)
                .Replace("Ú", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("Ù", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("Ủ", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("Ũ", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("Ụ", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("Ư", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("Ý", "Y", StringComparison.OrdinalIgnoreCase)
                .Replace("Ỳ", "Y", StringComparison.OrdinalIgnoreCase)
                .Replace("Ỷ", "Y", StringComparison.OrdinalIgnoreCase)
                .Replace("Ỹ", "Y", StringComparison.OrdinalIgnoreCase)
                .Replace("Ỵ", "Y", StringComparison.OrdinalIgnoreCase)
                .Replace(" ", "-", StringComparison.Ordinal)
                .Replace("XANH-NAVY", "XANH", StringComparison.OrdinalIgnoreCase)
                .ToUpperInvariant();
        }
    }
}

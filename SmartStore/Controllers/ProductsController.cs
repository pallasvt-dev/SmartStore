using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;

namespace SmartStore.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var products = await ProductQuery()
                .Where(product => product.IsActive)
                .OrderByDescending(product => product.Id)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await ProductDetailsQuery()
                .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ProductFormViewModel
            {
                MainImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80",
                Badge = "New",
                Rating = 4.8,
                Variants =
                {
                    new ProductVariantInputModel { StockQuantity = 5 }
                }
            };

            await LoadSelectListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            NormalizeForm(model);
            await ValidateVariantsAsync(model);

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(model);
                return View(model);
            }

            var product = new Product();
            ApplyProductFields(product, model, isNew: true);
            ApplyImages(product, model);
            ApplyVariants(product, model);

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            TempData["ProductMessage"] = "Đã thêm sản phẩm thời trang thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await ProductDetailsQuery()
                .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            var model = ToFormModel(product);
            await LoadSelectListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            NormalizeForm(model);
            await ValidateVariantsAsync(model, model.Id);

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(model);
                return View(model);
            }

            var product = await _dbContext.Products
                .Include(item => item.ProductImages)
                .Include(item => item.ProductVariants)
                .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            ApplyProductFields(product, model, isNew: false);

            _dbContext.ProductImages.RemoveRange(product.ProductImages);
            _dbContext.ProductVariants.RemoveRange(product.ProductVariants);
            await _dbContext.SaveChangesAsync();
            product.ProductImages.Clear();
            product.ProductVariants.Clear();

            ApplyImages(product, model);
            ApplyVariants(product, model);
            await _dbContext.SaveChangesAsync();

            TempData["ProductMessage"] = "Đã cập nhật sản phẩm thời trang thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await ProductDetailsQuery()
                .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _dbContext.Products
                .Include(item => item.ProductVariants)
                .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            foreach (var variant in product.ProductVariants)
            {
                variant.IsActive = false;
            }

            await _dbContext.SaveChangesAsync();

            TempData["ProductMessage"] = "Đã ngừng bán sản phẩm khỏi cửa hàng.";
            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Product> ProductQuery()
        {
            return _dbContext.Products
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Include(product => product.ProductImages)
                .Include(product => product.ProductVariants);
        }

        private IQueryable<Product> ProductDetailsQuery()
        {
            return _dbContext.Products
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Include(product => product.ProductImages)
                .Include(product => product.ProductVariants)
                    .ThenInclude(variant => variant.Size)
                .Include(product => product.ProductVariants)
                    .ThenInclude(variant => variant.Color);
        }

        private async Task LoadSelectListsAsync(ProductFormViewModel model)
        {
            var categories = await _dbContext.Categories
                .Where(category => category.IsActive)
                .OrderBy(category => category.Name)
                .ToListAsync();
            var brands = await _dbContext.Brands
                .Where(brand => brand.IsActive)
                .OrderBy(brand => brand.Name)
                .ToListAsync();
            var sizes = await _dbContext.Sizes
                .Where(size => size.IsActive)
                .OrderBy(size => size.DisplayOrder)
                .ToListAsync();
            var colors = await _dbContext.Colors
                .Where(color => color.IsActive)
                .OrderBy(color => color.Name)
                .ToListAsync();

            model.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
            model.Brands = new SelectList(brands, "Id", "Name", model.BrandId);
            model.Sizes = new SelectList(sizes, "Id", "Name");
            model.Colors = new SelectList(colors, "Id", "Name");
        }

        private static ProductFormViewModel ToFormModel(Product product)
        {
            var orderedImages = product.ProductImages
                .OrderByDescending(image => image.IsMain)
                .ThenBy(image => image.DisplayOrder)
                .ToList();
            var mainImage = orderedImages.FirstOrDefault(image => image.IsMain) ?? orderedImages.FirstOrDefault();
            var extraImages = orderedImages
                .Where(image => image.Id != mainImage?.Id)
                .Select(image => image.ImageUrl);

            return new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Material = product.Material,
                Gender = product.Gender,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Badge = product.Badge,
                Rating = product.Rating,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                MainImageUrl = mainImage?.ImageUrl ?? string.Empty,
                ExtraImageUrls = string.Join(Environment.NewLine, extraImages),
                Variants = product.ProductVariants
                    .OrderBy(variant => variant.Size.DisplayOrder)
                    .ThenBy(variant => variant.Color.Name)
                    .Select(variant => new ProductVariantInputModel
                    {
                        Id = variant.Id,
                        SizeId = variant.SizeId,
                        ColorId = variant.ColorId,
                        Sku = variant.Sku,
                        Price = variant.Price,
                        StockQuantity = variant.StockQuantity,
                        IsActive = variant.IsActive
                    })
                    .ToList()
            };
        }

        private static void ApplyProductFields(Product product, ProductFormViewModel model, bool isNew)
        {
            product.Name = model.Name.Trim();
            product.Slug = string.IsNullOrWhiteSpace(model.Slug) ? ToSlug(model.Name) : model.Slug.Trim();
            product.Description = model.Description.Trim();
            product.Material = model.Material?.Trim();
            product.Gender = model.Gender?.Trim();
            product.Price = model.Price;
            product.OldPrice = model.OldPrice;
            product.Badge = string.IsNullOrWhiteSpace(model.Badge) ? null : model.Badge.Trim();
            product.Rating = model.Rating;
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;
            product.IsActive = true;

            if (isNew)
            {
                product.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static void ApplyImages(Product product, ProductFormViewModel model)
        {
            product.ProductImages.Add(new ProductImage
            {
                ImageUrl = model.MainImageUrl.Trim(),
                IsMain = true,
                DisplayOrder = 1
            });

            var displayOrder = 2;
            foreach (var imageUrl in SplitExtraImages(model.ExtraImageUrls))
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImageUrl = imageUrl,
                    IsMain = false,
                    DisplayOrder = displayOrder++
                });
            }
        }

        private static void ApplyVariants(Product product, ProductFormViewModel model)
        {
            foreach (var variant in model.Variants.Where(IsUsableVariant))
            {
                product.ProductVariants.Add(new ProductVariant
                {
                    SizeId = variant.SizeId,
                    ColorId = variant.ColorId,
                    Sku = variant.Sku.Trim().ToUpperInvariant(),
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity,
                    IsActive = variant.IsActive,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private async Task ValidateVariantsAsync(ProductFormViewModel model, int? productId = null)
        {
            model.Variants = model.Variants.Where(IsUsableVariant).ToList();

            if (!model.Variants.Any())
            {
                ModelState.AddModelError(nameof(model.Variants), "Cần thêm ít nhất một biến thể sản phẩm.");
                return;
            }

            var duplicateSkus = model.Variants
                .Select(variant => variant.Sku.Trim().ToUpperInvariant())
                .GroupBy(sku => sku)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var sku in duplicateSkus)
            {
                ModelState.AddModelError(nameof(model.Variants), $"SKU bị trùng trong form: {sku}");
            }

            var postedSkus = model.Variants
                .Select(variant => variant.Sku.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            var skuExists = await _dbContext.ProductVariants
                .AnyAsync(variant => postedSkus.Contains(variant.Sku) && (!productId.HasValue || variant.ProductId != productId.Value));

            if (skuExists)
            {
                ModelState.AddModelError(nameof(model.Variants), "Một hoặc nhiều SKU đã tồn tại ở sản phẩm khác.");
            }
        }

        private static void NormalizeForm(ProductFormViewModel model)
        {
            model.Name = model.Name?.Trim() ?? string.Empty;
            model.Slug = model.Slug?.Trim() ?? string.Empty;
            model.Description = model.Description?.Trim() ?? string.Empty;
            model.MainImageUrl = model.MainImageUrl?.Trim() ?? string.Empty;
            model.Variants ??= new List<ProductVariantInputModel>();

            foreach (var variant in model.Variants)
            {
                variant.Sku = variant.Sku?.Trim().ToUpperInvariant() ?? string.Empty;
            }
        }

        private static bool IsUsableVariant(ProductVariantInputModel variant)
        {
            return variant.SizeId > 0
                || variant.ColorId > 0
                || !string.IsNullOrWhiteSpace(variant.Sku)
                || variant.StockQuantity > 0
                || variant.Price.HasValue;
        }

        private static IEnumerable<string> SplitExtraImages(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }

            return value
                .Split(new[] { "\r\n", "\n", ",", ";" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(item => !string.IsNullOrWhiteSpace(item));
        }

        private static string ToSlug(string value)
        {
            var chars = value.Trim().ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray();

            return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

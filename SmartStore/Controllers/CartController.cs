using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Helpers;
using SmartStore.Models;
using SmartStore.ViewModels;

namespace SmartStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const int MaxCartQuantity = 20;
        private const string DefaultImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80";
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await BuildCartViewModelAsync(GetUserId()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productVariantId, int quantity = 1, string? returnUrl = null)
        {
            var userId = GetUserId();
            var variant = await _dbContext.ProductVariants
                .Include(item => item.Product)
                    .ThenInclude(product => product.ProductImages)
                .Include(item => item.Size)
                .Include(item => item.Color)
                .FirstOrDefaultAsync(item => item.Id == productVariantId && item.IsActive && item.Product.IsActive);

            if (variant == null)
            {
                return await CartResponseAsync(userId, null, "Sản phẩm không tồn tại hoặc đã ngừng bán.", returnUrl, StatusCodes.Status404NotFound);
            }

            if (variant.StockQuantity <= 0)
            {
                return await CartResponseAsync(userId, null, "Biến thể này đang hết hàng.", returnUrl);
            }

            var cart = await _dbContext.ShoppingCarts
                .Include(item => item.CartItems)
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _dbContext.ShoppingCarts.Add(cart);
            }

            var requestedQuantity = Math.Clamp(quantity, 1, MaxCartQuantity);
            var currentCartQuantity = cart.CartItems.Sum(item => item.Quantity);
            var cartCapacity = MaxCartQuantity - currentCartQuantity;
            var existingItem = cart.CartItems.FirstOrDefault(item => item.ProductVariantId == productVariantId);
            var currentItemQuantity = existingItem?.Quantity ?? 0;
            var stockCapacity = variant.StockQuantity - currentItemQuantity;
            var quantityToAdd = Math.Min(requestedQuantity, Math.Min(cartCapacity, stockCapacity));

            if (quantityToAdd <= 0)
            {
                var message = stockCapacity <= 0
                    ? "Số lượng vượt quá tồn kho hiện có."
                    : $"Giỏ hàng chỉ được chứa tối đa {MaxCartQuantity} sản phẩm.";
                return await CartResponseAsync(userId, existingItem?.Id, message, returnUrl);
            }

            if (existingItem == null)
            {
                existingItem = new CartItemEntity
                {
                    ProductVariantId = variant.Id,
                    Quantity = quantityToAdd,
                    UnitPrice = variant.Price ?? variant.Product.Price
                };
                cart.CartItems.Add(existingItem);
            }
            else
            {
                existingItem.Quantity += quantityToAdd;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var successMessage = quantityToAdd < requestedQuantity
                ? "Đã thêm sản phẩm theo giới hạn tồn kho hoặc sức chứa của giỏ hàng."
                : "Đã thêm sản phẩm vào giỏ hàng.";
            return await CartResponseAsync(userId, existingItem.Id, successMessage, returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            var item = await FindOwnedCartItemAsync(userId, cartItemId);
            if (item == null)
            {
                return await CartResponseAsync(userId, cartItemId, "Sản phẩm không còn trong giỏ hàng.");
            }

            if (quantity <= 0)
            {
                return await RemoveCartItemAsync(item, userId, "Đã xóa sản phẩm khỏi giỏ hàng.");
            }

            var otherItemsQuantity = await _dbContext.CartItemEntities
                .Where(other => other.ShoppingCart.UserId == userId && other.Id != cartItemId)
                .SumAsync(other => other.Quantity);
            var maxAllowed = Math.Min(item.ProductVariant.StockQuantity, MaxCartQuantity - otherItemsQuantity);

            if (quantity > maxAllowed)
            {
                var message = quantity > item.ProductVariant.StockQuantity
                    ? "Số lượng vượt quá tồn kho hiện có."
                    : $"Giỏ hàng chỉ được chứa tối đa {MaxCartQuantity} sản phẩm.";
                return await CartResponseAsync(userId, cartItemId, message);
            }

            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
            item.ShoppingCart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return await CartResponseAsync(userId, cartItemId, "Đã cập nhật số lượng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Increase(int cartItemId)
        {
            var userId = GetUserId();
            var item = await FindOwnedCartItemAsync(userId, cartItemId);
            if (item == null)
            {
                return await CartResponseAsync(userId, cartItemId, "Sản phẩm không còn trong giỏ hàng.");
            }

            var cartQuantity = await _dbContext.CartItemEntities
                .Where(cartItem => cartItem.ShoppingCart.UserId == userId)
                .SumAsync(cartItem => cartItem.Quantity);

            if (item.Quantity >= item.ProductVariant.StockQuantity)
            {
                return await CartResponseAsync(userId, cartItemId, "Số lượng vượt quá tồn kho hiện có.");
            }

            if (cartQuantity >= MaxCartQuantity)
            {
                return await CartResponseAsync(userId, cartItemId, $"Giỏ hàng chỉ được chứa tối đa {MaxCartQuantity} sản phẩm.");
            }

            item.Quantity++;
            item.UpdatedAt = DateTime.UtcNow;
            item.ShoppingCart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return await CartResponseAsync(userId, cartItemId, "Đã cập nhật số lượng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrease(int cartItemId)
        {
            var userId = GetUserId();
            var item = await FindOwnedCartItemAsync(userId, cartItemId);
            if (item == null)
            {
                return await CartResponseAsync(userId, cartItemId, "Sản phẩm không còn trong giỏ hàng.");
            }

            if (item.Quantity <= 1)
            {
                return await RemoveCartItemAsync(item, userId, "Đã xóa sản phẩm khỏi giỏ hàng.");
            }

            item.Quantity--;
            item.UpdatedAt = DateTime.UtcNow;
            item.ShoppingCart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return await CartResponseAsync(userId, cartItemId, "Đã cập nhật số lượng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = GetUserId();
            var item = await FindOwnedCartItemAsync(userId, cartItemId);
            return item == null
                ? await CartResponseAsync(userId, cartItemId, "Sản phẩm không còn trong giỏ hàng.")
                : await RemoveCartItemAsync(item, userId, "Đã xóa sản phẩm khỏi giỏ hàng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            var items = await _dbContext.CartItemEntities
                .Where(item => item.ShoppingCart.UserId == userId)
                .ToListAsync();

            _dbContext.CartItemEntities.RemoveRange(items);
            var cart = await _dbContext.ShoppingCarts.FirstOrDefaultAsync(item => item.UserId == userId);
            if (cart != null)
            {
                cart.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            TempData["CartMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<CartItemEntity?> FindOwnedCartItemAsync(string userId, int cartItemId)
        {
            return await _dbContext.CartItemEntities
                .Include(item => item.ShoppingCart)
                .Include(item => item.ProductVariant)
                .FirstOrDefaultAsync(item => item.Id == cartItemId && item.ShoppingCart.UserId == userId);
        }

        private async Task<IActionResult> RemoveCartItemAsync(CartItemEntity item, string userId, string message)
        {
            item.ShoppingCart.UpdatedAt = DateTime.UtcNow;
            _dbContext.CartItemEntities.Remove(item);
            await _dbContext.SaveChangesAsync();
            return await CartResponseAsync(userId, item.Id, message);
        }

        private async Task<CartViewModel> BuildCartViewModelAsync(string userId)
        {
            var cart = await _dbContext.ShoppingCarts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(item => item.CartItems)
                    .ThenInclude(item => item.ProductVariant)
                        .ThenInclude(variant => variant.Product)
                            .ThenInclude(product => product.ProductImages)
                .Include(item => item.CartItems)
                    .ThenInclude(item => item.ProductVariant)
                        .ThenInclude(variant => variant.Size)
                .Include(item => item.CartItems)
                    .ThenInclude(item => item.ProductVariant)
                        .ThenInclude(variant => variant.Color)
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (cart == null)
            {
                return new CartViewModel();
            }

            return new CartViewModel
            {
                Items = cart.CartItems
                    .OrderByDescending(item => item.CreatedAt)
                    .Select(item => new CartItemViewModel
                    {
                        CartItemId = item.Id,
                        ProductId = item.ProductVariant.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ProductName = item.ProductVariant.Product.Name,
                        ImageUrl = GetMainImageUrl(item.ProductVariant.Product),
                        SizeName = item.ProductVariant.Size.Name,
                        ColorName = item.ProductVariant.Color.Name,
                        Sku = item.ProductVariant.Sku,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        StockQuantity = item.ProductVariant.StockQuantity
                    })
                    .ToList()
            };
        }

        private async Task<IActionResult> CartResponseAsync(
            string userId,
            int? changedCartItemId,
            string message,
            string? returnUrl = null,
            int statusCode = StatusCodes.Status200OK)
        {
            if (!IsAjaxRequest())
            {
                TempData["CartMessage"] = message;
                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                return RedirectToLocalCartTarget(returnUrl);
            }

            var model = await BuildCartViewModelAsync(userId);
            var changedItem = changedCartItemId.HasValue
                ? model.Items.FirstOrDefault(item => item.CartItemId == changedCartItemId.Value)
                : null;

            Response.StatusCode = statusCode;
            return Json(new
            {
                isEmpty = model.IsEmpty,
                count = model.Items.Sum(item => item.Quantity),
                changedCartItemId,
                message,
                item = changedItem == null ? null : new
                {
                    quantity = changedItem.Quantity,
                    lineTotal = UiFormat.Vnd(changedItem.LineTotal)
                },
                summary = new
                {
                    subTotal = UiFormat.Vnd(model.SubTotal),
                    shippingFee = UiFormat.Vnd(model.ShippingFee),
                    discount = $"-{UiFormat.Vnd(model.Discount)}",
                    total = UiFormat.Vnd(model.Total)
                }
            });
        }

        private string GetUserId()
        {
            return _userManager.GetUserId(User)
                ?? throw new InvalidOperationException("Không xác định được tài khoản đăng nhập.");
        }

        private IActionResult RedirectToLocalCartTarget(string? returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private static string GetMainImageUrl(Product product)
        {
            return product.ProductImages
                .OrderByDescending(image => image.IsMain)
                .ThenBy(image => image.DisplayOrder)
                .Select(image => image.ImageUrl)
                .FirstOrDefault() ?? DefaultImageUrl;
        }
    }
}

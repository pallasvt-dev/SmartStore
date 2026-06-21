using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Extensions;
using SmartStore.Models;
using System.Globalization;

namespace SmartStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const string CartSessionKey = "SHOPPING_CART";
        private const int MaxCartQuantity = 5;
        private readonly ApplicationDbContext _dbContext;

        public CartController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View(GetCart());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productVariantId, int quantity = 1, string? returnUrl = null)
        {
            var variant = await _dbContext.ProductVariants
                .Include(item => item.Product)
                    .ThenInclude(product => product.ProductImages)
                .Include(item => item.Size)
                .Include(item => item.Color)
                .FirstOrDefaultAsync(item => item.Id == productVariantId && item.IsActive && item.Product.IsActive);

            if (variant == null)
            {
                return NotFound();
            }

            if (variant.StockQuantity <= 0)
            {
                const string outOfStockMessage = "Biến thể này đang hết hàng.";
                TempData["CartMessage"] = outOfStockMessage;
                return IsAjaxRequest()
                    ? Json(new { message = outOfStockMessage, count = GetCartQuantity(GetCart()) })
                    : RedirectToLocalCartTarget(returnUrl);
            }

            var cart = GetCart();
            var currentTotal = GetCartQuantity(cart);
            var availableQuantity = MaxCartQuantity - currentTotal;
            var requestedQuantity = Math.Clamp(quantity, 1, MaxCartQuantity);

            string message;
            if (availableQuantity <= 0)
            {
                message = $"Giỏ hàng chỉ được chứa tối đa {MaxCartQuantity} sản phẩm.";
                TempData["CartMessage"] = message;

                if (IsAjaxRequest())
                {
                    return Json(new { message, count = currentTotal });
                }

                return RedirectToLocalCartTarget(returnUrl);
            }

            var quantityToAdd = Math.Min(Math.Min(requestedQuantity, availableQuantity), variant.StockQuantity);
            var item = cart.FirstOrDefault(i => i.ProductVariantId == productVariantId);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    SizeName = variant.Size.Name,
                    ColorName = variant.Color.Name,
                    Sku = variant.Sku,
                    Price = variant.Price ?? variant.Product.Price,
                    ImageUrl = GetMainImageUrl(variant.Product),
                    Quantity = quantityToAdd
                });
            }
            else
            {
                item.Quantity += quantityToAdd;
            }

            SaveCart(cart);
            message = quantityToAdd < requestedQuantity
                ? $"Đã thêm {quantityToAdd} sản phẩm. Giỏ hàng đã đạt giới hạn hoặc tồn kho hiện có."
                : $"Đã thêm {variant.Product.Name} ({variant.Size.Name}/{variant.Color.Name}) vào giỏ hàng.";
            TempData["CartMessage"] = message;

            if (IsAjaxRequest())
            {
                return Json(new { message, count = GetCartQuantity(cart) });
            }

            return RedirectToLocalCartTarget(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductVariantId == id);
            var message = string.Empty;

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                    message = "Đã xóa sản phẩm khỏi giỏ hàng.";
                }
                else
                {
                    var otherItemsTotal = cart.Where(cartItem => cartItem.ProductVariantId != id).Sum(cartItem => cartItem.Quantity);
                    var maxAllowedForItem = Math.Max(MaxCartQuantity - otherItemsTotal, 0);
                    item.Quantity = Math.Min(quantity, maxAllowedForItem);

                    if (item.Quantity <= 0)
                    {
                        cart.Remove(item);
                        message = $"Giỏ hàng đã đạt giới hạn {MaxCartQuantity} sản phẩm.";
                    }
                    else
                    {
                        message = quantity > maxAllowedForItem
                            ? $"Số lượng đã được giới hạn để tổng giỏ hàng không vượt {MaxCartQuantity} sản phẩm."
                            : "Đã cập nhật số lượng sản phẩm.";
                    }
                }

                SaveCart(cart);
                TempData["CartMessage"] = message;
            }

            if (IsAjaxRequest())
            {
                return CartJson(cart, id, message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Increase(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductVariantId == id);
            var message = string.Empty;

            if (item != null)
            {
                if (GetCartQuantity(cart) >= MaxCartQuantity)
                {
                    message = $"Giỏ hàng chỉ được chứa tối đa {MaxCartQuantity} sản phẩm.";
                }
                else
                {
                    item.Quantity++;
                    SaveCart(cart);
                    message = "Đã tăng số lượng sản phẩm.";
                }

                TempData["CartMessage"] = message;
            }

            if (IsAjaxRequest())
            {
                return CartJson(cart, id, message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrease(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductVariantId == id);

            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }

                SaveCart(cart);
            }

            if (IsAjaxRequest())
            {
                return CartJson(cart, id);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductVariantId == id);
            var message = string.Empty;

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                message = "Đã xóa sản phẩm khỏi giỏ hàng.";
                TempData["CartMessage"] = message;
            }

            if (IsAjaxRequest())
            {
                return CartJson(cart, id, message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            TempData["CartMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetJson(CartSessionKey, cart);
        }

        private IActionResult RedirectToLocalCartTarget(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private static int GetCartQuantity(List<CartItem> cart)
        {
            return cart.Sum(item => item.Quantity);
        }

        private JsonResult CartJson(List<CartItem> cart, int changedVariantId, string? message = null)
        {
            var subTotal = cart.Sum(item => item.LineTotal);
            var shippingFee = subTotal > 0 ? 30000 : 0;
            var discount = subTotal >= 500000 ? 50000 : 0;
            var total = subTotal + shippingFee - discount;
            var changedItem = cart.FirstOrDefault(item => item.ProductVariantId == changedVariantId);

            return Json(new
            {
                isEmpty = !cart.Any(),
                count = GetCartQuantity(cart),
                changedProductId = changedVariantId,
                message,
                item = changedItem == null ? null : new
                {
                    quantity = changedItem.Quantity,
                    lineTotal = FormatCurrency(changedItem.LineTotal)
                },
                summary = new
                {
                    subTotal = FormatCurrency(subTotal),
                    shippingFee = FormatCurrency(shippingFee),
                    discount = $"-{FormatCurrency(discount)}",
                    total = FormatCurrency(total)
                }
            });
        }

        private static string GetMainImageUrl(Product product)
        {
            return product.ProductImages
                .OrderByDescending(image => image.IsMain)
                .ThenBy(image => image.DisplayOrder)
                .Select(image => image.ImageUrl)
                .FirstOrDefault()
                ?? "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80";
        }

        private static string FormatCurrency(decimal value)
        {
            return $"{value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} ₫";
        }
    }
}

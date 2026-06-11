using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using SmartStore.Extensions;
using SmartStore.Models;

namespace SmartStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const string CartSessionKey = "SHOPPING_CART";
        private const int MaxCartQuantity = 5;

        public IActionResult Index()
        {
            return View(GetCart());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int id, int quantity = 1, string? returnUrl = null)
        {
            var product = ProductRepository.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
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

            var quantityToAdd = Math.Min(requestedQuantity, availableQuantity);
            var item = cart.FirstOrDefault(i => i.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantityToAdd
                });
            }
            else
            {
                item.Quantity += quantityToAdd;
            }

            SaveCart(cart);
            message = quantityToAdd < requestedQuantity
                ? $"Đã thêm {quantityToAdd} sản phẩm. Giỏ hàng đã đạt giới hạn {MaxCartQuantity} sản phẩm."
                : $"Đã thêm {product.Name} vào giỏ hàng.";
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
            var item = cart.FirstOrDefault(i => i.ProductId == id);
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
                    var otherItemsTotal = cart.Where(cartItem => cartItem.ProductId != id).Sum(cartItem => cartItem.Quantity);
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
            var item = cart.FirstOrDefault(i => i.ProductId == id);
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
            var item = cart.FirstOrDefault(i => i.ProductId == id);

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
            var item = cart.FirstOrDefault(i => i.ProductId == id);
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

        private JsonResult CartJson(List<CartItem> cart, int changedProductId, string? message = null)
        {
            var subTotal = cart.Sum(item => item.LineTotal);
            var shippingFee = subTotal > 0 ? 30000 : 0;
            var discount = subTotal >= 500000 ? 50000 : 0;
            var total = subTotal + shippingFee - discount;
            var changedItem = cart.FirstOrDefault(item => item.ProductId == changedProductId);

            return Json(new
            {
                isEmpty = !cart.Any(),
                count = GetCartQuantity(cart),
                changedProductId,
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

        private static string FormatCurrency(decimal value)
        {
            return $"{value.ToString("N0", CultureInfo.InvariantCulture)} đ";
        }
    }
}

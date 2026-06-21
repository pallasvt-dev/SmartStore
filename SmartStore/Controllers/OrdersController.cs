using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;
using SmartStore.ViewModels;
using System.Data;
using System.Security.Cryptography;

namespace SmartStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private const string DefaultImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80";
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<OrdersController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var cart = await LoadCartAsync(userId, tracking: false);
            if (cart == null || cart.CartItems.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var model = BuildCheckoutViewModel(cart);
            model.CustomerName = user?.FullName ?? string.Empty;
            model.PhoneNumber = user?.PhoneNumber ?? string.Empty;
            model.Email = user?.Email;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = GetUserId();
            if (!Enum.IsDefined(model.PaymentMethod))
            {
                ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                return await CheckoutValidationViewAsync(userId, model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var cart = await LoadCartAsync(userId, tracking: true);
                if (cart == null || cart.CartItems.Count == 0)
                {
                    await transaction.RollbackAsync();
                    TempData["CartMessage"] = "Giỏ hàng đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
                    return RedirectToAction("Index", "Cart");
                }

                foreach (var cartItem in cart.CartItems)
                {
                    var variant = cartItem.ProductVariant;
                    if (!variant.IsActive || !variant.Product.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, $"Sản phẩm {variant.Product.Name} đã ngừng bán.");
                        continue;
                    }

                    if (variant.StockQuantity < cartItem.Quantity)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            $"{variant.Product.Name} ({variant.Size.Name}/{variant.Color.Name}) chỉ còn {variant.StockQuantity} sản phẩm.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await transaction.RollbackAsync();
                    PopulateCheckoutItems(model, cart);
                    return View(model);
                }

                var subTotal = cart.CartItems.Sum(item => item.UnitPrice * item.Quantity);
                var shippingFee = subTotal > 0 ? 30000 : 0;
                var discount = subTotal >= 500000 ? 50000 : 0;
                var order = new SmartStore.Models.Order
                {
                    OrderCode = GenerateOrderCode(),
                    UserId = userId,
                    CustomerName = model.CustomerName.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                    ShippingAddress = model.ShippingAddress.Trim(),
                    Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
                    SubTotal = subTotal,
                    ShippingFee = shippingFee,
                    Discount = discount,
                    Total = subTotal + shippingFee - discount,
                    OrderStatus = OrderStatus.ChoXacNhan,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = PaymentStatus.ChuaThanhToan,
                    CreatedAt = DateTime.Now
                };

                foreach (var cartItem in cart.CartItems)
                {
                    var variant = cartItem.ProductVariant;
                    var lineTotal = cartItem.UnitPrice * cartItem.Quantity;
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductVariantId = variant.Id,
                        ProductName = variant.Product.Name,
                        SizeName = variant.Size.Name,
                        ColorName = variant.Color.Name,
                        Sku = variant.Sku,
                        ImageUrl = GetMainImageUrl(variant.Product),
                        UnitPrice = cartItem.UnitPrice,
                        Quantity = cartItem.Quantity,
                        LineTotal = lineTotal
                    });

                    variant.StockQuantity -= cartItem.Quantity;
                }

                _dbContext.Orders.Add(order);
                _dbContext.CartItemEntities.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Success), new { id = order.Id });
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception, "Không thể tạo đơn hàng cho tài khoản {UserId}", userId);
                ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng lúc này. Vui lòng thử lại.");
                return await CheckoutValidationViewAsync(userId, model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var order = await LoadOrderAsync(id);
            if (order == null || (!User.IsInRole(AppRoles.Admin) && order.UserId != GetUserId()))
            {
                return NotFound();
            }

            return View(BuildDetailsViewModel(order));
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            var orders = await _dbContext.Orders
                .AsNoTracking()
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.CreatedAt)
                .Select(order => new OrderListItemViewModel
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    CustomerName = order.CustomerName,
                    PhoneNumber = order.PhoneNumber,
                    CreatedAt = order.CreatedAt,
                    Total = order.Total,
                    OrderStatus = order.OrderStatus,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus
                })
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await LoadOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole(AppRoles.Admin);
            if (!isAdmin && order.UserId != GetUserId())
            {
                return NotFound();
            }

            var model = BuildDetailsViewModel(order);
            model.IsAdmin = isAdmin;
            model.CanCancel = !isAdmin && order.UserId == GetUserId() && order.OrderStatus == OrderStatus.ChoXacNhan;
            model.AllowedNextStatuses = isAdmin ? GetAllowedNextStatuses(order.OrderStatus) : new List<OrderStatus>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await _dbContext.Orders
                .Include(item => item.OrderItems)
                    .ThenInclude(item => item.ProductVariant)
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (order == null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (order.OrderStatus != OrderStatus.ChoXacNhan)
            {
                await transaction.RollbackAsync();
                TempData["OrderMessage"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận.";
                return RedirectToAction(nameof(Details), new { id });
            }

            RestoreStock(order);
            order.OrderStatus = OrderStatus.DaHuy;
            order.CanceledAt = DateTime.Now;
            if (order.PaymentStatus == PaymentStatus.DaThanhToan)
            {
                order.PaymentStatus = PaymentStatus.DaHoanTien;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["OrderMessage"] = "Đã hủy đơn hàng và hoàn lại tồn kho.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> AdminIndex(string? search, OrderStatus? status, DateTime? orderDate)
        {
            var query = _dbContext.Orders.AsNoTracking().AsQueryable();
            var totalOrders = await query.CountAsync();
            var completedRevenue = await query
                .Where(order => order.OrderStatus == OrderStatus.HoanThanh)
                .SumAsync(order => (decimal?)order.Total) ?? 0;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(order =>
                    order.OrderCode.Contains(keyword) ||
                    order.PhoneNumber.Contains(keyword) ||
                    order.CustomerName.Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(order => order.OrderStatus == status.Value);
            }

            if (orderDate.HasValue)
            {
                var start = orderDate.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(order => order.CreatedAt >= start && order.CreatedAt < end);
            }

            var items = await query
                .OrderByDescending(order => order.CreatedAt)
                .Select(order => new OrderListItemViewModel
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    CustomerName = order.CustomerName,
                    PhoneNumber = order.PhoneNumber,
                    CreatedAt = order.CreatedAt,
                    Total = order.Total,
                    OrderStatus = order.OrderStatus,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus
                })
                .ToListAsync();

            return View(new AdminOrderIndexViewModel
            {
                Items = items,
                Search = search,
                Status = status,
                OrderDate = orderDate,
                TotalOrders = totalOrders,
                CompletedRevenue = completedRevenue
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            if (!Enum.IsDefined(newStatus))
            {
                return BadRequest();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var order = await _dbContext.Orders
                .Include(item => item.OrderItems)
                    .ThenInclude(item => item.ProductVariant)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (order == null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            var allowedStatuses = GetAllowedNextStatuses(order.OrderStatus);
            if (!allowedStatuses.Contains(newStatus))
            {
                await transaction.RollbackAsync();
                TempData["OrderMessage"] = "Không thể chuyển sang trạng thái đã chọn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ApplyStatusChange(order, newStatus);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["OrderMessage"] = "Đã cập nhật trạng thái đơn hàng.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IActionResult> CheckoutValidationViewAsync(string userId, CheckoutViewModel model)
        {
            var cart = await LoadCartAsync(userId, tracking: false);
            if (cart == null || cart.CartItems.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            PopulateCheckoutItems(model, cart);
            return View(nameof(Checkout), model);
        }

        private async Task<ShoppingCart?> LoadCartAsync(string userId, bool tracking)
        {
            var query = _dbContext.ShoppingCarts
                .AsSplitQuery()
                .Include(cart => cart.CartItems)
                    .ThenInclude(item => item.ProductVariant)
                        .ThenInclude(variant => variant.Product)
                            .ThenInclude(product => product.ProductImages)
                .Include(cart => cart.CartItems)
                    .ThenInclude(item => item.ProductVariant)
                        .ThenInclude(variant => variant.Size)
                .Include(cart => cart.CartItems)
                    .ThenInclude(item => item.ProductVariant)
                        .ThenInclude(variant => variant.Color)
                .Where(cart => cart.UserId == userId);

            return tracking
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        private async Task<SmartStore.Models.Order?> LoadOrderAsync(int id)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.OrderItems)
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        private static CheckoutViewModel BuildCheckoutViewModel(ShoppingCart cart)
        {
            var model = new CheckoutViewModel();
            PopulateCheckoutItems(model, cart);
            return model;
        }

        private static void PopulateCheckoutItems(CheckoutViewModel model, ShoppingCart cart)
        {
            model.Items = cart.CartItems.Select(item => new CartItemViewModel
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
            }).ToList();
        }

        private static OrderDetailsViewModel BuildDetailsViewModel(SmartStore.Models.Order order)
        {
            return new OrderDetailsViewModel
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerName = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Email = order.Email,
                ShippingAddress = order.ShippingAddress,
                Note = order.Note,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                Discount = order.Discount,
                Total = order.Total,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                ConfirmedAt = order.ConfirmedAt,
                ShippingAt = order.ShippingAt,
                CompletedAt = order.CompletedAt,
                CanceledAt = order.CanceledAt,
                Items = order.OrderItems.OrderBy(item => item.Id).ToList()
            };
        }

        private static List<OrderStatus> GetAllowedNextStatuses(OrderStatus status) => status switch
        {
            OrderStatus.ChoXacNhan => new List<OrderStatus> { OrderStatus.DaXacNhan, OrderStatus.DaHuy },
            OrderStatus.DaXacNhan => new List<OrderStatus> { OrderStatus.DangGiao, OrderStatus.DaHuy },
            OrderStatus.DangGiao => new List<OrderStatus> { OrderStatus.HoanThanh },
            _ => new List<OrderStatus>()
        };

        private static void ApplyStatusChange(SmartStore.Models.Order order, OrderStatus newStatus)
        {
            order.OrderStatus = newStatus;
            var now = DateTime.Now;
            switch (newStatus)
            {
                case OrderStatus.DaXacNhan:
                    order.ConfirmedAt = now;
                    break;
                case OrderStatus.DangGiao:
                    order.ShippingAt = now;
                    break;
                case OrderStatus.HoanThanh:
                    order.CompletedAt = now;
                    if (order.PaymentMethod == PaymentMethod.COD)
                    {
                        order.PaymentStatus = PaymentStatus.DaThanhToan;
                    }
                    break;
                case OrderStatus.DaHuy:
                    RestoreStock(order);
                    order.CanceledAt = now;
                    if (order.PaymentStatus == PaymentStatus.DaThanhToan)
                    {
                        order.PaymentStatus = PaymentStatus.DaHoanTien;
                    }
                    break;
            }
        }

        private static void RestoreStock(SmartStore.Models.Order order)
        {
            foreach (var item in order.OrderItems)
            {
                item.ProductVariant.StockQuantity += item.Quantity;
            }
        }

        private string GetUserId()
        {
            return _userManager.GetUserId(User)
                ?? throw new InvalidOperationException("Không xác định được tài khoản đăng nhập.");
        }

        private static string GenerateOrderCode()
        {
            return $"DH{DateTime.Now:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 10000)}";
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

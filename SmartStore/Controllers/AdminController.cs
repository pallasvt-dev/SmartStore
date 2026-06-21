using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;
using SmartStore.ViewModels;

namespace SmartStore.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private const string DefaultImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80";
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonth = monthStart.AddMonths(1);
            var chartStart = today.AddDays(-6);

            var model = await _dbContext.Orders
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(group => new AdminDashboardViewModel
                {
                    TodayRevenue = group
                        .Where(order => order.OrderStatus == OrderStatus.HoanThanh
                            && order.CompletedAt >= today
                            && order.CompletedAt < tomorrow)
                        .Sum(order => (decimal?)order.Total) ?? 0,
                    MonthRevenue = group
                        .Where(order => order.OrderStatus == OrderStatus.HoanThanh
                            && order.CompletedAt >= monthStart
                            && order.CompletedAt < nextMonth)
                        .Sum(order => (decimal?)order.Total) ?? 0,
                    TotalRevenue = group
                        .Where(order => order.OrderStatus == OrderStatus.HoanThanh)
                        .Sum(order => (decimal?)order.Total) ?? 0,
                    TodayOrders = group.Count(order => order.CreatedAt >= today && order.CreatedAt < tomorrow),
                    PendingOrders = group.Count(order => order.OrderStatus == OrderStatus.ChoXacNhan),
                    ShippingOrders = group.Count(order => order.OrderStatus == OrderStatus.DangGiao),
                    CompletedOrders = group.Count(order => order.OrderStatus == OrderStatus.HoanThanh),
                    CanceledOrders = group.Count(order => order.OrderStatus == OrderStatus.DaHuy)
                })
                .FirstOrDefaultAsync() ?? new AdminDashboardViewModel();

            model.TotalProducts = await _dbContext.Products.AsNoTracking().CountAsync();
            model.ActiveProducts = await _dbContext.Products.AsNoTracking().CountAsync(product => product.IsActive);
            model.LowStockVariants = await _dbContext.ProductVariants
                .AsNoTracking()
                .CountAsync(variant => variant.StockQuantity > 0 && variant.StockQuantity <= 5);
            model.OutOfStockVariants = await _dbContext.ProductVariants
                .AsNoTracking()
                .CountAsync(variant => variant.StockQuantity == 0);

            var customerRoleId = await _dbContext.Roles
                .AsNoTracking()
                .Where(role => role.Name == AppRoles.Customer)
                .Select(role => role.Id)
                .FirstOrDefaultAsync();
            model.TotalCustomers = string.IsNullOrWhiteSpace(customerRoleId)
                ? 0
                : await _dbContext.UserRoles.AsNoTracking().CountAsync(item => item.RoleId == customerRoleId);

            model.RecentOrders = await _dbContext.Orders
                .AsNoTracking()
                .OrderByDescending(order => order.CreatedAt)
                .Take(8)
                .Select(order => new RecentOrderViewModel
                {
                    OrderId = order.Id,
                    OrderCode = order.OrderCode,
                    CustomerName = order.CustomerName,
                    CreatedAt = order.CreatedAt,
                    Total = order.Total,
                    OrderStatus = order.OrderStatus
                })
                .ToListAsync();

            model.LowStockVariantsList = await _dbContext.ProductVariants
                .AsNoTracking()
                .Where(variant => variant.IsActive && variant.Product.IsActive && variant.StockQuantity <= 5)
                .OrderBy(variant => variant.StockQuantity)
                .ThenBy(variant => variant.Product.Name)
                .Take(8)
                .Select(variant => new LowStockVariantViewModel
                {
                    ProductId = variant.ProductId,
                    ProductName = variant.Product.Name,
                    ProductVariantId = variant.Id,
                    SizeName = variant.Size.Name,
                    ColorName = variant.Color.Name,
                    Sku = variant.Sku,
                    StockQuantity = variant.StockQuantity,
                    ImageUrl = variant.Product.ProductImages
                        .OrderByDescending(image => image.IsMain)
                        .ThenBy(image => image.DisplayOrder)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? DefaultImageUrl
                })
                .ToListAsync();

            model.TopSellingProducts = await _dbContext.OrderItems
                .AsNoTracking()
                .Where(item => item.Order.OrderStatus == OrderStatus.HoanThanh)
                .GroupBy(item => item.ProductVariant.ProductId)
                .Select(group => new TopSellingProductViewModel
                {
                    ProductId = group.Key,
                    ProductName = group.OrderByDescending(item => item.Order.CreatedAt)
                        .Select(item => item.ProductName)
                        .First(),
                    SoldQuantity = group.Sum(item => item.Quantity),
                    Revenue = group.Sum(item => item.LineTotal)
                })
                .OrderByDescending(item => item.SoldQuantity)
                .ThenByDescending(item => item.Revenue)
                .Take(5)
                .ToListAsync();

            if (model.TopSellingProducts.Count > 0)
            {
                var topProductIds = model.TopSellingProducts.Select(item => item.ProductId).ToList();
                var productImages = await _dbContext.Products
                    .AsNoTracking()
                    .Where(product => topProductIds.Contains(product.Id))
                    .Select(product => new
                    {
                        product.Id,
                        ImageUrl = product.ProductImages
                            .OrderByDescending(image => image.IsMain)
                            .ThenBy(image => image.DisplayOrder)
                            .Select(image => image.ImageUrl)
                            .FirstOrDefault() ?? DefaultImageUrl
                    })
                    .ToDictionaryAsync(item => item.Id, item => item.ImageUrl);

                foreach (var product in model.TopSellingProducts)
                {
                    product.ImageUrl = productImages.GetValueOrDefault(product.ProductId, DefaultImageUrl);
                }
            }

            var revenueByDate = await _dbContext.Orders
                .AsNoTracking()
                .Where(order => order.OrderStatus == OrderStatus.HoanThanh
                    && order.CompletedAt >= chartStart
                    && order.CompletedAt < tomorrow)
                .GroupBy(order => order.CompletedAt!.Value.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Revenue = group.Sum(order => order.Total),
                    OrderCount = group.Count()
                })
                .ToDictionaryAsync(item => item.Date);

            model.RevenueChart = Enumerable.Range(0, 7)
                .Select(offset => chartStart.AddDays(offset))
                .Select(date => new RevenuePointViewModel
                {
                    Label = date.ToString("dd/MM"),
                    Revenue = revenueByDate.GetValueOrDefault(date)?.Revenue ?? 0,
                    OrderCount = revenueByDate.GetValueOrDefault(date)?.OrderCount ?? 0
                })
                .ToList();

            var orderStatusCounts = await _dbContext.Orders
                .AsNoTracking()
                .GroupBy(order => order.OrderStatus)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.Status, item => item.Count);

            model.OrderStatusChart = Enum.GetValues<OrderStatus>()
                .Select(status => new OrderStatusPointViewModel
                {
                    Status = status,
                    Count = orderStatusCounts.GetValueOrDefault(status)
                })
                .ToList();

            return View(model);
        }
    }
}

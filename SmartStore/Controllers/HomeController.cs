using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;
using SmartStore.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace SmartStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext dbContext, ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Include(product => product.ProductImages)
                .Include(product => product.ProductVariants)
                .Where(product => product.IsActive)
                .OrderByDescending(product => product.Id)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.FavoriteProductIds = string.IsNullOrWhiteSpace(userId)
                ? new HashSet<int>()
                : (await _dbContext.WishlistItems
                    .AsNoTracking()
                    .Where(item => item.UserId == userId)
                    .Select(item => item.ProductId)
                    .ToListAsync())
                    .ToHashSet();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(item => item.Category)
                .Include(item => item.Brand)
                .Include(item => item.ProductImages)
                .Include(item => item.ProductVariants)
                    .ThenInclude(variant => variant.Size)
                .Include(item => item.ProductVariants)
                    .ThenInclude(variant => variant.Color)
                .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = await _dbContext.ProductReviews
                .AsNoTracking()
                .Include(review => review.User)
                .Include(review => review.Images)
                .Where(review => review.ProductId == id && review.IsApproved)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();

            var suggestions = await _dbContext.OutfitSuggestions
                .AsNoTracking()
                .AsSplitQuery()
                .Include(item => item.SuggestedProduct)
                    .ThenInclude(item => item.ProductImages)
                .Include(item => item.SuggestedProduct)
                    .ThenInclude(item => item.ProductVariants)
                .Where(item => item.ProductId == id && item.IsActive && item.SuggestedProduct.IsActive)
                .OrderBy(item => item.DisplayOrder)
                .ThenBy(item => item.Id)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isCustomer = User.IsInRole(AppRoles.Customer);
            var isFavorite = false;
            var hasPurchased = false;
            var hasReviewed = false;

            if (isCustomer && !string.IsNullOrWhiteSpace(userId))
            {
                isFavorite = await _dbContext.WishlistItems
                    .AsNoTracking()
                    .AnyAsync(item => item.UserId == userId && item.ProductId == id);
                hasPurchased = await _dbContext.Orders
                    .AsNoTracking()
                    .AnyAsync(order => order.UserId == userId
                        && order.OrderStatus == OrderStatus.HoanThanh
                        && order.OrderItems.Any(item => item.ProductVariant.ProductId == id));
                hasReviewed = await _dbContext.ProductReviews
                    .AsNoTracking()
                    .AnyAsync(review => review.UserId == userId && review.ProductId == id);
            }

            return View(new ProductDetailsViewModel
            {
                Product = product,
                Reviews = reviews,
                OutfitSuggestions = suggestions,
                IsFavorite = isFavorite,
                IsCustomer = isCustomer,
                HasPurchased = hasPurchased,
                HasReviewed = hasReviewed
            });
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

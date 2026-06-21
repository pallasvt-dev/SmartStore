using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;

namespace SmartStore.Controllers
{
    [Authorize(Roles = AppRoles.Customer)]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var items = await _dbContext.WishlistItems
                .AsNoTracking()
                .AsSplitQuery()
                .Include(item => item.Product)
                    .ThenInclude(product => product.ProductImages)
                .Include(item => item.Product)
                    .ThenInclude(product => product.ProductVariants)
                .Include(item => item.Product)
                    .ThenInclude(product => product.Category)
                .Include(item => item.Product)
                    .ThenInclude(product => product.Brand)
                .Where(item => item.UserId == userId && item.Product.IsActive)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            var userId = GetUserId();
            var productExists = await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == productId && product.IsActive);
            if (!productExists)
            {
                return NotFound();
            }

            var existing = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId);
            bool isFavorite;
            string message;

            if (existing == null)
            {
                _dbContext.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId
                });
                isFavorite = true;
                message = "Đã thêm vào danh sách yêu thích";
            }
            else
            {
                _dbContext.WishlistItems.Remove(existing);
                isFavorite = false;
                message = "Đã xóa khỏi danh sách yêu thích";
            }

            await _dbContext.SaveChangesAsync();
            var count = await _dbContext.WishlistItems
                .AsNoTracking()
                .CountAsync(item => item.UserId == userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { isFavorite, message, count });
            }

            TempData["WishlistMessage"] = message;
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            var item = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(entry => entry.UserId == userId && entry.ProductId == productId);
            if (item != null)
            {
                _dbContext.WishlistItems.Remove(item);
                await _dbContext.SaveChangesAsync();
            }

            TempData["WishlistMessage"] = "Đã xóa khỏi danh sách yêu thích";
            return RedirectToAction(nameof(Index));
        }

        private string GetUserId() => _userManager.GetUserId(User)
            ?? throw new InvalidOperationException("Không xác định được tài khoản đăng nhập.");
    }
}

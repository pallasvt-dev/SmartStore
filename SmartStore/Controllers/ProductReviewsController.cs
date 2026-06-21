using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;

namespace SmartStore.Controllers
{
    [Authorize]
    public class ProductReviewsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductReviewsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> Create(int productId, int rating, string? comment, string? imageUrls)
        {
            var userId = GetUserId();
            if (rating < 1 || rating > 5)
            {
                TempData["ReviewMessage"] = "Vui lòng chọn số sao từ 1 đến 5.";
                return RedirectToProduct(productId);
            }

            comment = comment?.Trim();
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ReviewMessage"] = "Vui lòng nhập nội dung đánh giá.";
                return RedirectToProduct(productId);
            }

            if (comment.Length > 1000)
            {
                TempData["ReviewMessage"] = "Bình luận không được vượt quá 1000 ký tự.";
                return RedirectToProduct(productId);
            }

            var orderId = await _dbContext.Orders
                .AsNoTracking()
                .Where(order => order.UserId == userId
                    && order.OrderStatus == OrderStatus.HoanThanh
                    && order.OrderItems.Any(item => item.ProductVariant.ProductId == productId))
                .OrderByDescending(order => order.CompletedAt)
                .Select(order => order.Id)
                .FirstOrDefaultAsync();

            if (orderId == 0)
            {
                TempData["ReviewMessage"] = "Bạn cần mua và hoàn thành đơn hàng có sản phẩm này để đánh giá.";
                return RedirectToProduct(productId);
            }

            var alreadyReviewed = await _dbContext.ProductReviews
                .AsNoTracking()
                .AnyAsync(review => review.UserId == userId && review.ProductId == productId);
            if (alreadyReviewed)
            {
                TempData["ReviewMessage"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToProduct(productId);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                OrderId = orderId,
                Rating = rating,
                Comment = comment,
                IsApproved = true,
                CreatedAt = DateTime.Now
            };

            var displayOrder = 1;
            foreach (var imageUrl in SplitImageUrls(imageUrls).Take(5))
            {
                review.Images.Add(new ProductReviewImage
                {
                    ImageUrl = imageUrl,
                    DisplayOrder = displayOrder++
                });
            }

            _dbContext.ProductReviews.Add(review);
            await _dbContext.SaveChangesAsync();
            await RecalculateProductRatingAsync(productId);
            await transaction.CommitAsync();

            TempData["ReviewMessage"] = "Cảm ơn bạn đã gửi đánh giá sản phẩm.";
            return RedirectToProduct(productId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _dbContext.ProductReviews.FirstOrDefaultAsync(item => item.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole(AppRoles.Admin);
            if (!isAdmin && review.UserId != GetUserId())
            {
                return Forbid();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            _dbContext.ProductReviews.Remove(review);
            await _dbContext.SaveChangesAsync();
            await RecalculateProductRatingAsync(review.ProductId);
            await transaction.CommitAsync();

            TempData[isAdmin ? "ReviewAdminMessage" : "ReviewMessage"] = "Đã xóa đánh giá sản phẩm.";
            return isAdmin
                ? RedirectToAction(nameof(AdminIndex))
                : RedirectToProduct(review.ProductId);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> AdminIndex()
        {
            var reviews = await _dbContext.ProductReviews
                .AsNoTracking()
                .Include(review => review.Product)
                .Include(review => review.User)
                .Include(review => review.Images)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public Task<IActionResult> Approve(int id) => SetApprovalAsync(id, true);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public Task<IActionResult> Hide(int id) => SetApprovalAsync(id, false);

        private async Task<IActionResult> SetApprovalAsync(int id, bool isApproved)
        {
            var review = await _dbContext.ProductReviews.FirstOrDefaultAsync(item => item.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            review.IsApproved = isApproved;
            review.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            await RecalculateProductRatingAsync(review.ProductId);
            await transaction.CommitAsync();

            TempData["ReviewAdminMessage"] = isApproved ? "Đã hiển thị đánh giá." : "Đã ẩn đánh giá.";
            return RedirectToAction(nameof(AdminIndex));
        }

        private async Task RecalculateProductRatingAsync(int productId)
        {
            var average = await _dbContext.ProductReviews
                .AsNoTracking()
                .Where(review => review.ProductId == productId && review.IsApproved)
                .AverageAsync(review => (double?)review.Rating) ?? 0;

            await _dbContext.Products
                .Where(product => product.Id == productId)
                .ExecuteUpdateAsync(update => update.SetProperty(product => product.Rating, Math.Round(average, 1)));
        }

        private IActionResult RedirectToProduct(int productId) =>
            RedirectToAction("Details", "Home", new { id = productId }, "danh-gia");

        private string GetUserId() => _userManager.GetUserId(User)
            ?? throw new InvalidOperationException("Không xác định được tài khoản đăng nhập.");

        private static IEnumerable<string> SplitImageUrls(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }

            return value
                .Split(new[] { "\r\n", "\n", ",", ";" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .Where(url => url.Length <= 2000)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}

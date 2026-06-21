using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;
using SmartStore.ViewModels;

namespace SmartStore.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class OutfitSuggestionsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public OutfitSuggestionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _dbContext.OutfitSuggestions
                .AsNoTracking()
                .Include(item => item.Product)
                .Include(item => item.SuggestedProduct)
                .OrderBy(item => item.Product.Name)
                .ThenBy(item => item.DisplayOrder)
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new OutfitSuggestionFormViewModel();
            await LoadProductsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OutfitSuggestionFormViewModel model)
        {
            await ValidateSuggestionAsync(model);
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(model);
                return View(model);
            }

            _dbContext.OutfitSuggestions.Add(new OutfitSuggestion
            {
                ProductId = model.ProductId,
                SuggestedProductId = model.SuggestedProductId,
                Title = NormalizeOptional(model.Title),
                Note = NormalizeOptional(model.Note),
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            });
            await _dbContext.SaveChangesAsync();
            TempData["OutfitMessage"] = "Đã thêm gợi ý phối đồ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _dbContext.OutfitSuggestions.AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            var model = new OutfitSuggestionFormViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                SuggestedProductId = item.SuggestedProductId,
                Title = item.Title,
                Note = item.Note,
                DisplayOrder = item.DisplayOrder,
                IsActive = item.IsActive
            };
            await LoadProductsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OutfitSuggestionFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            await ValidateSuggestionAsync(model, id);
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(model);
                return View(model);
            }

            var item = await _dbContext.OutfitSuggestions.FirstOrDefaultAsync(entry => entry.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            item.ProductId = model.ProductId;
            item.SuggestedProductId = model.SuggestedProductId;
            item.Title = NormalizeOptional(model.Title);
            item.Note = NormalizeOptional(model.Note);
            item.DisplayOrder = model.DisplayOrder;
            item.IsActive = model.IsActive;
            await _dbContext.SaveChangesAsync();
            TempData["OutfitMessage"] = "Đã cập nhật gợi ý phối đồ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _dbContext.OutfitSuggestions.FirstOrDefaultAsync(entry => entry.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = !item.IsActive;
            await _dbContext.SaveChangesAsync();
            TempData["OutfitMessage"] = item.IsActive ? "Đã hiển thị gợi ý phối đồ." : "Đã ẩn gợi ý phối đồ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _dbContext.OutfitSuggestions
                .AsNoTracking()
                .Include(entry => entry.Product)
                .Include(entry => entry.SuggestedProduct)
                .FirstOrDefaultAsync(entry => entry.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _dbContext.OutfitSuggestions.FirstOrDefaultAsync(entry => entry.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            _dbContext.OutfitSuggestions.Remove(item);
            await _dbContext.SaveChangesAsync();
            TempData["OutfitMessage"] = "Đã xóa gợi ý phối đồ.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateSuggestionAsync(OutfitSuggestionFormViewModel model, int? currentId = null)
        {
            if (model.ProductId == model.SuggestedProductId && model.ProductId > 0)
            {
                ModelState.AddModelError(nameof(model.SuggestedProductId), "Sản phẩm phối cùng phải khác sản phẩm chính.");
            }

            var productIds = new[] { model.ProductId, model.SuggestedProductId };
            var existingProducts = await _dbContext.Products
                .AsNoTracking()
                .CountAsync(product => productIds.Contains(product.Id) && product.IsActive);
            if (model.ProductId > 0 && model.SuggestedProductId > 0 && existingProducts != 2)
            {
                ModelState.AddModelError(string.Empty, "Một trong các sản phẩm đã chọn không tồn tại hoặc đã ngừng bán.");
            }

            var duplicate = await _dbContext.OutfitSuggestions
                .AsNoTracking()
                .AnyAsync(item => item.ProductId == model.ProductId
                    && item.SuggestedProductId == model.SuggestedProductId
                    && (!currentId.HasValue || item.Id != currentId));
            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "Cặp sản phẩm này đã có trong danh sách gợi ý phối đồ.");
            }
        }

        private async Task LoadProductsAsync(OutfitSuggestionFormViewModel model)
        {
            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.IsActive)
                .OrderBy(product => product.Name)
                .Select(product => new { product.Id, product.Name })
                .ToListAsync();
            model.Products = new SelectList(products, "Id", "Name");
        }

        private static string? NormalizeOptional(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

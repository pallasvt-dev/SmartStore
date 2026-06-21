using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;

namespace SmartStore.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartSummaryViewComponent(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var count = string.IsNullOrWhiteSpace(userId)
                ? 0
                : await _dbContext.CartItemEntities
                    .Where(item => item.ShoppingCart.UserId == userId)
                    .SumAsync(item => item.Quantity);

            return View(count);
        }
    }
}

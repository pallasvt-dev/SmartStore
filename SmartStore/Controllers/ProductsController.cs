using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStore.Models;

namespace SmartStore.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View(ProductRepository.GetAll());
        }

        public IActionResult Details(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Create()
        {
            var product = new Product
            {
                ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=900&q=80",
                Badge = "New",
                Rating = 4.8,
                StockStatus = "Còn hàng"
            };

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            ProductRepository.Add(product);
            TempData["ProductMessage"] = "Đã thêm sản phẩm mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var updated = ProductRepository.Update(product);
            if (!updated)
            {
                return NotFound();
            }

            TempData["ProductMessage"] = "Đã cập nhật sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var deleted = ProductRepository.Delete(id);
            if (!deleted)
            {
                return NotFound();
            }

            TempData["ProductMessage"] = "Đã xóa sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}

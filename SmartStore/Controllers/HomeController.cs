using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStore.Data;
using SmartStore.Models;
using System.Diagnostics;

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
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Include(product => product.ProductImages)
                .Include(product => product.ProductVariants)
                .Where(product => product.IsActive)
                .OrderByDescending(product => product.Id)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _dbContext.Products
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

            return View(product);
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

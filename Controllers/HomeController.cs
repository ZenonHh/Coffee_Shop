using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public HomeController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();
            try
            {
                viewModel.FeaturedProducts = await _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.Category)
                    .Where(p => p.TrangThai && p.Category!.TenLoaiSP != "Topping")
                    .Take(8)
                    .ToListAsync();

                viewModel.Categories = await _context.Categories
                    .Where(c => c.TenLoaiSP != "Topping")
                    .OrderBy(c => c.ThuTuHienThi)
                    .ToListAsync();

                viewModel.CustomerReviews = await _context.Feedbacks
                    .Include(f => f.Customer)
                    .Where(f => f.DanhGia >= 4)
                    .OrderByDescending(f => f.NgayGui)
                    .Take(4)
                    .ToListAsync();
            }
            catch
            {
                // Fallback nếu database chưa có connection lúc dev view
            }

            return View(viewModel);
        }
        public IActionResult Contact()
        {
            return View();
        }

        public async Task<IActionResult> Blog()
        {
            var categories = new List<Category>();
            try
            {
                categories = await _context.Categories
                    .Include(c => c.Products.Where(p => p.TrangThai))
                        .ThenInclude(p => p.Variants.Where(v => v.TrangThai))
                    .Where(c => c.TenLoaiSP != "Topping")
                    .OrderBy(c => c.ThuTuHienThi)
                    .ToListAsync();
            }
            catch
            {
                // Fallback nếu database chưa có connection lúc dev view
            }

            return View(categories);
        }

        public async Task<IActionResult> BlogSingle(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    var product = await _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Variants)
                        .FirstOrDefaultAsync(p => p.MaSP == id.Value);

                    if (product != null)
                    {
                        ViewBag.Product = product;
                    }
                }

                ViewBag.RecentProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.TrangThai && p.Category!.TenLoaiSP != "Topping")
                    .Take(3)
                    .ToListAsync();
            }
            catch
            {
                ViewBag.RecentProducts = new List<Product>();
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

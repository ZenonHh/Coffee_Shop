using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Controllers
{
    public class ShopController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public ShopController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? keyword)
        {
            var query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.TrangThai && p.Category!.TenLoaiSP != "Topping");

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.MaLoaiSP == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.TenSP.Contains(keyword));
            }

            var viewModel = new ShopViewModel
            {
                Categories = await _context.Categories
                    .Where(c => c.TenLoaiSP != "Topping")
                    .OrderBy(c => c.ThuTuHienThi)
                    .ToListAsync(),
                Products = await query.ToListAsync(),
                SelectedCategoryId = categoryId,
                Keyword = keyword
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants.Where(v => v.TrangThai))
                .FirstOrDefaultAsync(p => p.MaSP == id && p.TrangThai);

            if (product == null)
            {
                return NotFound();
            }

            var availableToppings = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.TrangThai && p.Category!.TenLoaiSP == "Topping")
                .ToListAsync();

            var relatedProducts = await _context.Products
                .Include(p => p.Variants)
                .Where(p => p.TrangThai && p.MaLoaiSP == product.MaLoaiSP && p.MaSP != product.MaSP)
                .Take(4)
                .ToListAsync();

            var reviews = await _context.Feedbacks
                .Include(f => f.Customer)
                .Where(f => f.MaSP == id && f.TrangThaiXL == "Đã duyệt")
                .OrderByDescending(f => f.NgayGui)
                .ToListAsync();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Variants = product.Variants.ToList(),
                AvailableToppings = availableToppings,
                RelatedProducts = relatedProducts,
                Reviews = reviews,
                AverageRating = reviews.Any() ? reviews.Average(r => r.DanhGia ?? 5) : 5.0,
                TotalReviews = reviews.Count
            };

            return View(viewModel);
        }
    }
}

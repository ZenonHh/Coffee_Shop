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
            keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.TrangThai && p.MaLoaiSP != 9);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.MaLoaiSP == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.TenSP.Contains(keyword));
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(category => category.MaLoaiSP != 9)
                .OrderBy(category => category.ThuTuHienThi)
                .Select(category => new ShopCategoryViewModel
                {
                    MaLoaiSP = category.MaLoaiSP,
                    TenLoaiSP = category.TenLoaiSP
                })
                .ToListAsync();

            var products = await query
                .OrderBy(product => product.TenSP)
                .Select(product => new ShopProductCardViewModel
                {
                    MaSP = product.MaSP,
                    TenSP = product.TenSP,
                    HinhAnh = product.HinhAnh,
                    TenLoaiSP = product.Category!.TenLoaiSP,
                    GiaBan = product.Variants
                        .Where(variant => variant.TrangThai)
                        .OrderBy(variant => variant.MaBienThe)
                        .Select(variant => (decimal?)variant.GiaBan)
                        .FirstOrDefault(),
                    SoLuongTon = product.SoLuongTon
                })
                .ToListAsync();

            var viewModel = new ShopViewModel
            {
                Categories = categories,
                Products = products,
                SelectedCategoryId = categoryId,
                Keyword = keyword
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.MaSP == id && p.TrangThai)
                .Select(p => new ProductDetailProductViewModel
                {
                    MaSP = p.MaSP,
                    TenSP = p.TenSP,
                    HinhAnh = p.HinhAnh,
                    TenLoaiSP = p.Category!.TenLoaiSP,
                    SoLuongTon = p.SoLuongTon
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            var variants = await _context.ProductVariants
                .AsNoTracking()
                .Where(variant => variant.MaSP == id && variant.TrangThai)
                .OrderBy(variant => variant.MaBienThe)
                .Select(variant => new ProductVariantOptionViewModel
                {
                    MaBienThe = variant.MaBienThe,
                    KichCo = variant.KichCo,
                    GiaBan = variant.GiaBan
                })
                .ToListAsync();

            var availableToppings = await _context.Products
                .AsNoTracking()
                .Where(p => p.TrangThai && p.SoLuongTon > 0 && p.MaLoaiSP == 9)
                .OrderBy(p => p.TenSP)
                .Select(p => new ToppingOptionViewModel
                {
                    MaSP = p.MaSP,
                    TenSP = p.TenSP,
                    GiaBan = p.Variants.Where(variant => variant.TrangThai)
                        .OrderBy(variant => variant.MaBienThe)
                        .Select(variant => (decimal?)variant.GiaBan)
                        .FirstOrDefault() ?? 10000m
                })
                .ToListAsync();

            var reviews = await _context.Feedbacks
                .AsNoTracking()
                .Where(f => f.MaSP == id)
                .OrderByDescending(f => f.NgayGui)
                .Select(f => new ProductReviewViewModel
                {
                    CustomerName = f.Customer != null ? f.Customer.HoTen : "Khách hàng",
                    NoiDung = f.NoiDung,
                    DanhGia = f.DanhGia,
                    NgayGui = f.NgayGui,
                    PhanHoiQuanTri = f.PhanHoiQuanTri
                })
                .ToListAsync();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Variants = variants,
                AvailableToppings = availableToppings,
                Reviews = reviews,
                AverageRating = reviews.Any() ? reviews.Average(r => r.DanhGia ?? 5) : 5.0,
                TotalReviews = reviews.Count
            };

            return View(viewModel);
        }
    }
}

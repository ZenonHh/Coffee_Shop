using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Controllers
{
    public class ShopController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public ShopController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? categoryId,
            string? keyword,
            string? sortOrder,
            string? priceRange,
            List<string>? sizes,
            bool hasTopping = false,
            int? minimumRating = null)
        {
            keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
            sortOrder = sortOrder is "name-asc" or "name-desc" or "price-asc" or "price-desc" or "best-selling" or "rating-desc"
                ? sortOrder
                : "name-asc";
            priceRange = priceRange is "under-30" or "30-40" or "40-50" or "over-50"
                ? priceRange
                : null;
            sizes = sizes?
                .Where(size => size is "M" or "L")
                .Distinct(StringComparer.Ordinal)
                .ToList() ?? new List<string>();
            minimumRating = minimumRating == 4 ? minimumRating : null;

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

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.TrangThai && p.MaLoaiSP != 9);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.MaLoaiSP == categoryId.Value);
            }

            if (sizes.Count > 0)
            {
                query = query.Where(product => product.Variants
                    .Any(variant => variant.TrangThai && sizes.Contains(variant.KichCo)));
            }

            query = priceRange switch
            {
                "under-30" => query.Where(product => product.Variants
                    .Any(variant => variant.TrangThai
                        && (sizes.Count == 0 || sizes.Contains(variant.KichCo))
                        && variant.GiaBan < 30000m)),
                "30-40" => query.Where(product => product.Variants
                    .Any(variant => variant.TrangThai
                        && (sizes.Count == 0 || sizes.Contains(variant.KichCo))
                        && variant.GiaBan >= 30000m
                        && variant.GiaBan < 40000m)),
                "40-50" => query.Where(product => product.Variants
                    .Any(variant => variant.TrangThai
                        && (sizes.Count == 0 || sizes.Contains(variant.KichCo))
                        && variant.GiaBan >= 40000m
                        && variant.GiaBan <= 50000m)),
                "over-50" => query.Where(product => product.Variants
                    .Any(variant => variant.TrangThai
                        && (sizes.Count == 0 || sizes.Contains(variant.KichCo))
                        && variant.GiaBan > 50000m)),
                _ => query
            };

            IQueryable<ShopProductCardViewModel> productsQuery = query
                .Select(product => new ShopProductCardViewModel
                {
                    MaSP = product.MaSP,
                    TenSP = product.TenSP,
                    HinhAnh = product.HinhAnh,
                    TenLoaiSP = product.Category!.TenLoaiSP,
                    GiaBan = product.Variants
                        .Where(variant => variant.TrangThai
                            && (sizes.Count == 0 || sizes.Contains(variant.KichCo)))
                        .Select(variant => (decimal?)variant.GiaBan)
                        .Min(),
                    SoLuongTon = product.SoLuongTon,
                    SoLuongDaBan = _context.OrderDetails
                        .Where(detail => detail.MaSP == product.MaSP
                            && detail.Order!.TrangThai != OrderStatuses.Cancelled)
                        .Sum(detail => (int?)detail.SoLuong) ?? 0,
                    DiemDanhGia = product.Feedbacks
                        .Where(feedback => feedback.DanhGia.HasValue)
                        .Average(feedback => (double?)feedback.DanhGia)
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productsQuery = productsQuery.Where(product => product.TenSP.Contains(keyword));
            }

            if (hasTopping)
            {
                productsQuery = productsQuery.Where(_ => _context.Products
                    .Any(topping => topping.MaLoaiSP == 9 && topping.TrangThai && topping.SoLuongTon > 0));
            }

            if (minimumRating.HasValue)
            {
                productsQuery = productsQuery.Where(product => product.DiemDanhGia >= minimumRating.Value);
            }

            productsQuery = sortOrder switch
            {
                "name-desc" => productsQuery.OrderByDescending(product => product.TenSP),
                "price-asc" => productsQuery.OrderBy(product => product.GiaBan == null).ThenBy(product => product.GiaBan),
                "price-desc" => productsQuery.OrderBy(product => product.GiaBan == null).ThenByDescending(product => product.GiaBan),
                "best-selling" => productsQuery.OrderByDescending(product => product.SoLuongDaBan).ThenBy(product => product.TenSP),
                "rating-desc" => productsQuery.OrderByDescending(product => product.DiemDanhGia.HasValue)
                    .ThenByDescending(product => product.DiemDanhGia)
                    .ThenBy(product => product.TenSP),
                _ => productsQuery.OrderBy(product => product.TenSP)
            };

            var products = await productsQuery.ToListAsync();

            var viewModel = new ShopViewModel
            {
                Categories = categories,
                Products = products,
                SelectedCategoryId = categoryId,
                Keyword = keyword,
                SortOrder = sortOrder,
                PriceRange = priceRange,
                Sizes = sizes,
                HasTopping = hasTopping,
                MinimumRating = minimumRating
            };

            if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return PartialView("_ProductGrid", viewModel);
            }

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
                    MaLoaiSP = p.MaLoaiSP,
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

            var relatedProducts = await _context.Products
                .AsNoTracking()
                .Where(item => item.TrangThai
                    && item.MaLoaiSP == product.MaLoaiSP
                    && item.MaSP != product.MaSP)
                .OrderBy(item => item.TenSP)
                .Take(4)
                .Select(item => new ShopProductCardViewModel
                {
                    MaSP = item.MaSP,
                    TenSP = item.TenSP,
                    HinhAnh = item.HinhAnh,
                    TenLoaiSP = item.Category!.TenLoaiSP,
                    GiaBan = item.Variants
                        .Where(variant => variant.TrangThai)
                        .OrderBy(variant => variant.MaBienThe)
                        .Select(variant => (decimal?)variant.GiaBan)
                        .FirstOrDefault(),
                    SoLuongTon = item.SoLuongTon
                })
                .ToListAsync();

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
                RelatedProducts = relatedProducts,
                Reviews = reviews,
                AverageRating = reviews.Any() ? reviews.Average(r => r.DanhGia ?? 5) : 5.0,
                TotalReviews = reviews.Count
            };

            return View(viewModel);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CoffeeShop.Data;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Controllers
{
    public class ShopController : Controller
    {
        private static readonly SemaphoreSlim SearchCacheLock = new(1, 1);
        private readonly CoffeeShopDbContext _context;
        private readonly IMemoryCache _cache;

        public ShopController(CoffeeShopDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
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

            categoryId = categoryId is > 0 ? categoryId : null;
            const string cacheSessionKey = "ShopSearch.CacheSessionId";
            var sessionCacheId = HttpContext.Session.GetString(cacheSessionKey);
            if (string.IsNullOrEmpty(sessionCacheId))
            {
                sessionCacheId = Guid.NewGuid().ToString("N");
                HttpContext.Session.SetString(cacheSessionKey, sessionCacheId);
            }

            var cachedCategories = await GetOrCreateSearchCacheAsync(
                $"shop:categories:{sessionCacheId}",
                async () => await _context.Categories
                    .AsNoTracking()
                    .Where(category => category.MaLoaiSP != 9)
                    .OrderBy(category => category.ThuTuHienThi)
                    .Select(category => new ShopCategoryViewModel
                    {
                        MaLoaiSP = category.MaLoaiSP,
                        TenLoaiSP = category.TenLoaiSP
                    })
                    .ToListAsync());
            var categories = cachedCategories.Value;

            var sizeCacheKey = string.Join("-", sizes.OrderBy(size => size, StringComparer.Ordinal));
            var productsCacheKey = $"shop:products:{sessionCacheId}:{categoryId?.ToString() ?? "all"}:{priceRange ?? "any"}:{sizeCacheKey}";
            var cachedSearchResult = await GetOrCreateSearchCacheAsync(
                productsCacheKey,
                async () =>
                {
                    var query = _context.Products
                        .AsNoTracking()
                        .Where(product => product.TrangThai && product.MaLoaiSP != 9);

                    if (categoryId.HasValue)
                    {
                        query = query.Where(product => product.MaLoaiSP == categoryId.Value);
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

                    var products = await query
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
                            SelectedVariantId = product.Variants
                                .Where(variant => variant.TrangThai
                                    && (sizes.Count == 0 || sizes.Contains(variant.KichCo)))
                                .OrderBy(variant => variant.GiaBan)
                                .ThenBy(variant => variant.MaBienThe)
                                .Select(variant => (int?)variant.MaBienThe)
                                .FirstOrDefault(),
                            Variants = product.Variants
                                .Where(variant => variant.TrangThai)
                                .OrderBy(variant => variant.MaBienThe)
                                .Select(variant => new ProductVariantOptionViewModel
                                {
                                    MaBienThe = variant.MaBienThe,
                                    KichCo = variant.KichCo,
                                    GiaBan = variant.GiaBan
                                })
                                .ToList(),
                            SoLuongTon = product.SoLuongTon,
                            SoLuongDaBan = _context.OrderDetails
                                .Where(detail => detail.MaSP == product.MaSP
                                    && detail.Order!.TrangThai != OrderStatuses.Cancelled)
                                .Sum(detail => (int?)detail.SoLuong) ?? 0,
                            DiemDanhGia = product.Feedbacks
                                .Where(feedback => feedback.DanhGia.HasValue)
                                .Average(feedback => (double?)feedback.DanhGia)
                        })
                        .ToListAsync();

                    var hasAvailableToppings = await _context.Products
                        .AnyAsync(topping => topping.MaLoaiSP == 9 && topping.TrangThai && topping.SoLuongTon > 0);

                    return new CachedShopSearch(products, hasAvailableToppings);
                });
            var cachedSearch = cachedSearchResult.Value;
            Response.Headers["X-Search-Cache"] = cachedSearchResult.Hit ? "HIT" : "MISS";

            IEnumerable<ShopProductCardViewModel> productsQuery = cachedSearch.Products;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productsQuery = productsQuery.Where(product =>
                    product.TenSP.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (hasTopping && !cachedSearch.HasAvailableToppings)
            {
                productsQuery = Enumerable.Empty<ShopProductCardViewModel>();
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

            var products = productsQuery.ToList();

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

        private async Task<(T Value, bool Hit)> GetOrCreateSearchCacheAsync<T>(string cacheKey, Func<Task<T>> factory)
            where T : class
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedValue) && cachedValue is not null)
            {
                return (cachedValue, true);
            }

            await SearchCacheLock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(cacheKey, out cachedValue) && cachedValue is not null)
                {
                    return (cachedValue, true);
                }

                var value = await factory();
                _cache.Set(cacheKey, value, TimeSpan.FromMinutes(5));
                return (value, false);
            }
            finally
            {
                SearchCacheLock.Release();
            }
        }

        private sealed record CachedShopSearch(
            List<ShopProductCardViewModel> Products,
            bool HasAvailableToppings);

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

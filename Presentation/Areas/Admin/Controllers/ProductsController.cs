using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Data.Entities;
using CoffeeShop.Business.ViewModels;
using CoffeeShop.Business.Services;

namespace CoffeeShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff,NhanVien")]
    public class ProductsController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(CoffeeShopDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string soldPeriod = "month",
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            const int pageSize = 10;
            var today = DateTime.Today;

            DateTime filterStart;
            DateTime filterEnd;

            var yearStart = new DateTime(today.Year, 1, 1);
            var yearEnd = yearStart.AddYears(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            var quarterStart = new DateTime(today.Year, ((today.Month - 1) / 3 * 3) + 1, 1);
            var quarterEnd = quarterStart.AddMonths(3);
            var weekDiff = (int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
            var weekStart = today.AddDays(-weekDiff);
            var weekEnd = weekStart.AddDays(7);
            var dayStart = today;
            var dayEnd = today.AddDays(1);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value <= toDate.Value)
            {
                soldPeriod = "custom";
                filterStart = fromDate.Value.Date;
                filterEnd = toDate.Value.Date.AddDays(1);
            }
            else
            {
                soldPeriod = soldPeriod is "day" or "week" or "month" or "quarter" or "year"
                    ? soldPeriod : "day";

                (filterStart, filterEnd) = soldPeriod switch
                {
                    "day"     => (dayStart, dayEnd),
                    "week"    => (weekStart, weekEnd),
                    "quarter" => (quarterStart, quarterEnd),
                    "year"    => (yearStart, yearEnd),
                    "month"   => (monthStart, monthEnd),
                    _         => (dayStart, dayEnd)
                };
            }

            var productsQuery = _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.MaSP);
            var totalCount = await productsQuery.CountAsync();
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)));
            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(product => new AdminProductListItemViewModel
                {
                    MaSP = product.MaSP,
                    TenSP = product.TenSP,
                    HinhAnh = product.HinhAnh,
                    TenLoaiSP = product.Category!.TenLoaiSP,
                    SoLuongTon = product.SoLuongTon,
                    TrangThai = product.TrangThai,
                    Variants = product.Variants
                        .OrderBy(variant => variant.MaBienThe)
                        .Select(variant => new AdminProductVariantViewModel
                        {
                            KichCo = variant.KichCo,
                            GiaBan = variant.GiaBan
                        }).ToList()
                })
                .ToListAsync();

            if (products.Count > 0)
            {
                var queryStart = filterStart < yearStart ? filterStart : yearStart;
                var queryEnd = filterEnd > yearEnd ? filterEnd : yearEnd;
                var productIds = products.Select(product => product.MaSP).ToArray();
                var salesByProduct = await _context.OrderDetails.AsNoTracking()
                    .Where(detail => productIds.Contains(detail.MaSP)
                        && detail.Order != null
                        && detail.Order.TrangThai != OrderStatuses.Cancelled
                        && detail.Order.NgayTao >= queryStart
                        && detail.Order.NgayTao < queryEnd)
                    .GroupBy(detail => detail.MaSP)
                    .Select(group => new
                    {
                        MaSP = group.Key,
                        ThisMonth = group
                            .Where(detail => detail.Order!.NgayTao >= monthStart && detail.Order.NgayTao < monthEnd)
                            .Sum(detail => detail.SoLuong),
                        ThisQuarter = group
                            .Where(detail => detail.Order!.NgayTao >= quarterStart && detail.Order.NgayTao < quarterEnd)
                            .Sum(detail => detail.SoLuong),
                        ThisYear = group
                            .Where(detail => detail.Order!.NgayTao >= yearStart && detail.Order.NgayTao < yearEnd)
                            .Sum(detail => detail.SoLuong),
                        InPeriod = group
                            .Where(detail => detail.Order!.NgayTao >= filterStart && detail.Order.NgayTao < filterEnd)
                            .Sum(detail => detail.SoLuong)
                    })
                    .ToDictionaryAsync(item => item.MaSP);

                foreach (var product in products)
                {
                    if (!salesByProduct.TryGetValue(product.MaSP, out var sales))
                        continue;
                    product.SoldThisMonth = sales.ThisMonth;
                    product.SoldThisQuarter = sales.ThisQuarter;
                    product.SoldThisYear = sales.ThisYear;
                    product.SoldInPeriod = sales.InPeriod;
                }
            }

            ViewBag.SoldPeriod = soldPeriod;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.FilterStart = filterStart;
            ViewBag.FilterEnd = filterEnd.AddDays(-1);
            ViewBag.Pagination = new PaginationViewModel
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductSalesChartData(string period = "day", int? productId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var today = DateTime.Today;
            DateTime rangeStart;
            DateTime rangeEnd = today.AddDays(1);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value <= toDate.Value)
            {
                period = "custom";
                rangeStart = fromDate.Value.Date;
                rangeEnd = toDate.Value.Date.AddDays(1);
            }
            else
            {
                period = period?.ToLower() switch
                {
                    "week" => "week",
                    "month" => "month",
                    "quarter" => "quarter",
                    "year" => "year",
                    "custom" => "custom",
                    _ => "day"
                };

                switch (period)
                {
                    case "week":
                        var diff = (int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
                        rangeStart = today.AddDays(-diff);
                        rangeEnd = rangeStart.AddDays(7);
                        break;
                    case "month":
                        rangeStart = new DateTime(today.Year, today.Month, 1);
                        rangeEnd = rangeStart.AddMonths(1);
                        break;
                    case "quarter":
                        var quarterMonth = ((today.Month - 1) / 3 * 3) + 1;
                        rangeStart = new DateTime(today.Year, quarterMonth, 1);
                        rangeEnd = rangeStart.AddMonths(3);
                        break;
                    case "year":
                        rangeStart = new DateTime(today.Year, 1, 1);
                        rangeEnd = rangeStart.AddYears(1);
                        break;
                    case "day":
                    default:
                        rangeStart = today;
                        rangeEnd = today.AddDays(1);
                        break;
                }
            }

            var baseQuery = _context.OrderDetails
                .AsNoTracking()
                .Where(d => d.Order != null
                    && d.Order.TrangThai != OrderStatuses.Cancelled
                    && d.Order.NgayTao >= rangeStart
                    && d.Order.NgayTao < rangeEnd);

            if (productId.HasValue && productId.Value > 0)
            {
                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.MaSP == productId.Value);
                var productName = product?.TenSP ?? $"Món #{productId}";

                var startDay = rangeStart;
                var endDay = rangeEnd <= today ? rangeEnd.AddDays(-1) : today;
                if (period == "day" || (endDay - startDay).TotalDays < 6)
                {
                    startDay = today.AddDays(-6);
                    endDay = today;
                }
                else if (period is "year" or "quarter" && (endDay - startDay).TotalDays > 31)
                {
                    startDay = today.AddDays(-29);
                    endDay = today;
                }

                var productDaily = await _context.OrderDetails
                    .AsNoTracking()
                    .Where(d => d.MaSP == productId.Value
                        && d.Order != null
                        && d.Order.TrangThai != OrderStatuses.Cancelled
                        && d.Order.NgayTao >= startDay
                        && d.Order.NgayTao < endDay.AddDays(1))
                    .GroupBy(d => d.Order!.NgayTao.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Sum(d => d.SoLuong)
                    })
                    .ToListAsync();

                var dailyDict = productDaily.ToDictionary(x => x.Date, x => x.Count);

                var daysList = new List<object>();
                for (var d = startDay; d <= endDay; d = d.AddDays(1))
                {
                    daysList.Add(new
                    {
                        date = d.ToString("dd/MM"),
                        fullDate = d.ToString("dd/MM/yyyy"),
                        count = dailyDict.GetValueOrDefault(d, 0)
                    });
                }

                var totalInPeriod = await baseQuery
                    .Where(d => d.MaSP == productId.Value)
                    .SumAsync(d => d.SoLuong);

                return Json(new
                {
                    mode = "product_daily",
                    period,
                    productId = productId.Value,
                    productName,
                    totalSold = totalInPeriod,
                    items = daysList
                });
            }
            else
            {
                var top5 = await baseQuery
                    .Where(d => d.Product != null)
                    .GroupBy(d => new
                    {
                        d.MaSP,
                        d.Product!.TenSP,
                        d.Product.HinhAnh,
                        CategoryName = d.Product.Category != null ? d.Product.Category.TenLoaiSP : ""
                    })
                    .Select(g => new
                    {
                        productId = g.Key.MaSP,
                        productName = g.Key.TenSP,
                        categoryName = g.Key.CategoryName,
                        image = g.Key.HinhAnh,
                        totalSold = g.Sum(d => d.SoLuong)
                    })
                    .OrderByDescending(x => x.totalSold)
                    .Take(5)
                    .ToListAsync();

                var allProducts = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.TrangThai)
                    .OrderBy(p => p.TenSP)
                    .Select(p => new { productId = p.MaSP, productName = p.TenSP })
                    .ToListAsync();

                return Json(new
                {
                    mode = "top5",
                    period,
                    items = top5,
                    allProducts
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new AdminProductEditViewModel
            {
                TrangThai = true,
                Variants = new List<AdminProductVariantEditViewModel> { new() }
            };
            await LoadCategoriesAsync(model);
            model.MaLoaiSP = model.Categories.FirstOrDefault()?.MaLoaiSP ?? 0;
            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductEditViewModel model)
        {
            model.Variants ??= new List<AdminProductVariantEditViewModel>();
            if (model.Variants.Count == 0)
            {
                ModelState.AddModelError(nameof(model.Variants), "Thêm ít nhất một kích cỡ và giá bán.");
            }

            if (model.Variants
                .GroupBy(variant => (variant.KichCo ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1))
            {
                ModelState.AddModelError(nameof(model.Variants), "Kích cỡ không được trùng nhau.");
            }

            if (!await _context.Categories.AnyAsync(category => category.MaLoaiSP == model.MaLoaiSP))
            {
                ModelState.AddModelError(nameof(model.MaLoaiSP), "Loại sản phẩm không tồn tại.");
            }

            await ValidateImageUploadAsync(model.HinhAnhMoi);
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model);
                return PartialView("_EditModal", model);
            }

            var product = new Product
            {
                TenSP = model.TenSP.Trim(),
                MaLoaiSP = model.MaLoaiSP,
                SoLuongTon = model.SoLuongTon,
                TrangThai = model.TrangThai,
                Variants = model.Variants.Select(variant => new ProductVariant
                {
                    KichCo = variant.KichCo.Trim(),
                    GiaBan = variant.GiaBan,
                    TrangThai = true
                }).ToList()
            };

            string? uploadedImagePath = null;
            if (model.HinhAnhMoi is { Length: > 0 } uploadedImage)
            {
                (product.HinhAnh, uploadedImagePath) = await SaveImageAsync(uploadedImage);
            }

            _context.Products.Add(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                DeleteUploadedImage(uploadedImagePath);
                throw;
            }

            TempData["Success"] = $"Đã thêm sản phẩm {product.TenSP}.";
            if (IsAjaxRequest())
            {
                return Json(new { success = true });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(product => product.MaSP == id)
                .Select(product => new AdminProductEditViewModel
                {
                    MaSP = product.MaSP,
                    TenSP = product.TenSP,
                    MaLoaiSP = product.MaLoaiSP,
                    SoLuongTon = product.SoLuongTon,
                    TrangThai = product.TrangThai,
                    HinhAnh = product.HinhAnh,
                    Variants = product.Variants
                        .OrderBy(variant => variant.MaBienThe)
                        .Select(variant => new AdminProductVariantEditViewModel
                        {
                            MaBienThe = variant.MaBienThe,
                            KichCo = variant.KichCo,
                            GiaBan = variant.GiaBan
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            await LoadCategoriesAsync(product);
            return IsAjaxRequest()
                ? PartialView("_EditModal", product)
                : View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminProductEditViewModel model)
        {
            if (id != model.MaSP)
            {
                return BadRequest();
            }

            model.Variants ??= new List<AdminProductVariantEditViewModel>();
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var existingVariants = await _context.ProductVariants
                .Where(variant => variant.MaSP == id)
                .OrderBy(variant => variant.MaBienThe)
                .ToListAsync();
            var postedVariantIds = model.Variants.Select(variant => variant.MaBienThe).OrderBy(variantId => variantId);
            var existingVariantIds = existingVariants.Select(variant => variant.MaBienThe).OrderBy(variantId => variantId);

            if (!postedVariantIds.SequenceEqual(existingVariantIds))
            {
                ModelState.AddModelError(nameof(model.Variants), "Danh sách kích cỡ sản phẩm không hợp lệ. Vui lòng tải lại trang.");
            }

            if (model.Variants
                .GroupBy(variant => (variant.KichCo ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1))
            {
                ModelState.AddModelError(nameof(model.Variants), "Kích cỡ không được trùng nhau.");
            }

            if (!await _context.Categories.AnyAsync(category => category.MaLoaiSP == model.MaLoaiSP))
            {
                ModelState.AddModelError(nameof(model.MaLoaiSP), "Loại sản phẩm không tồn tại.");
            }

            await ValidateImageUploadAsync(model.HinhAnhMoi);

            if (!ModelState.IsValid)
            {
                model.HinhAnh = product.HinhAnh;
                await LoadCategoriesAsync(model);
                return IsAjaxRequest()
                    ? PartialView("_EditModal", model)
                    : View(model);
            }

            product.TenSP = model.TenSP.Trim();
            product.MaLoaiSP = model.MaLoaiSP;
            product.SoLuongTon = model.SoLuongTon;
            product.TrangThai = model.TrangThai;

            string? uploadedImagePath = null;
            if (model.HinhAnhMoi is { Length: > 0 } uploadedImage)
            {
                (product.HinhAnh, uploadedImagePath) = await SaveImageAsync(uploadedImage);
            }

            var variantsById = existingVariants.ToDictionary(variant => variant.MaBienThe);
            foreach (var variantModel in model.Variants)
            {
                var variant = variantsById[variantModel.MaBienThe];
                variant.KichCo = variantModel.KichCo.Trim();
                variant.GiaBan = variantModel.GiaBan;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                DeleteUploadedImage(uploadedImagePath);
                throw;
            }
            TempData["Success"] = $"Đã cập nhật sản phẩm {product.TenSP}.";
            if (IsAjaxRequest())
            {
                return Json(new { success = true });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.TrangThai = !product.TrangThai;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã thay đổi trạng thái sản phẩm {product.TenSP}.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync(AdminProductEditViewModel model)
        {
            model.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.ThuTuHienThi)
                .ThenBy(category => category.TenLoaiSP)
                .Select(category => new AdminProductCategoryOptionViewModel
                {
                    MaLoaiSP = category.MaLoaiSP,
                    TenLoaiSP = category.TenLoaiSP
                })
                .ToListAsync();
        }

        private bool IsAjaxRequest() =>
            string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        private async Task ValidateImageUploadAsync(IFormFile? imageFile)
        {
            if (imageFile == null)
            {
                return;
            }

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (imageFile.Length == 0 || imageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(AdminProductEditViewModel.HinhAnhMoi), "Ảnh phải có dung lượng từ 1 byte đến 5 MB.");
            }
            else if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            {
                ModelState.AddModelError(nameof(AdminProductEditViewModel.HinhAnhMoi), "Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.");
            }
            else if (!await HasValidImageSignatureAsync(imageFile, extension))
            {
                ModelState.AddModelError(nameof(AdminProductEditViewModel.HinhAnhMoi), "Nội dung tệp không đúng định dạng ảnh.");
            }
        }

        private async Task<(string FileName, string FilePath)> SaveImageAsync(IFormFile imageFile)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var imageDirectory = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "picture");
            Directory.CreateDirectory(imageDirectory);
            var filePath = Path.Combine(imageDirectory, fileName);
            try
            {
                await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await imageFile.CopyToAsync(stream);
            }
            catch (IOException)
            {
                DeleteUploadedImage(filePath);
                throw;
            }

            return (fileName, filePath);
        }

        private static void DeleteUploadedImage(string? imagePath)
        {
            if (imagePath != null && System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        private static async Task<bool> HasValidImageSignatureAsync(IFormFile imageFile, string extension)
        {
            var header = new byte[12];
            var bytesRead = 0;
            await using var stream = imageFile.OpenReadStream();
            while (bytesRead < header.Length)
            {
                var read = await stream.ReadAsync(header.AsMemory(bytesRead));
                if (read == 0)
                {
                    break;
                }
                bytesRead += read;
            }

            return extension switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => bytesRead >= 8 &&
                    header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
                ".webp" => bytesRead >= 12 &&
                    header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F' &&
                    header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P',
                _ => false
            };
        }
    }
}

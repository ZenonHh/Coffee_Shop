using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

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

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.MaSP)
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

            return View(products);
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

            if (model.HinhAnhMoi is { } imageFile)
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (imageFile.Length == 0 || imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.HinhAnhMoi), "Ảnh phải có dung lượng từ 1 byte đến 5 MB.");
                }
                else if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
                {
                    ModelState.AddModelError(nameof(model.HinhAnhMoi), "Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.");
                }
                else if (!await HasValidImageSignatureAsync(imageFile, extension))
                {
                    ModelState.AddModelError(nameof(model.HinhAnhMoi), "Nội dung tệp không đúng định dạng ảnh.");
                }
            }

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
                var extension = Path.GetExtension(uploadedImage.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var imageDirectory = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "picture");
                Directory.CreateDirectory(imageDirectory);
                uploadedImagePath = Path.Combine(imageDirectory, fileName);
                try
                {
                    await using var stream = new FileStream(uploadedImagePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    await uploadedImage.CopyToAsync(stream);
                }
                catch (IOException)
                {
                    if (System.IO.File.Exists(uploadedImagePath))
                    {
                        System.IO.File.Delete(uploadedImagePath);
                    }
                    throw;
                }

                product.HinhAnh = fileName;
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
                if (uploadedImagePath != null && System.IO.File.Exists(uploadedImagePath))
                {
                    System.IO.File.Delete(uploadedImagePath);
                }
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

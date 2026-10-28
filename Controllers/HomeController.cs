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
                    .AsNoTracking()
                    .Where(p => p.TrangThai && p.MaLoaiSP != 9)
                    .OrderBy(p => p.TenSP)
                    .Take(8)
                    .Select(product => new ShopProductCardViewModel
                    {
                        MaSP = product.MaSP,
                        TenSP = product.TenSP,
                        HinhAnh = product.HinhAnh,
                        TenLoaiSP = product.Category!.TenLoaiSP,
                        GiaBan = product.Variants.Where(variant => variant.TrangThai)
                            .OrderBy(variant => variant.MaBienThe)
                            .Select(variant => (decimal?)variant.GiaBan)
                            .FirstOrDefault(),
                        SoLuongTon = product.SoLuongTon
                    })
                    .ToListAsync();

                viewModel.Categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.MaLoaiSP != 9)
                    .OrderBy(c => c.ThuTuHienThi)
                    .Select(category => new ShopCategoryViewModel { MaLoaiSP = category.MaLoaiSP, TenLoaiSP = category.TenLoaiSP })
                    .ToListAsync();

                viewModel.CustomerReviews = await _context.Feedbacks
                    .AsNoTracking()
                    .Where(f => f.DanhGia >= 4)
                    .OrderByDescending(f => f.NgayGui)
                    .Take(4)
                    .Select(feedback => new ProductReviewViewModel
                    {
                        CustomerName = feedback.Customer != null ? feedback.Customer.HoTen : "Khách hàng",
                        NoiDung = feedback.NoiDung,
                        DanhGia = feedback.DanhGia,
                        NgayGui = feedback.NgayGui
                    })
                    .ToListAsync();

                viewModel.TotalProducts = await _context.Products.CountAsync(p => p.TrangThai && p.MaLoaiSP != 9);
                viewModel.TotalOrders = await _context.Orders.CountAsync();
                viewModel.TotalCustomers = await _context.Customers.CountAsync();
                viewModel.TotalReviews = await _context.Feedbacks.CountAsync();
            }
            catch
            {
                // Fallback nếu database chưa có connection lúc dev view
            }

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string? subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ họ tên, email và nội dung lời nhắn.";
                return View();
            }

            try
            {
                var contact = new ContactMessage
                {
                    HoTen = name.Trim(),
                    Email = email.Trim(),
                    ChuDe = string.IsNullOrWhiteSpace(subject) ? "Không có chủ đề" : subject.Trim(),
                    NoiDung = message.Trim(),
                    NgayGui = DateTime.Now,
                    TrangThai = "Chưa xử lý"
                };

                _context.ContactMessages.Add(contact);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cảm ơn bạn đã liên hệ! Lời nhắn của bạn đã được chuyển tới ban Chăm sóc khách hàng của Katinat Coffee.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể gửi lời nhắn lúc này. Vui lòng thử lại sau: " + ex.Message;
                return View();
            }
        }

        [Route("Blog")]
        public async Task<IActionResult> Blog()
        {
            var categories = new List<HomeCategoryViewModel>();
            try
            {
                categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.MaLoaiSP != 9)
                    .OrderBy(c => c.ThuTuHienThi)
                    .Select(category => new HomeCategoryViewModel
                    {
                        TenLoaiSP = category.TenLoaiSP,
                        Products = category.Products.Where(product => product.TrangThai)
                            .OrderBy(product => product.TenSP)
                            .Select(product => new ShopProductCardViewModel
                            {
                                MaSP = product.MaSP,
                                TenSP = product.TenSP,
                                HinhAnh = product.HinhAnh,
                                TenLoaiSP = category.TenLoaiSP,
                                GiaBan = product.Variants.Where(variant => variant.TrangThai)
                                    .OrderBy(variant => variant.MaBienThe)
                                    .Select(variant => (decimal?)variant.GiaBan)
                                    .FirstOrDefault(),
                                SoLuongTon = product.SoLuongTon
                            }).ToList()
                    })
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
                    ViewBag.ProductExists = await _context.Products.AsNoTracking().AnyAsync(p => p.MaSP == id.Value);
                }

                ViewBag.RecentProducts = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.TrangThai && p.MaLoaiSP != 9)
                    .OrderBy(p => p.TenSP)
                    .Take(3)
                    .Select(product => new ShopProductCardViewModel
                    {
                        MaSP = product.MaSP,
                        TenSP = product.TenSP,
                        HinhAnh = product.HinhAnh,
                        TenLoaiSP = product.Category!.TenLoaiSP
                    })
                    .ToListAsync();
            }
            catch
            {
                ViewBag.RecentProducts = new List<ShopProductCardViewModel>();
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

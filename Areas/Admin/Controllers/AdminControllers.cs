using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public DashboardController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();
            try
            {
                viewModel.TotalRevenue = await _context.Orders
                    .Where(order => order.TrangThai == OrderStatuses.Completed)
                    .SumAsync(order => order.TongTien);
                viewModel.TotalOrders = await _context.Orders.CountAsync();
                viewModel.TotalCustomers = await _context.Customers.CountAsync();
                viewModel.TotalProducts = await _context.Products.CountAsync(p => p.TrangThai);
                viewModel.RecentOrders = await _context.Orders
                    .AsNoTracking()
                    .OrderByDescending(o => o.NgayTao)
                    .Take(10)
                    .Select(order => new AdminDashboardOrderViewModel
                    {
                        MaHD = order.MaHD,
                        HoTenNguoiNhan = order.HoTenNguoiNhan,
                        SdtNguoiNhan = order.SdtNguoiNhan,
                        DiaChiGiao = order.DiaChiGiao,
                        TongTien = order.TongTien,
                        NgayTao = order.NgayTao,
                        TrangThai = order.TrangThai
                    })
                    .ToListAsync();
            }
            catch
            {
                // Fallback nếu kết nối db đang thiết lập
            }

            return View(viewModel);
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff,NhanVien")]
    public class ProductsController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public ProductsController(CoffeeShopDbContext context)
        {
            _context = context;
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
    }
}

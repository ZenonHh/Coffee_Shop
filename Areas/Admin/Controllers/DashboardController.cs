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
}

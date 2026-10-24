using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NhanVien")]
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
                viewModel.TotalRevenue = await _context.Orders.Where(o => o.TrangThai == "Hoàn tất" || o.TrangThai == "Đã thanh toán").SumAsync(o => o.TongTien);
                viewModel.TotalOrders = await _context.Orders.CountAsync();
                viewModel.TotalCustomers = await _context.Customers.CountAsync();
                viewModel.TotalProducts = await _context.Products.CountAsync(p => p.TrangThai);
                viewModel.RecentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .OrderByDescending(o => o.NgayTao)
                    .Take(10)
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
    [Authorize(Roles = "Admin,NhanVien")]
    public class OrdersController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public OrdersController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.NgayTao)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.TrangThai == status);
            }

            var orders = await query.ToListAsync();
            ViewBag.SelectedStatus = status;
            return View(orders);
        }

                public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Variant)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Toppings).ThenInclude(t => t.ToppingProduct)
                .FirstOrDefaultAsync(o => o.MaHD == id);
            return order == null ? NotFound() : View(order);
        }
[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.TrangThai = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #{id} thành '{status}'.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    [Area("Admin")]
    [Authorize(Roles = "Admin,NhanVien")]
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
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.MaSP)
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

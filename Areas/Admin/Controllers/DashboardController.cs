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
    [Authorize(Roles = "Admin,Staff,NhanVien")]
    public class DashboardController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public DashboardController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalCustomerCount = await _context.Customers.CountAsync(),
                PendingReviewCount = await _context.Feedbacks.CountAsync(feedback =>
                    feedback.PhanHoiQuanTri == null || feedback.PhanHoiQuanTri == ""),
                PendingContactCount = await _context.ContactMessages.CountAsync(message => message.TrangThai == "Chưa xử lý"),
                ActiveSurveyCount = await _context.Surveys.CountAsync(survey =>
                    survey.NgayBatDau <= DateTime.Now && survey.NgayKetThuc >= DateTime.Now)
            };

            if (User.IsInRole("Admin"))
            {
                viewModel.TotalRevenue = await _context.Orders
                    .Where(order => order.TrangThai == OrderStatuses.Completed
                        || order.TrangThai == OrderStatuses.LegacyCompleted)
                    .SumAsync(order => order.TongTien);
                viewModel.PendingOrderCount = await _context.Orders.CountAsync(order =>
                    order.TrangThai == OrderStatuses.WaitingForConfirmation);

                // Thống kê doanh thu & đơn hàng 7 ngày gần nhất
                var today = DateTime.Today;
                var sevenDaysAgo = today.AddDays(-6);
                var tomorrow = today.AddDays(1);

                var recentOrders = await _context.Orders.AsNoTracking()
                    .Where(order => order.NgayTao >= sevenDaysAgo && order.NgayTao < tomorrow)
                    .Select(order => new { order.NgayTao, order.TongTien, order.TrangThai })
                    .ToListAsync();

                for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
                {
                    var currentDate = date.Date;
                    var dayOrders = recentOrders.Where(order => order.NgayTao.Date == currentDate).ToList();
                    var dayCompletedOrders = dayOrders.Where(order => order.TrangThai == OrderStatuses.Completed
                        || order.TrangThai == OrderStatuses.LegacyCompleted).ToList();

                    viewModel.DailyRevenues.Add(new DashboardDailyRevenueViewModel
                    {
                        Date = date.ToString("dd/MM"),
                        Revenue = dayCompletedOrders.Sum(order => order.TongTien),
                        OrderCount = dayOrders.Count
                    });
                }

                // Cơ cấu trạng thái đơn hàng
                var allOrdersStatusGroup = await _context.Orders.AsNoTracking()
                    .GroupBy(order => order.TrangThai)
                    .Select(group => new { Status = group.Key, Count = group.Count() })
                    .ToListAsync();

                static string GetStatusColor(string status) => status switch
                {
                    OrderStatuses.WaitingForConfirmation => "#d97706", // Amber
                    OrderStatuses.Confirmed => "#2563eb",              // Blue
                    OrderStatuses.Delivering => "#0284c7",             // Sky
                    OrderStatuses.Completed => "#16a34a",              // Green
                    OrderStatuses.LegacyCompleted => "#16a34a",        // Green
                    OrderStatuses.Cancelled => "#dc2626",              // Red
                    _ => "#8e8981"
                };

                viewModel.OrderStatusesList = allOrdersStatusGroup
                    .Select(group => new DashboardOrderStatusViewModel
                    {
                        Status = group.Status,
                        Count = group.Count,
                        Color = GetStatusColor(group.Status)
                    })
                    .OrderByDescending(status => status.Count)
                    .ToList();

                // Top 5 sản phẩm bán chạy nhất
                var topProductsData = await _context.OrderDetails.AsNoTracking()
                    .Where(detail => detail.Order != null
                        && (detail.Order.TrangThai == OrderStatuses.Completed || detail.Order.TrangThai == OrderStatuses.LegacyCompleted)
                        && detail.Product != null)
                    .GroupBy(detail => new { detail.MaSP, detail.Product!.TenSP })
                    .Select(group => new
                    {
                        ProductName = group.Key.TenSP,
                        Quantity = group.Sum(detail => detail.SoLuong),
                        Amount = group.Sum(detail => detail.ThanhTien)
                    })
                    .OrderByDescending(item => item.Quantity)
                    .Take(5)
                    .ToListAsync();

                viewModel.TopSellingProducts = topProductsData.Select(product => new DashboardTopProductViewModel
                {
                    ProductName = product.ProductName,
                    QuantitySold = product.Quantity,
                    TotalAmount = product.Amount
                }).ToList();
            }

            return View(viewModel);
        }
    }
}

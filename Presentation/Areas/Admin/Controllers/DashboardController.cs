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
    [Authorize(Roles = "Admin,Staff,NhanVien,CRM")]
    public class DashboardController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public DashboardController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6);
            var tomorrow = today.AddDays(1);
            var thirtyDaysAgo = today.AddDays(-29);
            var viewModel = new AdminDashboardViewModel
            {
                TotalCustomerCount = await _context.Customers.CountAsync(),
                PendingReviewCount = await _context.Feedbacks.CountAsync(feedback =>
                    feedback.PhanHoiQuanTri == null || feedback.PhanHoiQuanTri == ""),
                PendingContactCount = await _context.ContactMessages.CountAsync(message => message.TrangThai == "Chưa xử lý"),
                ActiveSurveyCount = await _context.Surveys.CountAsync(survey =>
                    survey.NgayBatDau <= now && survey.NgayKetThuc >= now)
            };

            var customerRegistrations = await _context.Customers.AsNoTracking()
                .Where(customer => customer.NgayTao >= sevenDaysAgo && customer.NgayTao < tomorrow)
                .Select(customer => customer.NgayTao)
                .ToListAsync();
            for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
            {
                var currentDate = date.Date;
                viewModel.CustomerTrend.Add(new DashboardCustomerTrendViewModel
                {
                    Date = date.ToString("dd/MM"),
                    CustomerCount = customerRegistrations.Count(registration => registration.Date == currentDate)
                });
            }

            var recentRatings = await _context.Feedbacks.AsNoTracking()
                .Where(feedback => feedback.NgayGui >= thirtyDaysAgo
                    && feedback.NgayGui < tomorrow
                    && feedback.DanhGia.HasValue)
                .GroupBy(feedback => feedback.DanhGia!.Value)
                .Select(group => new
                {
                    Rating = group.Key,
                    Count = group.Count(),
                    Average = group.Average(feedback => feedback.DanhGia!.Value)
                })
                .ToListAsync();
            viewModel.SatisfactionResponseCount = recentRatings.Sum(item => item.Count);
            viewModel.AverageSatisfaction = viewModel.SatisfactionResponseCount == 0
                ? null
                : (decimal)(recentRatings.Sum(item => item.Average * item.Count)
                    / viewModel.SatisfactionResponseCount);
            viewModel.SatisfactionRatings = Enumerable.Range(1, 5)
                .Select(rating => new DashboardRatingViewModel
                {
                    Rating = rating,
                    Count = recentRatings.FirstOrDefault(item => item.Rating == rating)?.Count ?? 0
                })
                .ToList();

            var activeSurveyData = await _context.Surveys.AsNoTracking()
                .Where(survey => survey.NgayBatDau <= now && survey.NgayKetThuc >= now)
                .OrderByDescending(survey => survey.NgayKetThuc)
                .Select(survey => new
                {
                    survey.TieuDe,
                    TargetCount = survey.DoiTuong == "TatCa"
                        ? _context.Customers.Count()
                        : survey.AudienceCustomers.Count(),
                    ResponseCount = survey.Responses.Count()
                })
                .ToListAsync();
            viewModel.SurveyParticipation = activeSurveyData
                .Take(5)
                .Select(survey => new DashboardSurveyParticipationViewModel
                {
                    Title = survey.TieuDe,
                    TargetCount = survey.TargetCount,
                    ResponseCount = survey.ResponseCount,
                    Percentage = survey.TargetCount == 0
                        ? 0
                        : Math.Round(survey.ResponseCount * 100d / survey.TargetCount, 1)
                })
                .ToList();

            if (User.IsInRole("Admin"))
            {
                viewModel.TotalRevenue = await _context.Orders
                    .Where(order => order.TrangThai != OrderStatuses.Cancelled)
                    .SumAsync(order => order.TongTien);
                viewModel.PendingOrderCount = await _context.Orders.CountAsync(order =>
                    order.TrangThai == OrderStatuses.WaitingForConfirmation);

                var oneYearAgo = today.AddYears(-1);

                // Lấy đơn hàng trong 1 năm qua để tính thống kê Ngày, Tuần, Tháng
                var pastOrders = await _context.Orders.AsNoTracking()
                    .Where(order => order.NgayTao >= oneYearAgo && order.NgayTao < tomorrow)
                    .Select(order => new { order.NgayTao, order.TongTien, order.TrangThai })
                    .ToListAsync();

                static bool IsCompleted(string status) => status != OrderStatuses.Cancelled;

                // 1. Thống kê theo Ngày (7 ngày qua)
                for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
                {
                    var currentDate = date.Date;
                    var dayOrders = pastOrders.Where(order => order.NgayTao.Date == currentDate).ToList();
                    var dayCompletedOrders = dayOrders.Where(order => IsCompleted(order.TrangThai)).ToList();

                    var stat = new DashboardChartPeriodViewModel
                    {
                        Label = date.ToString("dd'/'MM", System.Globalization.CultureInfo.InvariantCulture),
                        Revenue = dayCompletedOrders.Sum(order => order.TongTien),
                        OrderCount = dayOrders.Count
                    };

                    viewModel.DailyStats.Add(stat);
                    viewModel.DailyRevenues.Add(new DashboardDailyRevenueViewModel
                    {
                        Date = stat.Label,
                        Revenue = stat.Revenue,
                        OrderCount = stat.OrderCount
                    });
                }

                // 2. Thống kê theo Tuần (8 tuần gần nhất)
                int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var currentWeekMonday = today.AddDays(-diff);
                var eightWeeksAgoMonday = currentWeekMonday.AddDays(-7 * 7);

                for (int w = 0; w < 8; w++)
                {
                    var weekStart = eightWeeksAgoMonday.AddDays(w * 7);
                    var weekEnd = weekStart.AddDays(6);
                    var weekEndExclusive = weekEnd.AddDays(1);

                    var weekOrders = pastOrders
                        .Where(order => order.NgayTao >= weekStart && order.NgayTao < weekEndExclusive)
                        .ToList();
                    var weekCompletedOrders = weekOrders.Where(order => IsCompleted(order.TrangThai)).ToList();

                    viewModel.WeeklyStats.Add(new DashboardChartPeriodViewModel
                    {
                        Label = $"{weekStart.ToString("dd'/'MM", System.Globalization.CultureInfo.InvariantCulture)} - {weekEnd.ToString("dd'/'MM", System.Globalization.CultureInfo.InvariantCulture)}",
                        Revenue = weekCompletedOrders.Sum(order => order.TongTien),
                        OrderCount = weekOrders.Count
                    });
                }

                // 3. Thống kê theo Tháng (6 tháng gần nhất)
                var currentMonthStart = new DateTime(today.Year, today.Month, 1);
                for (int m = 5; m >= 0; m--)
                {
                    var monthDate = currentMonthStart.AddMonths(-m);
                    var nextMonth = monthDate.AddMonths(1);

                    var monthOrders = pastOrders
                        .Where(order => order.NgayTao >= monthDate && order.NgayTao < nextMonth)
                        .ToList();
                    var monthCompletedOrders = monthOrders.Where(order => IsCompleted(order.TrangThai)).ToList();

                    viewModel.MonthlyStats.Add(new DashboardChartPeriodViewModel
                    {
                        Label = $"T{monthDate.Month:00}/{monthDate.Year}",
                        Revenue = monthCompletedOrders.Sum(order => order.TongTien),
                        OrderCount = monthOrders.Count
                    });
                }

                // Cơ cấu trạng thái đơn hàng: Chuẩn hóa đồng nhất 5 trạng thái theo đúng phân hệ Quản lý đơn hàng
                var allOrderStatuses = await _context.Orders.AsNoTracking()
                    .Select(order => order.TrangThai)
                    .ToListAsync();

                var canonicalStatuses = new[]
                {
                    new { Status = OrderStatuses.WaitingForConfirmation, Color = "#f59e0b" }, // Chờ xác nhận
                    new { Status = OrderStatuses.Confirmed,              Color = "#3b82f6" }, // Đã xác nhận
                    new { Status = OrderStatuses.Delivering,             Color = "#0ea5e9" }, // Đang giao
                    new { Status = OrderStatuses.Completed,              Color = "#10b981" }, // Hoàn thành (kể cả Hoàn tất)
                    new { Status = OrderStatuses.Cancelled,              Color = "#ef4444" }  // Đã hủy
                };

                var statusCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    [OrderStatuses.WaitingForConfirmation] = 0,
                    [OrderStatuses.Confirmed] = 0,
                    [OrderStatuses.Delivering] = 0,
                    [OrderStatuses.Completed] = 0,
                    [OrderStatuses.Cancelled] = 0
                };

                foreach (var rawStatus in allOrderStatuses)
                {
                    var canonical = OrderStatuses.Canonicalize(rawStatus ?? string.Empty);
                    if (statusCounts.ContainsKey(canonical))
                    {
                        statusCounts[canonical]++;
                    }
                    else
                    {
                        statusCounts[OrderStatuses.WaitingForConfirmation]++;
                    }
                }

                viewModel.OrderStatusesList = canonicalStatuses
                    .Select(item => new DashboardOrderStatusViewModel
                    {
                        Status = item.Status,
                        Count = statusCounts[item.Status],
                        Color = item.Color
                    })
                    .ToList();

                // Top 5 sản phẩm bán chạy nhất
                var topProductsData = await _context.OrderDetails.AsNoTracking()
                    .Where(detail => detail.Order != null
                        && detail.Order.TrangThai != OrderStatuses.Cancelled
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

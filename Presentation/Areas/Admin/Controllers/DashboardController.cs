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

        public async Task<IActionResult> Index(
            string productPeriod = "quarter",
            int? productYear = null,
            int? productQuarter = null,
            int? productMonth = null,
            DateTime? productFromDate = null,
            DateTime? productToDate = null)
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

                var period = (productPeriod ?? string.Empty).ToLowerInvariant();
                if (period is not ("quarter" or "month" or "year" or "custom"))
                {
                    ModelState.AddModelError(string.Empty, "Kiểu thời gian không hợp lệ. Đã chuyển về quý hiện tại.");
                    period = "quarter";
                }

                var selectedYear = productYear ?? today.Year;
                if (selectedYear < 2000 || selectedYear > today.Year)
                {
                    ModelState.AddModelError(string.Empty, $"Năm phải nằm trong khoảng 2000 đến {today.Year}.");
                    selectedYear = Math.Clamp(selectedYear, 2000, today.Year);
                }

                var selectedQuarter = productQuarter ?? ((today.Month - 1) / 3 + 1);
                if (selectedQuarter < 1 || selectedQuarter > 4)
                {
                    ModelState.AddModelError(string.Empty, "Quý phải nằm trong khoảng từ 1 đến 4.");
                    selectedQuarter = Math.Clamp(selectedQuarter, 1, 4);
                }

                var selectedMonth = productMonth ?? today.Month;
                if (selectedMonth < 1 || selectedMonth > 12)
                {
                    ModelState.AddModelError(string.Empty, "Tháng phải nằm trong khoảng từ 1 đến 12.");
                    selectedMonth = Math.Clamp(selectedMonth, 1, 12);
                }

                var rangeStart = new DateTime(selectedYear, (selectedQuarter - 1) * 3 + 1, 1);
                var rangeEnd = rangeStart.AddMonths(3);
                var periodLabel = $"Quý {selectedQuarter}/{selectedYear}";

                if (period == "month")
                {
                    rangeStart = new DateTime(selectedYear, selectedMonth, 1);
                    rangeEnd = rangeStart.AddMonths(1);
                    periodLabel = $"Tháng {selectedMonth:00}/{selectedYear}";
                }
                else if (period == "year")
                {
                    rangeStart = new DateTime(selectedYear, 1, 1);
                    rangeEnd = rangeStart.AddYears(1);
                    periodLabel = $"Năm {selectedYear}";
                }
                else if (period == "custom")
                {
                    if (productFromDate.HasValue
                        && productToDate.HasValue
                        && productFromDate.Value.Date <= productToDate.Value.Date)
                    {
                        rangeStart = productFromDate.Value.Date;
                        rangeEnd = productToDate.Value.Date.AddDays(1);
                        periodLabel = $"Từ {rangeStart:dd/MM/yyyy} đến {rangeEnd.AddDays(-1):dd/MM/yyyy}";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Vui lòng chọn khoảng ngày hợp lệ (từ ngày không lớn hơn đến ngày). Đã chuyển về quý đã chọn.");
                        period = "quarter";
                    }
                }

                viewModel.ProductRevenuePeriod = period;
                viewModel.ProductRevenueYear = selectedYear;
                viewModel.ProductRevenueQuarter = selectedQuarter;
                viewModel.ProductRevenueMonth = selectedMonth;
                viewModel.ProductRevenueFromDate = productFromDate?.Date;
                viewModel.ProductRevenueToDate = productToDate?.Date;
                viewModel.ProductRevenuePeriodLabel = periodLabel;

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

                var topRevenueProducts = await _context.OrderDetails.AsNoTracking()
                    .Where(detail => detail.Order != null
                        && detail.Order.TrangThai != OrderStatuses.Cancelled
                        && detail.Product != null
                        && detail.Order.NgayTao >= rangeStart
                        && detail.Order.NgayTao < rangeEnd)
                    .GroupBy(detail => new { detail.MaSP, detail.Product!.TenSP })
                    .Select(group => new
                    {
                        ProductId = group.Key.MaSP,
                        ProductName = group.Key.TenSP,
                        Quantity = group.Sum(detail => detail.SoLuong),
                        Revenue = group.Sum(detail => detail.ThanhTien)
                    })
                    .OrderByDescending(item => item.Revenue)
                    .ThenByDescending(item => item.Quantity)
                    .Take(10)
                    .ToListAsync();

                viewModel.TopRevenueProducts = topRevenueProducts.Select(product => new DashboardTopRevenueProductViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    QuantitySold = product.Quantity,
                    TotalRevenue = product.Revenue
                }).ToList();
            }

            return View(viewModel);
        }
    }
}

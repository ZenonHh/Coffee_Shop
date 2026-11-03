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
            var viewModel = new AdminDashboardViewModel
            {
                TotalRevenue = await _context.Orders
                    .Where(order => order.TrangThai == OrderStatuses.Completed
                        || order.TrangThai == OrderStatuses.LegacyCompleted)
                    .SumAsync(order => order.TongTien),
                PendingOrderCount = await _context.Orders.CountAsync(order =>
                    order.TrangThai == OrderStatuses.WaitingForConfirmation),
                UnprocessedFeedbackCount =
                    await _context.ContactMessages.CountAsync(message => message.TrangThai == "Chưa xử lý")
                    + await _context.Feedbacks.CountAsync(feedback =>
                        feedback.PhanHoiQuanTri == null || feedback.PhanHoiQuanTri == ""),
                ActiveSurveyCount = await _context.Surveys.CountAsync(survey =>
                    survey.NgayBatDau <= DateTime.Now && survey.NgayKetThuc >= DateTime.Now),
                RatedFeedbackCount = await _context.Feedbacks.CountAsync(feedback => feedback.DanhGia.HasValue),
                AverageSatisfaction = await _context.Feedbacks
                    .Where(feedback => feedback.DanhGia.HasValue)
                    .Select(feedback => (double?)feedback.DanhGia)
                    .AverageAsync() ?? 0,
                RecentOrders = await _context.Orders
                    .AsNoTracking()
                    .OrderByDescending(order => order.NgayTao)
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
                    .ToListAsync()
            };

            var today = DateTime.Today;
            var customerBirthDates = await _context.Customers.AsNoTracking()
                .Select(customer => customer.NgaySinh)
                .ToListAsync();
            var ageGroupResponses = customerBirthDates
                .Select(birthDate => CustomerCrmOptions.GetAgeGroup(birthDate, today))
                .Where(group => group != null)
                .ToList();
            viewModel.AgeGroupStats = CustomerCrmOptions.AgeGroups
                .Select(group => new CrmSegmentViewModel
                {
                    Label = group,
                    Count = ageGroupResponses.Count(response => response == group),
                    Percentage = ageGroupResponses.Count == 0
                        ? 0
                        : Math.Round(ageGroupResponses.Count(response => response == group) * 100.0
                            / ageGroupResponses.Count, 1)
                })
                .ToList();

            var purchasedInterests = await _context.OrderDetails.AsNoTracking()
                .Where(detail => detail.Order != null
                    && detail.Order.MaKH.HasValue
                    && (detail.Order.TrangThai == OrderStatuses.Completed
                        || detail.Order.TrangThai == OrderStatuses.LegacyCompleted)
                    && detail.Product != null
                    && detail.Product.Category != null)
                .Select(detail => new
                {
                    CustomerId = detail.Order!.MaKH!.Value,
                    Interest = detail.Product!.Category!.TenLoaiSP
                })
                .ToListAsync();
            var customersWithPurchases = purchasedInterests
                .Select(item => item.CustomerId)
                .Distinct()
                .Count();
            viewModel.InterestStats = purchasedInterests
                .GroupBy(item => item.Interest)
                .Select(group => new CrmSegmentViewModel
                {
                    Label = group.Key,
                    Count = group.Select(item => item.CustomerId).Distinct().Count(),
                    Percentage = customersWithPurchases == 0
                        ? 0
                        : Math.Round(group.Select(item => item.CustomerId).Distinct().Count() * 100.0
                            / customersWithPurchases, 1)
                })
                .OrderByDescending(item => item.Count)
                .ToList();

            foreach (var order in viewModel.RecentOrders)
            {
                order.TrangThai = OrderStatuses.Canonicalize(order.TrangThai);
            }

            return View(viewModel);
        }
    }
}

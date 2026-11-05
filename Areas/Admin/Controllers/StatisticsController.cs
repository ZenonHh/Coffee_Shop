using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class StatisticsController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public StatisticsController(CoffeeShopDbContext context) => _context = context;

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string tab = "customers",
        string period = "30days",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? year = null)
    {
        tab = tab is "satisfaction" or "surveys" or "finance" ? tab : "customers";
        period = period is "quarter" or "year" or "custom" ? period : "30days";
        var today = DateTime.Today;
        var financeYear = year ?? today.Year;
        var start = period switch
        {
            "quarter" => new DateTime(today.Year, ((today.Month - 1) / 3 * 3) + 1, 1),
            "year" => new DateTime(today.Year, 1, 1),
            "custom" => fromDate?.Date ?? today.AddDays(-29),
            _ => today.AddDays(-29)
        };
        var end = period == "custom" ? toDate?.Date ?? today : today;
        var model = new CrmStatisticsViewModel
        {
            Tab = tab,
            Period = period,
            FromDate = start,
            ToDate = end,
            FinanceYear = financeYear
        };

        if (tab == "finance" && (financeYear < 2000 || financeYear > today.Year))
            ModelState.AddModelError(nameof(year), $"Năm phải nằm trong khoảng 2000–{today.Year}.");
        if (period == "custom" && (!fromDate.HasValue || !toDate.HasValue))
            ModelState.AddModelError(nameof(fromDate), "Vui lòng chọn cả ngày bắt đầu và ngày kết thúc.");
        if (start > end)
            ModelState.AddModelError(nameof(fromDate), "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thống kê";
            return View(model);
        }
        model.RangeIsValid = true;

        if (tab == "finance")
        {
            await PopulateFinancialStatisticsAsync(model, financeYear);
            ViewData["Title"] = "Thống kê";
            return View(model);
        }

        var endExclusive = end.AddDays(1);
        model.TotalCustomers = await _context.Customers.CountAsync();
        model.NewCustomers = await _context.Customers.CountAsync(customer =>
            customer.NgayTao >= start && customer.NgayTao < endExclusive);

        var todayForAge = DateTime.Today;
        var birthDates = await _context.Customers.AsNoTracking()
            .Select(customer => customer.NgaySinh)
            .ToListAsync();
        var knownAgeGroups = birthDates
            .Select(birthDate => CustomerCrmOptions.GetAgeGroup(birthDate, todayForAge))
            .Where(group => group != null)
            .ToList();
        model.AgeGroups = CustomerCrmOptions.AgeGroups
            .Select(group => new CrmSegmentViewModel
            {
                Label = group,
                Count = knownAgeGroups.Count(value => value == group),
                Percentage = model.TotalCustomers == 0 ? 0
                    : Math.Round(knownAgeGroups.Count(value => value == group) * 100d / model.TotalCustomers, 1)
            })
            .ToList();
        var missingBirthDates = birthDates.Count - knownAgeGroups.Count;
        model.AgeGroups.Add(new CrmSegmentViewModel
        {
            Label = "Chưa khai báo",
            Count = missingBirthDates,
            Percentage = model.TotalCustomers == 0 ? 0
                : Math.Round(missingBirthDates * 100d / model.TotalCustomers, 1)
        });

        model.LoyaltyTiers = await _context.Customers.AsNoTracking()
            .GroupBy(customer => customer.HangTV)
            .Select(group => new CrmSegmentViewModel { Label = group.Key, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ToListAsync();
        foreach (var tier in model.LoyaltyTiers)
            tier.Percentage = model.TotalCustomers == 0 ? 0 : Math.Round(tier.Count * 100d / model.TotalCustomers, 1);

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
        model.Interests = purchasedInterests
            .GroupBy(item => item.Interest)
            .Select(group => new CrmSegmentViewModel
            {
                Label = group.Key,
                Count = group.Select(item => item.CustomerId).Distinct().Count(),
                Percentage = model.TotalCustomers == 0 ? 0
                    : Math.Round(group.Select(item => item.CustomerId).Distinct().Count() * 100d / model.TotalCustomers, 1)
            })
            .OrderByDescending(item => item.Count)
            .ToList();

        var feedbackQuery = _context.Feedbacks.AsNoTracking()
            .Where(feedback => feedback.NgayGui >= start && feedback.NgayGui < endExclusive);
        model.TotalFeedbackCount = await feedbackQuery.CountAsync();
        model.RespondedCustomerCount = await feedbackQuery.Select(feedback => feedback.MaKH).Distinct().CountAsync();
        var ratedFeedback = await feedbackQuery
            .Where(feedback => feedback.DanhGia.HasValue)
            .Select(feedback => new { feedback.DanhGia, feedback.NgayGui })
            .ToListAsync();
        model.RatedFeedbackCount = ratedFeedback.Count;
        model.AverageSatisfaction = ratedFeedback.Count == 0 ? null : ratedFeedback.Average(item => item.DanhGia!.Value);
        model.Ratings = Enumerable.Range(1, 5)
            .Select(rating =>
            {
                var count = ratedFeedback.Count(item => item.DanhGia == rating);
                return new CrmRatingViewModel
                {
                    Rating = rating,
                    Count = count,
                    Percentage = ratedFeedback.Count == 0 ? 0 : Math.Round(count * 100d / ratedFeedback.Count, 1)
                };
            })
            .ToList();

        var productRatings = await feedbackQuery
            .Where(feedback => feedback.DanhGia.HasValue && feedback.MaSP.HasValue)
            .GroupBy(feedback => new { feedback.MaSP, Name = feedback.Product!.TenSP })
            .Select(group => new CrmProductRatingViewModel
            {
                Name = group.Key.Name,
                Count = group.Count(),
                Average = group.Average(feedback => feedback.DanhGia!.Value)
            })
            .Where(product => product.Count > 0)
            .ToListAsync();
        model.TopProducts = productRatings.OrderByDescending(product => product.Average)
            .ThenByDescending(product => product.Count).Take(5).ToList();
        model.LowestProducts = productRatings.OrderBy(product => product.Average)
            .ThenByDescending(product => product.Count).Take(5).ToList();
        model.MonthlyRatings = ratedFeedback
            .GroupBy(item => new { item.NgayGui.Year, item.NgayGui.Month })
            .OrderBy(group => group.Key.Year).ThenBy(group => group.Key.Month)
            .Select(group => new CrmMonthlyRatingViewModel
            {
                Month = $"{group.Key.Month:00}/{group.Key.Year}",
                Count = group.Count(),
                Average = Math.Round(group.Average(item => item.DanhGia!.Value), 1)
            })
            .ToList();

        var surveys = await _context.Surveys.AsNoTracking()
            .OrderByDescending(survey => survey.NgayBatDau)
            .Select(survey => new
            {
                survey.MaKhaoSat,
                survey.TieuDe,
                TargetCount = survey.DoiTuong == "TatCa"
                    ? _context.Customers.Count()
                    : survey.AudienceCustomers.Count(),
                ResponseCount = survey.Responses.Count(response =>
                    response.NgayHoanThanh >= start && response.NgayHoanThanh < endExclusive)
            })
            .ToListAsync();
        model.Surveys = surveys.Select(survey => new CrmSurveyParticipationViewModel
        {
            Id = survey.MaKhaoSat,
            Title = survey.TieuDe,
            TargetCount = survey.TargetCount,
            ResponseCount = survey.ResponseCount,
            Percentage = survey.TargetCount == 0 ? 0
                : Math.Round(survey.ResponseCount * 100d / survey.TargetCount, 1)
        }).ToList();

        ViewData["Title"] = "Thống kê";
        return View(model);
    }

    private async Task PopulateFinancialStatisticsAsync(CrmStatisticsViewModel model, int year)
    {
        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = yearStart.AddYears(1);
        var completedOrders = _context.Orders.AsNoTracking()
            .Where(order =>
                (order.TrangThai == OrderStatuses.Completed || order.TrangThai == OrderStatuses.LegacyCompleted)
                && order.NgayTao >= yearStart
                && order.NgayTao < yearEnd);

        model.CompletedInvoiceCount = await completedOrders.CountAsync();
        model.TotalCompletedRevenue = await completedOrders
            .SumAsync(order => (decimal?)order.TongTien) ?? 0m;

        var revenueByQuarter = await completedOrders
            .GroupBy(order => ((order.NgayTao.Month - 1) / 3) + 1)
            .Select(group => new { Quarter = group.Key, Amount = group.Sum(order => order.TongTien) })
            .ToDictionaryAsync(item => item.Quarter, item => item.Amount);

        var approvedPurchases = _context.Purchases.AsNoTracking()
            .Where(purchase => purchase.TrangThai == "Đã duyệt"
                && (purchase.NgayDuyet ?? purchase.NgayNhap) >= yearStart
                && (purchase.NgayDuyet ?? purchase.NgayNhap) < yearEnd);
        var purchasesByQuarter = await approvedPurchases
            .GroupBy(purchase => ((((purchase.NgayDuyet ?? purchase.NgayNhap)!.Value.Month - 1) / 3) + 1))
            .Select(group => new { Quarter = group.Key, Amount = group.Sum(purchase => purchase.TongTien ?? 0m) })
            .ToDictionaryAsync(item => item.Quarter, item => item.Amount);

        model.QuarterlyFinance = Enumerable.Range(1, 4)
            .Select(quarter => new QuarterFinanceViewModel
            {
                Quarter = quarter,
                Revenue = revenueByQuarter.GetValueOrDefault(quarter),
                Purchases = purchasesByQuarter.GetValueOrDefault(quarter)
            })
            .ToList();
        model.TotalApprovedPurchases = model.QuarterlyFinance.Sum(item => item.Purchases);
        model.RevenueMinusPurchases = model.TotalCompletedRevenue - model.TotalApprovedPurchases;

        var firstStaffReportYear = Math.Max(2000, year - 2);
        model.StaffReportYears = Enumerable.Range(firstStaffReportYear, year - firstStaffReportYear + 1).ToList();
        var staffSales = await _context.Orders.AsNoTracking()
            .Where(order => order.MaNV.HasValue
                && (order.TrangThai == OrderStatuses.Completed || order.TrangThai == OrderStatuses.LegacyCompleted)
                && order.NgayTao >= new DateTime(model.StaffReportYears[0], 1, 1)
                && order.NgayTao < yearEnd)
            .GroupBy(order => new { StaffId = order.MaNV!.Value, order.NgayTao.Year })
            .Select(group => new
            {
                group.Key.StaffId,
                group.Key.Year,
                Revenue = group.Sum(order => order.TongTien)
            })
            .ToListAsync();
        var staffRevenueLookup = staffSales.ToDictionary(
            item => (item.StaffId, item.Year),
            item => item.Revenue);
        var staffMembers = await _context.StaffMembers.AsNoTracking()
            .OrderBy(staff => staff.HoVaDem)
            .ThenBy(staff => staff.Ten)
            .Select(staff => new { staff.MaNV, staff.HoVaDem, staff.Ten })
            .ToListAsync();
        model.StaffRevenue = staffMembers.Select(staff => new StaffRevenueViewModel
        {
            Name = $"{staff.HoVaDem} {staff.Ten}",
            RevenueByYear = model.StaffReportYears.ToDictionary(
                reportYear => reportYear,
                reportYear => staffRevenueLookup.GetValueOrDefault((staff.MaNV, reportYear)))
        }).ToList();

        var productSales = await _context.OrderDetails.AsNoTracking()
            .Where(detail => detail.Order != null
                && (detail.Order.TrangThai == OrderStatuses.Completed || detail.Order.TrangThai == OrderStatuses.LegacyCompleted)
                && detail.Order.NgayTao >= yearStart
                && detail.Order.NgayTao < yearEnd
                && detail.Product != null)
            .GroupBy(detail => new
            {
                detail.MaSP,
                Name = detail.Product!.TenSP,
                Quarter = ((detail.Order!.NgayTao.Month - 1) / 3) + 1
            })
            .Select(group => new
            {
                group.Key.MaSP,
                group.Key.Name,
                group.Key.Quarter,
                Revenue = group.Sum(detail => detail.ThanhTien)
            })
            .ToListAsync();
        model.ProductQuarterRevenue = productSales
            .GroupBy(item => new { item.MaSP, item.Name })
            .OrderBy(group => group.Key.Name)
            .Select(group => new ProductQuarterRevenueViewModel
            {
                Name = group.Key.Name,
                RevenueByQuarter = group.ToDictionary(item => item.Quarter, item => item.Revenue)
            })
            .ToList();
    }
}

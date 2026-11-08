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
[Authorize(Roles = "Admin,Staff,NhanVien,CRM")]
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
        int? year = null,
        string financePeriod = "quarter")
    {
        tab = tab is "satisfaction" or "surveys" or "finance" ? tab : "customers";
        if (User.IsInRole("CRM") && string.Equals(tab, "finance", StringComparison.OrdinalIgnoreCase))
            return Forbid();
        period = period is "quarter" or "year" or "custom" ? period : "30days";
        financePeriod = financePeriod is "month" or "year" ? financePeriod : "quarter";
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
            FinanceYear = financeYear,
            FinancePeriod = financePeriod
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

    [HttpGet("ExportExcel")]
    public async Task<IActionResult> ExportExcel(
        string tab = "customers",
        string period = "30days",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? year = null,
        string financePeriod = "quarter")
    {
        if (User.IsInRole("CRM") && string.Equals(tab, "finance", StringComparison.OrdinalIgnoreCase))
            return Forbid();
        var model = await GetExportModelAsync(tab, period, fromDate, toDate, year, financePeriod);
        if (model == null)
            return BadRequest("Bộ lọc báo cáo không hợp lệ.");

        return File(
            StatisticsReportExporter.ExportExcel(model),
            StatisticsReportExporter.ExcelContentType,
            StatisticsReportExporter.GetFileName(model, "xlsx"));
    }

    [HttpGet("ExportPdf")]
    public async Task<IActionResult> ExportPdf(
        string tab = "customers",
        string period = "30days",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? year = null,
        string financePeriod = "quarter")
    {
        if (User.IsInRole("CRM") && string.Equals(tab, "finance", StringComparison.OrdinalIgnoreCase))
            return Forbid();
        var model = await GetExportModelAsync(tab, period, fromDate, toDate, year, financePeriod);
        if (model == null)
            return BadRequest("Bộ lọc báo cáo không hợp lệ.");

        return File(
            StatisticsReportExporter.ExportPdf(model),
            StatisticsReportExporter.PdfContentType,
            StatisticsReportExporter.GetFileName(model, "pdf"));
    }

    private async Task<CrmStatisticsViewModel?> GetExportModelAsync(
        string tab,
        string period,
        DateTime? fromDate,
        DateTime? toDate,
        int? year,
        string financePeriod)
    {
        var result = await Index(tab, period, fromDate, toDate, year, financePeriod);
        return result is ViewResult view
            && view.Model is CrmStatisticsViewModel model
            && model.RangeIsValid
                ? model
                : null;
    }

    private async Task PopulateFinancialStatisticsAsync(CrmStatisticsViewModel model, int year)
    {
        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = yearStart.AddYears(1);
        var firstReportYear = model.FinancePeriod == "year" ? Math.Max(2000, year - 2) : year;
        var reportStart = new DateTime(firstReportYear, 1, 1);
        var periodOrders = _context.Orders.AsNoTracking()
            .Where(order =>
                (order.TrangThai == OrderStatuses.Completed || order.TrangThai == OrderStatuses.LegacyCompleted)
                && order.NgayTao >= reportStart
                && order.NgayTao < yearEnd);
        var selectedYearOrders = periodOrders.Where(order => order.NgayTao >= yearStart);

        model.CompletedInvoiceCount = await selectedYearOrders.CountAsync();
        model.TotalCompletedRevenue = await selectedYearOrders
            .SumAsync(order => (decimal?)order.TongTien) ?? 0m;

        var approvedPurchases = _context.Purchases.AsNoTracking()
            .Where(purchase => purchase.TrangThai == "Đã duyệt"
                && (purchase.NgayDuyet ?? purchase.NgayNhap) >= yearStart
                && (purchase.NgayDuyet ?? purchase.NgayNhap) < yearEnd);
        model.TotalApprovedPurchases = await approvedPurchases
            .SumAsync(purchase => (decimal?)purchase.TongTien) ?? 0m;
        model.RevenueMinusPurchases = model.TotalCompletedRevenue - model.TotalApprovedPurchases;

        var purchasesForPeriods = _context.Purchases.AsNoTracking()
            .Where(purchase => purchase.TrangThai == "Đã duyệt"
                && (purchase.NgayDuyet ?? purchase.NgayNhap) >= reportStart
                && (purchase.NgayDuyet ?? purchase.NgayNhap) < yearEnd);
        Dictionary<int, decimal> revenueByPeriod;
        Dictionary<int, decimal> purchasesByPeriod;
        if (model.FinancePeriod == "month")
        {
            revenueByPeriod = await periodOrders
                .GroupBy(order => order.NgayTao.Month)
                .Select(group => new { Period = group.Key, Amount = group.Sum(order => order.TongTien) })
                .ToDictionaryAsync(item => item.Period, item => item.Amount);
            purchasesByPeriod = await purchasesForPeriods
                .GroupBy(purchase => (purchase.NgayDuyet ?? purchase.NgayNhap)!.Value.Month)
                .Select(group => new { Period = group.Key, Amount = group.Sum(purchase => purchase.TongTien ?? 0m) })
                .ToDictionaryAsync(item => item.Period, item => item.Amount);
        }
        else if (model.FinancePeriod == "quarter")
        {
            revenueByPeriod = await periodOrders
                .GroupBy(order => ((order.NgayTao.Month - 1) / 3) + 1)
                .Select(group => new { Period = group.Key, Amount = group.Sum(order => order.TongTien) })
                .ToDictionaryAsync(item => item.Period, item => item.Amount);
            purchasesByPeriod = await purchasesForPeriods
                .GroupBy(purchase => ((((purchase.NgayDuyet ?? purchase.NgayNhap)!.Value.Month - 1) / 3) + 1))
                .Select(group => new { Period = group.Key, Amount = group.Sum(purchase => purchase.TongTien ?? 0m) })
                .ToDictionaryAsync(item => item.Period, item => item.Amount);
        }
        else
        {
            revenueByPeriod = await periodOrders
                .GroupBy(order => order.NgayTao.Year)
                .Select(group => new { Period = group.Key, Amount = group.Sum(order => order.TongTien) })
                .ToDictionaryAsync(item => item.Period, item => item.Amount);
            purchasesByPeriod = await purchasesForPeriods
                .GroupBy(purchase => (purchase.NgayDuyet ?? purchase.NgayNhap)!.Value.Year)
                .Select(group => new { Period = group.Key, Amount = group.Sum(purchase => purchase.TongTien ?? 0m) })
                .ToDictionaryAsync(item => item.Period, item => item.Amount);
        }

        var periods = model.FinancePeriod switch
        {
            "month" => Enumerable.Range(1, 12).Select(month => (month, $"Tháng {month}")),
            "year" => Enumerable.Range(firstReportYear, year - firstReportYear + 1)
                .Select(reportYear => (reportYear, reportYear.ToString())),
            _ => Enumerable.Range(1, 4).Select(quarter => (quarter, $"Quý {quarter}"))
        };
        model.FinancialPeriods = periods
            .Select(period => new FinancePeriodViewModel
            {
                Label = period.Item2,
                Revenue = revenueByPeriod.GetValueOrDefault(period.Item1),
                Purchases = purchasesByPeriod.GetValueOrDefault(period.Item1)
            })
            .ToList();

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
        var unassignedSales = await _context.Orders.AsNoTracking()
            .Where(order => !order.MaNV.HasValue
                && (order.TrangThai == OrderStatuses.Completed || order.TrangThai == OrderStatuses.LegacyCompleted)
                && order.NgayTao >= new DateTime(model.StaffReportYears[0], 1, 1)
                && order.NgayTao < yearEnd)
            .GroupBy(order => order.NgayTao.Year)
            .Select(group => new { Year = group.Key, Revenue = group.Sum(order => order.TongTien) })
            .ToDictionaryAsync(item => item.Year, item => item.Revenue);
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
        model.StaffRevenue.Add(new StaffRevenueViewModel
        {
            Name = "Chưa gán nhân viên",
            RevenueByYear = model.StaffReportYears.ToDictionary(
                reportYear => reportYear,
                reportYear => unassignedSales.GetValueOrDefault(reportYear))
        });

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

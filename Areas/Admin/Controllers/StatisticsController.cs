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
        DateTime? toDate = null)
    {
        tab = tab is "satisfaction" or "surveys" ? tab : "customers";
        period = period is "quarter" or "year" or "custom" ? period : "30days";
        var today = DateTime.Today;
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
            ToDate = end
        };

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
}

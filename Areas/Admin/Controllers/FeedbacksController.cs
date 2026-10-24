using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,NhanVien")]
public class FeedbacksController : Controller
{
    private const int PageSize = 20;
    private static readonly string[] ValidStatuses = ["Chờ xử lý", "Đã duyệt", "Từ chối"];
    private readonly CoffeeShopDbContext _context;

    public FeedbacksController(CoffeeShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status, int? rating, string? keyword, int page = 1)
    {
        if (!string.IsNullOrWhiteSpace(status) && !ValidStatuses.Contains(status))
        {
            return BadRequest();
        }

        if (rating is < 1 or > 5)
        {
            return BadRequest();
        }

        page = Math.Max(1, page);
        var allFeedbacks = _context.Feedbacks.AsNoTracking();
        var ratedFeedbacks = allFeedbacks.Where(f => f.DanhGia.HasValue);
        IQueryable<Feedback> filteredFeedbacks = allFeedbacks
            .Include(f => f.Customer)
            .Include(f => f.Product);

        if (!string.IsNullOrWhiteSpace(status))
        {
            filteredFeedbacks = filteredFeedbacks.Where(f => f.TrangThaiXL == status);
        }

        if (rating.HasValue)
        {
            filteredFeedbacks = filteredFeedbacks.Where(f => f.DanhGia == rating.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim();
            filteredFeedbacks = filteredFeedbacks.Where(f =>
                f.NoiDung.Contains(search) ||
                (f.Customer != null && f.Customer.HoTen.Contains(search)) ||
                (f.Product != null && f.Product.TenSP.Contains(search)));
            keyword = search;
        }

        var totalFiltered = await filteredFeedbacks.CountAsync();
        var totalPages = (int)Math.Ceiling(totalFiltered / (double)PageSize);
        if (totalPages > 0)
        {
            page = Math.Min(page, totalPages);
        }

        var ratingCounts = new int[5];
        var ratingGroups = await ratedFeedbacks
            .GroupBy(f => f.DanhGia)
            .Select(group => new { Rating = group.Key, Count = group.Count() })
            .ToListAsync();
        foreach (var group in ratingGroups)
        {
            if (group.Rating is >= 1 and <= 5)
            {
                ratingCounts[group.Rating.Value - 1] = group.Count;
            }
        }

        var viewModel = new AdminFeedbackViewModel
        {
            TotalFeedbacks = await allFeedbacks.CountAsync(),
            FilteredFeedbacks = totalFiltered,
            PendingFeedbacks = await allFeedbacks.CountAsync(f => f.TrangThaiXL == "Chờ xử lý"),
            ApprovedFeedbacks = await allFeedbacks.CountAsync(f => f.TrangThaiXL == "Đã duyệt"),
            AverageRating = await ratedFeedbacks.Where(f => f.TrangThaiXL == "Đã duyệt")
                .Select(f => (double?)f.DanhGia)
                .AverageAsync() ?? 0,
            RatingCounts = ratingCounts,
            Feedbacks = await filteredFeedbacks
                .OrderByDescending(f => f.NgayGui)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(),
            Status = status,
            Rating = rating,
            Keyword = keyword,
            Page = page,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!ValidStatuses.Contains(status))
        {
            return BadRequest();
        }

        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null)
        {
            return NotFound();
        }

        feedback.TrangThaiXL = status;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái đánh giá.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null)
        {
            return NotFound();
        }

        _context.Feedbacks.Remove(feedback);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã xóa đánh giá.";

        return RedirectToAction(nameof(Index));
    }
}

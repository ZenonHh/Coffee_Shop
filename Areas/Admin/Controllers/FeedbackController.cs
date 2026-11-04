using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class FeedbackController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public FeedbackController(CoffeeShopDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string tab = "reviews",
        string status = "all",
        int? stars = null,
        int? productId = null,
        string? search = null,
        int page = 1)
    {
        const int pageSize = 10;
        tab = tab.Equals("contacts", StringComparison.OrdinalIgnoreCase) ? "contacts" : "reviews";
        status = status is "pending" or "answered" ? status : "all";
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        IQueryable<Feedback> feedbackQuery = _context.Feedbacks.AsNoTracking().Include(item => item.Customer).Include(item => item.Product);
        IQueryable<ContactMessage> contactQuery = _context.ContactMessages.AsNoTracking();
        if (tab == "reviews")
        {
            if (status == "pending") feedbackQuery = feedbackQuery.Where(item => item.PhanHoiQuanTri == null || item.PhanHoiQuanTri == "");
            if (status == "answered") feedbackQuery = feedbackQuery.Where(item => item.PhanHoiQuanTri != null && item.PhanHoiQuanTri != "");
            if (stars.HasValue) feedbackQuery = feedbackQuery.Where(item => item.DanhGia == stars.Value);
            if (productId.HasValue) feedbackQuery = feedbackQuery.Where(item => item.MaSP == productId.Value);
            if (search != null)
            {
                feedbackQuery = feedbackQuery.Where(item =>
                    (item.Customer != null && item.Customer.HoTen.Contains(search))
                    || item.NoiDung.Contains(search)
                    || (item.Product != null && item.Product.TenSP.Contains(search)));
            }
        }
        else
        {
            if (status == "pending") contactQuery = contactQuery.Where(item => item.TrangThai == "Chưa xử lý");
            if (status == "answered") contactQuery = contactQuery.Where(item => item.TrangThai == "Đã xử lý");
            if (search != null)
            {
                contactQuery = contactQuery.Where(item =>
                    item.HoTen.Contains(search) || item.Email.Contains(search)
                    || (item.ChuDe != null && item.ChuDe.Contains(search))
                    || item.NoiDung.Contains(search));
            }
        }

        var totalCount = tab == "reviews" ? await feedbackQuery.CountAsync() : await contactQuery.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var model = new FeedbackAdminIndexViewModel
        {
            Tab = tab,
            Status = status,
            Stars = stars,
            ProductId = productId,
            Search = search,
            PendingFeedbackCount = await _context.Feedbacks.CountAsync(item => item.PhanHoiQuanTri == null || item.PhanHoiQuanTri == ""),
            PendingContactCount = await _context.ContactMessages.CountAsync(item => item.TrangThai == "Chưa xử lý"),
            TotalFeedbackCount = await _context.Feedbacks.CountAsync(),
            AverageFeedbackRating = await _context.Feedbacks
                .Where(item => item.DanhGia.HasValue)
                .Select(item => (double?)item.DanhGia)
                .AverageAsync(),
            Products = await _context.Products.AsNoTracking().OrderBy(item => item.TenSP).ToListAsync(),
            Pagination = new PaginationViewModel
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                PageParameter = "page"
            }
        };
        if (tab == "reviews")
        {
            model.Feedbacks = await feedbackQuery
                .OrderBy(item => item.PhanHoiQuanTri != null && item.PhanHoiQuanTri != "")
                .ThenByDescending(item => item.NgayGui)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        else
        {
            model.Contacts = await contactQuery.OrderByDescending(item => item.NgayGui)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        ViewData["Title"] = "Phản hồi";
        return View(model);
    }

    [HttpPost("ReplyToFeedback")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyToFeedback(int id, string response)
    {
        var trimmedResponse = response?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedResponse) || trimmedResponse.Length > 2000)
        {
            TempData["Error"] = "Phản hồi cần có nội dung và không được vượt quá 2.000 ký tự.";
            return RedirectToAction(nameof(Index), new { tab = "reviews" });
        }
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null) return NotFound();
        feedback.PhanHoiQuanTri = trimmedResponse;
        feedback.NgayPhanHoiQuanTri = DateTime.Now;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã lưu phản hồi cho đánh giá của khách hàng.";
        return RedirectToAction(nameof(Index), new { tab = "reviews" });
    }

    [HttpPost("ToggleContactStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleContactStatus(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return NotFound();
        message.TrangThai = message.TrangThai == "Đã xử lý" ? "Chưa xử lý" : "Đã xử lý";
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã đổi trạng thái lời nhắn của {message.HoTen} sang {message.TrangThai}.";
        return RedirectToAction(nameof(Index), new { tab = "contacts" });
    }

    [HttpPost("DeleteContact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return NotFound();
        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã xóa lời nhắn liên hệ.";
        return RedirectToAction(nameof(Index), new { tab = "contacts" });
    }
}

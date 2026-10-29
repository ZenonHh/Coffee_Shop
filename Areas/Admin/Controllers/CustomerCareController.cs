using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class CustomerCareController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public CustomerCareController(CoffeeShopDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string tab = "surveys",
        int? editId = null,
        int? viewResponses = null,
        int surveyPage = 1,
        int contactPage = 1,
        int reviewPage = 1)
    {
        const int pageSize = 10;
        var activeTab = tab.ToLowerInvariant() switch
        {
            "contacts" => "contacts",
            "reviews" => "reviews",
            _ => "surveys"
        };
        var surveyCount = await _context.Surveys.CountAsync();
        var activeSurveyCount = await _context.Surveys.CountAsync(survey =>
            survey.NgayBatDau <= DateTime.Now && survey.NgayKetThuc >= DateTime.Now);
        var contactCount = await _context.ContactMessages.CountAsync();
        var reviewCount = await _context.Feedbacks.CountAsync();
        surveyPage = Math.Clamp(surveyPage, 1, Math.Max(1, (int)Math.Ceiling(surveyCount / (double)pageSize)));
        contactPage = Math.Clamp(contactPage, 1, Math.Max(1, (int)Math.Ceiling(contactCount / (double)pageSize)));
        reviewPage = Math.Clamp(reviewPage, 1, Math.Max(1, (int)Math.Ceiling(reviewCount / (double)pageSize)));

        var surveys = activeTab == "surveys" ? await GetSurveyListAsync(surveyPage, pageSize) : new List<SurveyListItemViewModel>();
        var contacts = activeTab == "contacts"
            ? await _context.ContactMessages.AsNoTracking().OrderByDescending(c => c.NgayGui)
                .Skip((contactPage - 1) * pageSize).Take(pageSize).ToListAsync()
            : new List<ContactMessage>();
        var feedbacks = activeTab == "reviews"
            ? await _context.Feedbacks.AsNoTracking().Include(f => f.Customer).Include(f => f.Product)
                .OrderByDescending(f => f.NgayGui).Skip((reviewPage - 1) * pageSize).Take(pageSize).ToListAsync()
            : new List<Feedback>();

        var unprocessedCount = await _context.ContactMessages.CountAsync(c => c.TrangThai == "Chưa xử lý");
        var unansweredReviewsCount = await _context.Feedbacks.CountAsync(f => f.PhanHoiQuanTri == null || f.PhanHoiQuanTri == "");
        var processedContactCount = await _context.ContactMessages.CountAsync(c => c.TrangThai == "Đã xử lý");
        var answeredReviewsCount = reviewCount - unansweredReviewsCount;

        ViewBag.EditSurvey = editId.HasValue
            ? await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.MaKhaoSat == editId.Value)
            : null;
        ViewBag.ContactMessages = contacts;
        ViewBag.Feedbacks = feedbacks;
        ViewBag.ActiveTab = activeTab;
        ViewBag.SurveyCount = surveyCount;
        ViewBag.ActiveSurveyCount = activeSurveyCount;
        ViewBag.TotalContacts = contactCount;
        ViewBag.UnprocessedCount = unprocessedCount;
        ViewBag.ProcessedContactCount = processedContactCount;
        ViewBag.ReviewCount = reviewCount;
        ViewBag.AnsweredReviewsCount = answeredReviewsCount;
        ViewBag.UnansweredReviewsCount = unansweredReviewsCount;
        ViewBag.Pagination = activeTab switch
        {
            "contacts" => new PaginationViewModel { PageNumber = contactPage, PageSize = pageSize, TotalCount = contactCount, PageParameter = "contactPage" },
            "reviews" => new PaginationViewModel { PageNumber = reviewPage, PageSize = pageSize, TotalCount = reviewCount, PageParameter = "reviewPage" },
            _ => new PaginationViewModel { PageNumber = surveyPage, PageSize = pageSize, TotalCount = surveyCount, PageParameter = "surveyPage" }
        };

        return View(surveys);
    }

    #region Survey Operations (Tab 1)

    [HttpPost("CreateSurvey")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSurvey(Survey survey, List<string>? questionContent, List<string>? questionType)
    {
        Normalize(survey);
        if (!ModelState.IsValid || survey.NgayKetThuc <= survey.NgayBatDau)
        {
            return await InvalidForm(survey);
        }

        survey.MaTaiKhoan = await CurrentAccountId();
        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync();

        if (questionContent != null && questionContent.Any())
        {
            for (int i = 0; i < questionContent.Count; i++)
            {
                var text = questionContent[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var type = (questionType != null && i < questionType.Count && !string.IsNullOrWhiteSpace(questionType[i]))
                        ? questionType[i].Trim()
                        : "TRAC_NGHIEM";

                    _context.SurveyQuestions.Add(new SurveyQuestion
                    {
                        MaKhaoSat = survey.MaKhaoSat,
                        NoiDungCauHoi = text,
                        LoaiCauHoi = type
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Đã tạo đợt khảo sát mới chi tiết thành công.";
        return RedirectToAction(nameof(Index), new { tab = "surveys" });
    }

    [HttpPost("EditSurvey")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSurvey(Survey survey)
    {
        var existing = await _context.Surveys.FindAsync(survey.MaKhaoSat);
        if (existing == null) return NotFound();

        Normalize(survey);
        if (!ModelState.IsValid || survey.NgayKetThuc <= survey.NgayBatDau)
        {
            return await InvalidForm(survey);
        }

        existing.TieuDe = survey.TieuDe;
        existing.MoTa = survey.MoTa;
        existing.NgayBatDau = survey.NgayBatDau;
        existing.NgayKetThuc = survey.NgayKetThuc;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật đợt khảo sát.";
        return RedirectToAction(nameof(Index), new { tab = "surveys" });
    }

    [HttpPost("DeleteSurvey")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSurvey(int id)
    {
        var survey = await _context.Surveys.FindAsync(id);
        if (survey != null)
        {
            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa khảo sát.";
        }
        return RedirectToAction(nameof(Index), new { tab = "surveys" });
    }

    [HttpPost("AddQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int surveyId, string content, string type = "TRAC_NGHIEM")
    {
        if (!string.IsNullOrWhiteSpace(content) && await _context.Surveys.AnyAsync(s => s.MaKhaoSat == surveyId))
        {
            _context.SurveyQuestions.Add(new SurveyQuestion
            {
                MaKhaoSat = surveyId,
                NoiDungCauHoi = content.Trim(),
                LoaiCauHoi = type
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm câu hỏi vào khảo sát.";
        }
        return RedirectToAction(nameof(Index), new { tab = "surveys", editId = surveyId });
    }

    [HttpPost("DeleteQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int surveyId)
    {
        var question = await _context.SurveyQuestions.FindAsync(id);
        if (question != null)
        {
            _context.SurveyQuestions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa câu hỏi.";
        }
        return RedirectToAction(nameof(Index), new { tab = "surveys", editId = surveyId });
    }

    #endregion

    #region Contact & Feedback Operations (Tab 2)

    [HttpPost("ToggleContactStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleContactStatus(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            msg.TrangThai = (msg.TrangThai == "Đã xử lý") ? "Chưa xử lý" : "Đã xử lý";
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã đổi trạng thái lời nhắn của {msg.HoTen} sang: {msg.TrangThai}.";
        }
        return RedirectToAction(nameof(Index), new { tab = "contacts" });
    }

    [HttpPost("DeleteContact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var msg = await _context.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa lời nhắn khỏi hộp thư.";
        }
        return RedirectToAction(nameof(Index), new { tab = "contacts" });
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

    #endregion

    #region Helpers

    private Task<List<SurveyListItemViewModel>> GetSurveyListAsync(int page = 1, int pageSize = 10) => _context.Surveys
        .AsNoTracking()
        .OrderByDescending(survey => survey.NgayBatDau)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(survey => new SurveyListItemViewModel
        {
            MaKhaoSat = survey.MaKhaoSat,
            TieuDe = survey.TieuDe,
            MoTa = survey.MoTa,
            NgayBatDau = survey.NgayBatDau,
            NgayKetThuc = survey.NgayKetThuc,
            AnswerCount = survey.Questions.SelectMany(question => question.Answers).Count(),
            Questions = survey.Questions.Select(question => new SurveyQuestionListItemViewModel
            {
                MaCauHoi = question.MaCauHoi,
                NoiDungCauHoi = question.NoiDungCauHoi,
                LoaiCauHoi = question.LoaiCauHoi,
                Answers = new SurveyAnswerCountViewModel { Count = question.Answers.Count },
                RecentAnswers = question.Answers
                    .OrderByDescending(a => a.NgayTraLoi)
                    .Take(15)
                    .Select(a => new SurveyAnswerDetailViewModel
                    {
                        MaCauTraLoi = a.MaCauTraLoi,
                        NoiDungTraLoi = a.NoiDungTraLoi,
                        NgayTraLoi = a.NgayTraLoi
                    }).ToList()
            }).ToList()
        })
        .ToListAsync();

    private Task<int> CurrentAccountId()
    {
        return Task.FromResult(int.TryParse(User.FindFirstValue("AccountId"), out var accountId) ? accountId : 0);
    }

    private async Task<IActionResult> InvalidForm(Survey survey)
    {
        ViewBag.EditSurvey = survey.MaKhaoSat > 0 ? survey : null;
        ViewBag.FormError = "Vui lòng kiểm tra tiêu đề và thời gian khảo sát (ngày kết thúc phải sau ngày bắt đầu).";
        ViewBag.ActiveTab = "surveys";
        const int pageSize = 10;
        var surveyCount = await _context.Surveys.CountAsync();
        var contactCount = await _context.ContactMessages.CountAsync();
        var reviewCount = await _context.Feedbacks.CountAsync();
        var unansweredCount = await _context.Feedbacks.CountAsync(feedback =>
            feedback.PhanHoiQuanTri == null || feedback.PhanHoiQuanTri == "");
        ViewBag.SurveyCount = surveyCount;
        ViewBag.ActiveSurveyCount = await _context.Surveys.CountAsync(item =>
            item.NgayBatDau <= DateTime.Now && item.NgayKetThuc >= DateTime.Now);
        ViewBag.TotalContacts = contactCount;
        ViewBag.UnprocessedCount = await _context.ContactMessages.CountAsync(message => message.TrangThai == "Chưa xử lý");
        ViewBag.ProcessedContactCount = await _context.ContactMessages.CountAsync(message => message.TrangThai == "Đã xử lý");
        ViewBag.ReviewCount = reviewCount;
        ViewBag.UnansweredReviewsCount = unansweredCount;
        ViewBag.AnsweredReviewsCount = reviewCount - unansweredCount;
        ViewBag.Pagination = new PaginationViewModel
        {
            PageNumber = 1,
            PageSize = pageSize,
            TotalCount = surveyCount,
            PageParameter = "surveyPage"
        };
        ViewBag.ContactMessages = new List<ContactMessage>();
        ViewBag.Feedbacks = new List<Feedback>();
        return View("Index", await GetSurveyListAsync(1, pageSize));
    }

    private static void Normalize(Survey survey)
    {
        survey.TieuDe = survey.TieuDe?.Trim() ?? string.Empty;
        survey.MoTa = string.IsNullOrWhiteSpace(survey.MoTa) ? null : survey.MoTa.Trim();
    }

    #endregion
}

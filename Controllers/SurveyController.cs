using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers;

public class SurveyController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public SurveyController(CoffeeShopDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Index()
    {
        if (!TryGetCustomerId(out var customerId)) return Forbid();
        if (!await _context.Customers.AsNoTracking().AnyAsync(customer => customer.MaKH == customerId && customer.TrangThai))
        {
            return Forbid();
        }
        var now = DateTime.Now;
        var surveys = await _context.Surveys.AsNoTracking()
            .Include(survey => survey.Questions.OrderBy(question => question.ThuTu))
                .ThenInclude(question => question.Options.OrderBy(option => option.ThuTu))
            .Where(survey => survey.NgayBatDau <= now && survey.NgayKetThuc >= now
                && survey.Questions.Any()
                && (survey.DoiTuong == "TatCa" || survey.AudienceCustomers.Any(target => target.MaKH == customerId))
                && !survey.Responses.Any(response => response.MaKH == customerId))
            .OrderByDescending(survey => survey.NgayBatDau)
            .ToListAsync();
        return View(surveys);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> GetActiveSurvey()
    {
        if (!TryGetCustomerId(out var customerId)) return NoContent();
        if (!await _context.Customers.AsNoTracking().AnyAsync(customer => customer.MaKH == customerId && customer.TrangThai))
        {
            return NoContent();
        }
        var now = DateTime.Now;
        var activeSurvey = await _context.Surveys.AsNoTracking()
            .Where(survey => survey.NgayBatDau <= now && survey.NgayKetThuc >= now
                && survey.Questions.Any()
                && (survey.DoiTuong == "TatCa" || survey.AudienceCustomers.Any(target => target.MaKH == customerId))
                && !survey.Responses.Any(response => response.MaKH == customerId))
            .OrderByDescending(survey => survey.NgayBatDau)
            .Select(survey => new { survey.TieuDe, survey.MoTa })
            .FirstOrDefaultAsync();
        return activeSurvey == null ? NoContent() : Json(activeSurvey);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitSurvey(int surveyId, Dictionary<int, string[]>? answers)
    {
        if (!TryGetCustomerId(out var customerId)) return Forbid();
        var now = DateTime.Now;
        var survey = await _context.Surveys
            .Include(item => item.Questions).ThenInclude(question => question.Options)
            .FirstOrDefaultAsync(item => item.MaKhaoSat == surveyId
                && item.NgayBatDau <= now && item.NgayKetThuc >= now
                && item.Questions.Any()
                && (item.DoiTuong == "TatCa" || item.AudienceCustomers.Any(target => target.MaKH == customerId)));
        if (survey == null)
        {
            TempData["Error"] = "Khảo sát không còn mở hoặc tài khoản của bạn không thuộc đối tượng nhận.";
            return RedirectToAction(nameof(Index));
        }
        if (await _context.SurveyResponses.AnyAsync(response => response.MaKhaoSat == surveyId && response.MaKH == customerId))
        {
            TempData["Error"] = "Bạn đã hoàn thành khảo sát này trước đó.";
            return RedirectToAction(nameof(Index));
        }

        var questions = survey.Questions.OrderBy(question => question.ThuTu).ToList();
        var answerRows = new List<(int QuestionId, string Value)>();
        foreach (var question in questions)
        {
            if (answers == null || !answers.TryGetValue(question.MaCauHoi, out var submittedValues))
            {
                TempData["Error"] = "Vui lòng trả lời tất cả câu hỏi.";
                return RedirectToAction(nameof(Index));
            }

            var values = submittedValues.Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim()).ToList();
            if (!IsValidAnswer(question, values))
            {
                TempData["Error"] = "Một hoặc nhiều câu trả lời không hợp lệ. Vui lòng kiểm tra và thử lại.";
                return RedirectToAction(nameof(Index));
            }
            answerRows.AddRange(values.Select(value => (question.MaCauHoi, value)));
        }
        if (answers != null && answers.Keys.Any(questionId => questions.All(question => question.MaCauHoi != questionId)))
        {
            TempData["Error"] = "Biểu mẫu khảo sát không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(item => item.MaKH == customerId && item.TrangThai);
        if (customer == null) return Forbid();

        var response = new SurveyResponse
        {
            MaKhaoSat = surveyId,
            MaKH = customerId,
            NgayHoanThanh = now,
            Answers = answerRows.Select(answer => new SurveyAnswer
            {
                MaCauHoi = answer.QuestionId,
                NoiDungTraLoi = answer.Value,
                NgayTraLoi = now
            }).ToList()
        };
        _context.SurveyResponses.Add(response);
        const int surveyBonusPoints = 50;
        customer.DiemTichLuy += surveyBonusPoints;
        if (customer.DiemTichLuy >= 500) customer.HangTV = "Kim Cương";
        else if (customer.DiemTichLuy >= 200) customer.HangTV = "Vàng";
        else if (customer.DiemTichLuy >= 50) customer.HangTV = "Bạc";
        _context.PointHistories.Add(new PointHistory
        {
            MaKH = customer.MaKH,
            DiemThayDoi = surveyBonusPoints,
            LyDo = "Thưởng +50 điểm khi hoàn thành khảo sát ý kiến",
            NgayThucHien = now
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            TempData["Error"] = "Bạn đã hoàn thành khảo sát này trước đó.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Cảm ơn bạn đã hoàn thành khảo sát! Bạn được cộng 50 điểm tích lũy.";
        return RedirectToAction(nameof(Index));
    }

    private bool TryGetCustomerId(out int customerId) =>
        int.TryParse(User.FindFirstValue("CustomerId"), out customerId) && customerId > 0;

    private static bool IsValidAnswer(SurveyQuestion question, IReadOnlyCollection<string> values)
    {
        if (values.Count == 0) return false;
        return question.LoaiCauHoi switch
        {
            "SAO" => values.Count == 1 && int.TryParse(values.First(), out var rating) && rating is >= 1 and <= 5,
            "TU_LUAN" => values.Count == 1 && values.First().Length <= 4000,
            "MOT_LUA_CHON" => values.Count == 1 && question.Options.Any(option => option.NoiDung == values.First()),
            "NHIEU_LUA_CHON" => values.Distinct(StringComparer.Ordinal).Count() == values.Count
                && values.All(value => question.Options.Any(option => option.NoiDung == value)),
            _ => false
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        for (var inner = exception.InnerException; inner != null; inner = inner.InnerException)
        {
            if (inner is SqlException { Number: 2601 or 2627 }) return true;
        }
        return false;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class SurveysController : Controller
{
    private readonly CoffeeShopDbContext _context;
    public SurveysController(CoffeeShopDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? editId)
    {
        var surveys = await GetSurveyListAsync();
        ViewBag.EditSurvey = editId.HasValue
            ? await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(survey => survey.MaKhaoSat == editId.Value)
            : null;
        return View(surveys);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Survey survey)
    {
        Normalize(survey);
        if (!ModelState.IsValid || survey.NgayKetThuc <= survey.NgayBatDau) return await InvalidForm(survey);
        survey.MaTaiKhoan = await CurrentAccountId();
        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã tạo khảo sát.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Survey survey)
    {
        var existing = await _context.Surveys.FindAsync(survey.MaKhaoSat);
        if (existing == null) return NotFound();
        Normalize(survey);
        if (!ModelState.IsValid || survey.NgayKetThuc <= survey.NgayBatDau) return await InvalidForm(survey);
        existing.TieuDe = survey.TieuDe; existing.MoTa = survey.MoTa; existing.NgayBatDau = survey.NgayBatDau; existing.NgayKetThuc = survey.NgayKetThuc;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật khảo sát.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var survey = await _context.Surveys.FindAsync(id);
        if (survey != null) { _context.Surveys.Remove(survey); await _context.SaveChangesAsync(); TempData["Success"] = "Đã xóa khảo sát."; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int surveyId, string content, string type = "TRAC_NGHIEM")
    {
        if (!string.IsNullOrWhiteSpace(content) && await _context.Surveys.AnyAsync(s => s.MaKhaoSat == surveyId))
        {
            _context.SurveyQuestions.Add(new SurveyQuestion { MaKhaoSat = surveyId, NoiDungCauHoi = content.Trim(), LoaiCauHoi = type });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm câu hỏi.";
        }
        return RedirectToAction(nameof(Index), new { editId = surveyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int surveyId)
    {
        var question = await _context.SurveyQuestions.FindAsync(id);
        if (question != null) { _context.SurveyQuestions.Remove(question); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index), new { editId = surveyId });
    }

    private Task<List<SurveyListItemViewModel>> GetSurveyListAsync() => _context.Surveys
        .AsNoTracking()
        .OrderByDescending(survey => survey.NgayBatDau)
        .Select(survey => new SurveyListItemViewModel
        {
            MaKhaoSat = survey.MaKhaoSat,
            TieuDe = survey.TieuDe,
            NgayBatDau = survey.NgayBatDau,
            NgayKetThuc = survey.NgayKetThuc,
            AnswerCount = survey.Questions.SelectMany(question => question.Answers).Count(),
            Questions = survey.Questions.Select(question => new SurveyQuestionListItemViewModel
            {
                MaCauHoi = question.MaCauHoi,
                NoiDungCauHoi = question.NoiDungCauHoi,
                Answers = new SurveyAnswerCountViewModel { Count = question.Answers.Count }
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
        ViewBag.FormError = "Vui lòng kiểm tra tiêu đề và thời gian khảo sát.";
        return View("Index", await GetSurveyListAsync());
    }

    private static void Normalize(Survey survey)
    {
        survey.TieuDe = survey.TieuDe?.Trim() ?? string.Empty;
        survey.MoTa = string.IsNullOrWhiteSpace(survey.MoTa) ? null : survey.MoTa.Trim();
    }
}
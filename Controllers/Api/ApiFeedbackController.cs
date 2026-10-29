using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/mobile-feedback")]
[ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
public sealed class ApiFeedbackController(CoffeeShopDbContext db) : ControllerBase
{
    [HttpGet("surveys")]
    [ProducesResponseType(typeof(IReadOnlyList<MobileSurveyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MobileSurveyResponse>>> Surveys(
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var surveys = await db.Surveys.AsNoTracking()
            .Where(survey => survey.NgayBatDau <= now && survey.NgayKetThuc >= now)
            .OrderBy(survey => survey.NgayKetThuc)
            .Select(survey => new MobileSurveyResponse(
                survey.MaKhaoSat,
                survey.TieuDe,
                survey.MoTa,
                survey.NgayKetThuc,
                survey.Questions
                    .OrderBy(question => question.MaCauHoi)
                    .Select(question => new MobileSurveyQuestionResponse(
                        question.MaCauHoi,
                        question.NoiDungCauHoi,
                        question.LoaiCauHoi))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Ok(surveys);
    }

    [HttpPost("surveys/{surveyId:int}/answers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitSurvey(
        int surveyId,
        SubmitMobileSurveyRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var questionIds = request.Answers.Select(answer => answer.MaCauHoi).Distinct().ToList();
        if (request.Answers.Count == 0 || questionIds.Count != request.Answers.Count)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Câu trả lời khảo sát không hợp lệ" });

        var validQuestionIds = await db.SurveyQuestions.AsNoTracking()
            .Where(question => question.MaKhaoSat == surveyId
                && question.Survey!.NgayBatDau <= now
                && question.Survey.NgayKetThuc >= now)
            .Select(question => question.MaCauHoi)
            .ToListAsync(cancellationToken);
        if (validQuestionIds.Count != questionIds.Count
            || questionIds.Except(validQuestionIds).Any())
            return BadRequest(new ProblemDetails { Status = 400, Title = "Khảo sát không còn khả dụng" });

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        foreach (var answer in request.Answers)
        {
            db.SurveyAnswers.Add(new SurveyAnswer
            {
                MaCauHoi = answer.MaCauHoi,
                NoiDungTraLoi = answer.NoiDungTraLoi.Trim(),
                NgayTraLoi = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("reviews")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitReview(
        SubmitMobileReviewRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        var productExists = await db.Products.AsNoTracking()
            .Where(product => product.MaSP == request.MaSP && product.TrangThai)
            .Select(product => product.MaSP)
            .AnyAsync(cancellationToken);
        if (!productExists)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Sản phẩm không tồn tại" });

        var feedback = new Feedback
        {
            MaKH = customerId,
            MaSP = request.MaSP,
            DanhGia = request.DanhGia,
            NoiDung = request.NoiDung.Trim(),
            NgayGui = DateTime.Now,
            TrangThaiXL = "Chờ xử lý"
        };
        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new { feedback.MaPhanHoi });
    }
}

public sealed record MobileSurveyResponse(
    int MaKhaoSat,
    string TieuDe,
    string? MoTa,
    DateTime NgayKetThuc,
    IReadOnlyList<MobileSurveyQuestionResponse> Questions);

public sealed record MobileSurveyQuestionResponse(
    int MaCauHoi,
    string NoiDungCauHoi,
    string LoaiCauHoi);

public sealed record SubmitMobileSurveyRequest
{
    [Required, MinLength(1), MaxLength(50)]
    public required List<MobileSurveyAnswerRequest> Answers { get; init; }
}

public sealed record MobileSurveyAnswerRequest
{
    [Range(1, int.MaxValue)]
    public int MaCauHoi { get; init; }

    [Required, StringLength(2000)]
    public required string NoiDungTraLoi { get; init; }
}

public sealed record SubmitMobileReviewRequest
{
    [Range(1, int.MaxValue)]
    public int MaSP { get; init; }

    [Range(1, 5)]
    public int DanhGia { get; init; }

    [Required, StringLength(2000, MinimumLength = 3)]
    public required string NoiDung { get; init; }
}

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
public class SurveysController : Controller
{
    private static readonly HashSet<string> QuestionTypes = new(StringComparer.Ordinal)
    {
        "SAO", "TU_LUAN", "MOT_LUA_CHON", "NHIEU_LUA_CHON"
    };

    private readonly CoffeeShopDbContext _context;

    public SurveysController(CoffeeShopDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        int? resultId = null,
        int? editId = null,
        string resultTab = "summary",
        string state = "all")
    {
        const int pageSize = 10;
        state = state is "active" or "draft" or "ended" ? state : "all";
        var now = DateTime.Now;
        var surveysQuery = _context.Surveys.AsNoTracking();
        if (state == "active") surveysQuery = surveysQuery.Where(survey => survey.NgayBatDau <= now && survey.NgayKetThuc >= now);
        if (state == "draft") surveysQuery = surveysQuery.Where(survey => survey.NgayBatDau > now);
        if (state == "ended") surveysQuery = surveysQuery.Where(survey => survey.NgayKetThuc < now);
        var totalCount = await surveysQuery.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)));
        resultTab = resultTab == "answers" ? "answers" : "summary";
        var model = await BuildIndexModel(page, pageSize, resultId, editId, resultTab: resultTab, state: state);
        if (model.EditingSurvey) ViewBag.OpenCreateModal = true;
        ViewBag.Pagination = new PaginationViewModel
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            PageParameter = "page"
        };
        ViewData["Title"] = "Quản lý khảo sát";
        return View(model);
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> Statistics(int? id, DateTime? fromDate, DateTime? toDate)
    {
        var model = new SurveyStatisticsViewModel
        {
            SelectedSurveyId = id,
            FromDate = fromDate,
            ToDate = toDate,
            Surveys = await _context.Surveys.AsNoTracking()
                .OrderBy(survey => survey.TieuDe)
                .Select(survey => new SurveyStatisticsSurveyOptionViewModel
                {
                    Id = survey.MaKhaoSat,
                    Title = survey.TieuDe
                })
                .ToListAsync()
        };
        ViewData["Title"] = "Thống kê khảo sát";

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            ModelState.AddModelError(nameof(fromDate), "Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");
            return View(model);
        }

        if (!id.HasValue)
        {
            return View(model);
        }

        var survey = await _context.Surveys.AsNoTracking()
            .Include(item => item.Questions)
                .ThenInclude(question => question.Options)
            .FirstOrDefaultAsync(item => item.MaKhaoSat == id.Value);
        if (survey == null)
        {
            return NotFound();
        }

        model.SurveyTitle = survey.TieuDe;
        model.QuestionCount = survey.Questions.Count;

        var responsesQuery = _context.SurveyResponses.AsNoTracking()
            .Where(response => response.MaKhaoSat == id.Value);
        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            responsesQuery = responsesQuery.Where(response => response.NgayHoanThanh >= start);
        }
        if (toDate.HasValue)
        {
            var endExclusive = toDate.Value.Date.AddDays(1);
            responsesQuery = responsesQuery.Where(response => response.NgayHoanThanh < endExclusive);
        }
        model.ResponseCount = await responsesQuery.CountAsync();

        var questionIds = survey.Questions.Select(question => question.MaCauHoi).ToList();
        var answersQuery = _context.SurveyAnswers.AsNoTracking()
            .Where(answer => questionIds.Contains(answer.MaCauHoi));
        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            answersQuery = answersQuery.Where(answer => answer.NgayTraLoi >= start);
        }
        if (toDate.HasValue)
        {
            var endExclusive = toDate.Value.Date.AddDays(1);
            answersQuery = answersQuery.Where(answer => answer.NgayTraLoi < endExclusive);
        }
        var answers = await answersQuery
            .Select(answer => new
            {
                answer.MaCauHoi,
                answer.MaLuotTraLoi,
                answer.NoiDungTraLoi,
                answer.NgayTraLoi
            })
            .ToListAsync();

        var ratingValues = new List<int>();
        foreach (var question in survey.Questions.OrderBy(question => question.ThuTu))
        {
            var questionAnswers = answers
                .Where(answer => answer.MaCauHoi == question.MaCauHoi
                    && !string.IsNullOrWhiteSpace(answer.NoiDungTraLoi))
                .ToList();
            var result = new SurveyStatisticsQuestionViewModel
            {
                Text = question.NoiDungCauHoi,
                Type = question.LoaiCauHoi,
                ResponseCount = questionAnswers
                    .Where(answer => answer.MaLuotTraLoi.HasValue)
                    .Select(answer => answer.MaLuotTraLoi!.Value)
                    .Distinct()
                    .Count()
                    + questionAnswers.Count(answer => !answer.MaLuotTraLoi.HasValue)
            };

            if (question.LoaiCauHoi == "SAO")
            {
                var ratings = questionAnswers
                    .Select(answer => int.TryParse(answer.NoiDungTraLoi, out var rating) ? rating : 0)
                    .Where(rating => rating is >= 1 and <= 5)
                    .ToList();
                ratingValues.AddRange(ratings);
                result.AverageRating = ratings.Count == 0 ? null : ratings.Average();
                result.Ratings = Enumerable.Range(1, 5)
                    .Select(rating => new SurveyStatisticsRatingViewModel
                    {
                        Rating = rating,
                        Count = ratings.Count(value => value == rating),
                        Percentage = ratings.Count == 0
                            ? 0
                            : ratings.Count(value => value == rating) * 100d / ratings.Count
                    })
                    .ToList();
            }
            else if (question.LoaiCauHoi == "TU_LUAN")
            {
                result.TextAnswers = questionAnswers
                    .OrderByDescending(answer => answer.NgayTraLoi)
                    .Select(answer => new SurveyStatisticsTextAnswerViewModel
                    {
                        Text = answer.NoiDungTraLoi!,
                        AnsweredAt = answer.NgayTraLoi
                    })
                    .ToList();
            }
            else if (question.LoaiCauHoi is "MOT_LUA_CHON" or "NHIEU_LUA_CHON")
            {
                result.Options = question.Options.OrderBy(option => option.ThuTu)
                    .Select(option =>
                    {
                        var count = questionAnswers.Count(answer => answer.NoiDungTraLoi == option.NoiDung);
                        return new SurveyStatisticsOptionViewModel
                        {
                            Text = option.NoiDung,
                            Count = count,
                            Percentage = result.ResponseCount == 0
                                ? 0
                                : count * 100d / result.ResponseCount
                        };
                    })
                    .ToList();
            }

            model.Questions.Add(result);
        }

        model.AverageRating = ratingValues.Count == 0 ? null : ratingValues.Average();
        return View(model);
    }

    [HttpPost("CreateSurvey")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSurvey(SurveyEditorViewModel editor)
    {
        ValidateEditor(editor, requireQuestions: true);
        var targetCustomers = await ValidateAudience(editor);
        if (!ModelState.IsValid)
        {
            ViewBag.OpenCreateModal = true;
            ViewBag.FormError = "Vui lòng kiểm tra thông tin khảo sát, thời gian, đối tượng và câu hỏi.";
            ViewBag.Pagination = new PaginationViewModel { PageNumber = 1, PageSize = 10, TotalCount = await _context.Surveys.CountAsync(), PageParameter = "page" };
            ViewData["Title"] = "Quản lý khảo sát";
            return View("Index", await BuildIndexModel(1, 10, editor: editor));
        }

        var survey = new Survey
        {
            MaTaiKhoan = int.TryParse(User.FindFirst("AccountId")?.Value, out var accountId) ? accountId : 0,
            TieuDe = editor.TieuDe.Trim(),
            MoTa = string.IsNullOrWhiteSpace(editor.MoTa) ? null : editor.MoTa.Trim(),
            NgayBatDau = editor.NgayBatDau,
            NgayKetThuc = editor.NgayKetThuc,
            DoiTuong = editor.DoiTuong,
            AudienceCustomers = targetCustomers.Select(customer => new SurveyAudienceCustomer { MaKH = customer }).ToList(),
            Questions = editor.Questions.Select((question, index) => new SurveyQuestion
            {
                NoiDungCauHoi = question.NoiDung.Trim(),
                LoaiCauHoi = question.Loai,
                ThuTu = index + 1,
                Options = question.Loai is "MOT_LUA_CHON" or "NHIEU_LUA_CHON"
                    ? question.LuaChon.Where(option => !string.IsNullOrWhiteSpace(option))
                        .Select((option, optionIndex) => new SurveyQuestionOption
                        {
                            NoiDung = option.Trim(),
                            ThuTu = optionIndex + 1
                        }).ToList()
                    : new List<SurveyQuestionOption>()
            }).ToList()
        };

        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã tạo khảo sát.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("EditSurvey")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSurvey(SurveyEditorViewModel editor)
    {
        var survey = await _context.Surveys
            .Include(item => item.Questions).ThenInclude(question => question.Answers)
            .Include(item => item.AudienceCustomers)
            .FirstOrDefaultAsync(item => item.MaKhaoSat == editor.MaKhaoSat);
        if (survey == null) return NotFound();
        var hasResponses = await _context.SurveyResponses.AnyAsync(response => response.MaKhaoSat == survey.MaKhaoSat)
            || survey.Questions.Any(question => question.Answers.Count > 0);
        if (editor.OnlyMetadata)
        {
            ValidateEditor(editor, requireQuestions: false);
        }
        else
        {
            ValidateEditor(editor, requireQuestions: true);
            await ValidateAudience(editor);
            if (hasResponses)
            {
                TempData["Error"] = "Khảo sát đã có câu trả lời; không thể sửa câu hỏi hoặc đối tượng nhận.";
                return RedirectToAction(nameof(Index));
            }
        }
        if (!ModelState.IsValid)
        {
            ViewBag.OpenCreateModal = true;
            ViewBag.FormError = "Vui lòng kiểm tra thông tin khảo sát, thời gian, đối tượng và câu hỏi.";
            ViewBag.Pagination = new PaginationViewModel { PageNumber = 1, PageSize = 10, TotalCount = await _context.Surveys.CountAsync(), PageParameter = "page" };
            ViewData["Title"] = "Quản lý khảo sát";
            return View("Index", await BuildIndexModel(1, 10, editor: editor));
        }

        survey.TieuDe = editor.TieuDe.Trim();
        survey.MoTa = string.IsNullOrWhiteSpace(editor.MoTa) ? null : editor.MoTa.Trim();
        survey.NgayBatDau = editor.NgayBatDau;
        survey.NgayKetThuc = editor.NgayKetThuc;
        if (!editor.OnlyMetadata)
        {
            _context.SurveyAudienceCustomers.RemoveRange(survey.AudienceCustomers);
            survey.AudienceCustomers = editor.DoiTuong == "TatCa"
                ? new List<SurveyAudienceCustomer>()
                : editor.MaKH.Distinct().Select(customerId => new SurveyAudienceCustomer { MaKH = customerId }).ToList();

            _context.SurveyQuestions.RemoveRange(survey.Questions);
            survey.Questions = BuildQuestions(editor);
        }
        await _context.SaveChangesAsync();
        TempData["Success"] = editor.OnlyMetadata
            ? "Đã cập nhật thông tin khảo sát."
            : "Đã cập nhật khảo sát và danh sách câu hỏi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("DeleteSurvey")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSurvey(int id)
    {
        var survey = await _context.Surveys
            .Include(item => item.Questions)
            .FirstOrDefaultAsync(item => item.MaKhaoSat == id);
        if (survey == null) return NotFound();

        var questionIds = survey.Questions.Select(question => question.MaCauHoi).ToList();
        var answers = await _context.SurveyAnswers.Where(answer => questionIds.Contains(answer.MaCauHoi)).ToListAsync();
        var responses = await _context.SurveyResponses.Where(response => response.MaKhaoSat == id).ToListAsync();
        _context.SurveyAnswers.RemoveRange(answers);
        _context.SurveyResponses.RemoveRange(responses);
        _context.Surveys.Remove(survey);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã xóa khảo sát và các dữ liệu trả lời liên quan.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SurveyAdminIndexViewModel> BuildIndexModel(
        int page,
        int pageSize,
        int? resultId = null,
        int? editId = null,
        SurveyEditorViewModel? editor = null,
        string resultTab = "summary",
        string state = "all")
    {
        var now = DateTime.Now;
        var surveysQuery = _context.Surveys.AsNoTracking();
        if (state == "active") surveysQuery = surveysQuery.Where(survey => survey.NgayBatDau <= now && survey.NgayKetThuc >= now);
        if (state == "draft") surveysQuery = surveysQuery.Where(survey => survey.NgayBatDau > now);
        if (state == "ended") surveysQuery = surveysQuery.Where(survey => survey.NgayKetThuc < now);
        var surveys = await surveysQuery.OrderByDescending(survey => survey.NgayBatDau)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(survey => new SurveyListItemViewModel
            {
                MaKhaoSat = survey.MaKhaoSat,
                TieuDe = survey.TieuDe,
                MoTa = survey.MoTa,
                NgayBatDau = survey.NgayBatDau,
                NgayKetThuc = survey.NgayKetThuc,
                DoiTuong = survey.DoiTuong == "TatCa" ? "Tất cả khách hàng" : "Khách hàng được chọn",
                AnswerCount = survey.Responses.Count,
                AudienceCount = survey.DoiTuong == "TatCa"
                    ? _context.Customers.Count()
                    : survey.AudienceCustomers.Count(),
                QuestionCount = survey.Questions.Count,
                HasResponses = survey.Responses.Any() || survey.Questions.Any(question => question.Answers.Any())
            })
            .ToListAsync();

        var editingSurvey = editor?.MaKhaoSat > 0 ? editor : editId.HasValue ? await GetEditor(editId.Value) : null;
        var activeCustomers = await _context.Customers.AsNoTracking()
            .Where(customer => customer.TrangThai)
            .OrderBy(customer => customer.HoTen)
            .Select(customer => new
            {
                customer.MaKH,
                customer.HoTen,
                customer.TaiKhoan,
                customer.NgaySinh
            })
            .ToListAsync();
        var purchasedInterests = await _context.OrderDetails.AsNoTracking()
            .Where(detail => detail.Order != null
                && detail.Order.MaKH.HasValue
                && detail.Order.Customer != null
                && detail.Order.Customer.TrangThai
                && (detail.Order.TrangThai == OrderStatuses.Completed
                    || detail.Order.TrangThai == OrderStatuses.LegacyCompleted)
                && detail.Product != null
                && detail.Product.Category != null)
            .Select(detail => new
            {
                CustomerId = detail.Order!.MaKH!.Value,
                Interest = detail.Product!.Category!.TenLoaiSP
            })
            .Distinct()
            .ToListAsync();
        var customerOptions = activeCustomers.Select(customer => new SurveyCustomerOptionViewModel
        {
            MaKH = customer.MaKH,
            HoTen = customer.HoTen,
            TaiKhoan = customer.TaiKhoan,
            AgeGroup = CustomerCrmOptions.GetAgeGroup(customer.NgaySinh, DateTime.Today),
            Interests = purchasedInterests.Where(item => item.CustomerId == customer.MaKH)
                .Select(item => item.Interest)
                .Distinct()
                .OrderBy(item => item)
                .ToList()
        }).ToList();
        var model = new SurveyAdminIndexViewModel
        {
            Surveys = surveys,
            Customers = customerOptions,
            Editor = editingSurvey ?? new SurveyEditorViewModel
            {
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(7),
                Questions = new List<SurveyQuestionEditorViewModel>
                {
                    new() { Loai = "SAO" },
                    new() { Loai = "TU_LUAN" }
                }
            },
            EditingSurvey = editingSurvey?.MaKhaoSat > 0,
            ResultTab = resultTab,
            State = state
        };

        if (resultId.HasValue)
        {
            model.Results = await BuildResult(resultId.Value);
        }
        return model;
    }

    private async Task<SurveyEditorViewModel?> GetEditor(int surveyId)
    {
        var survey = await _context.Surveys.AsNoTracking()
            .Include(item => item.Questions).ThenInclude(question => question.Options)
            .Include(item => item.AudienceCustomers)
            .FirstOrDefaultAsync(item => item.MaKhaoSat == surveyId);
        if (survey == null) return null;
        return new SurveyEditorViewModel
        {
            MaKhaoSat = survey.MaKhaoSat,
            TieuDe = survey.TieuDe,
            MoTa = survey.MoTa,
            NgayBatDau = survey.NgayBatDau,
            NgayKetThuc = survey.NgayKetThuc,
            DoiTuong = survey.DoiTuong,
            MaKH = survey.AudienceCustomers.Select(target => target.MaKH).ToList(),
            Questions = survey.Questions.OrderBy(question => question.ThuTu)
                .Select(question => new SurveyQuestionEditorViewModel
                {
                    NoiDung = question.NoiDungCauHoi,
                    Loai = question.LoaiCauHoi,
                    LuaChon = question.Options.OrderBy(option => option.ThuTu)
                        .Select(option => option.NoiDung).ToList()
                }).ToList()
        };
    }

    private static List<SurveyQuestion> BuildQuestions(SurveyEditorViewModel editor) =>
        editor.Questions.Select((question, index) => new SurveyQuestion
        {
            NoiDungCauHoi = question.NoiDung.Trim(),
            LoaiCauHoi = question.Loai,
            ThuTu = index + 1,
            Options = question.Loai is "MOT_LUA_CHON" or "NHIEU_LUA_CHON"
                ? question.LuaChon.Where(option => !string.IsNullOrWhiteSpace(option))
                    .Select((option, optionIndex) => new SurveyQuestionOption
                    {
                        NoiDung = option.Trim(),
                        ThuTu = optionIndex + 1
                    }).ToList()
                : new List<SurveyQuestionOption>()
        }).ToList();

    private async Task<SurveyResultViewModel?> BuildResult(int surveyId)
    {
        var survey = await _context.Surveys.AsNoTracking()
            .Include(item => item.Questions).ThenInclude(question => question.Options)
            .FirstOrDefaultAsync(item => item.MaKhaoSat == surveyId);
        if (survey == null) return null;

        var responses = await _context.SurveyResponses.AsNoTracking()
            .Where(response => response.MaKhaoSat == surveyId)
            .Include(response => response.Answers)
            .Include(response => response.Customer)
            .ToListAsync();
        var result = new SurveyResultViewModel
        {
            MaKhaoSat = surveyId,
            TieuDe = survey.TieuDe,
            TotalResponses = responses.Count
        };
        var questionNames = survey.Questions.ToDictionary(question => question.MaCauHoi, question => question.NoiDungCauHoi);
        result.Responses = responses.OrderByDescending(response => response.NgayHoanThanh)
            .Select(response => new SurveyResponseResultViewModel
            {
                CustomerName = response.Customer?.HoTen ?? "Khách hàng",
                CompletedAt = response.NgayHoanThanh,
                Answers = response.Answers
                    .OrderBy(answer => questionNames.ContainsKey(answer.MaCauHoi)
                        ? survey.Questions.First(question => question.MaCauHoi == answer.MaCauHoi).ThuTu
                        : int.MaxValue)
                    .Select(answer => new SurveyAnswerResultViewModel
                    {
                        Question = questionNames.TryGetValue(answer.MaCauHoi, out var questionName)
                            ? questionName
                            : "Câu hỏi",
                        Answer = answer.NoiDungTraLoi ?? "—"
                    })
                    .ToList()
            })
            .ToList();

        foreach (var question in survey.Questions.OrderBy(question => question.ThuTu))
        {
            var questionAnswers = responses.SelectMany(response => response.Answers)
                .Where(answer => answer.MaCauHoi == question.MaCauHoi && !string.IsNullOrWhiteSpace(answer.NoiDungTraLoi))
                .ToList();
            var questionResult = new SurveyQuestionResultViewModel
            {
                NoiDung = question.NoiDungCauHoi,
                Loai = question.LoaiCauHoi
            };
            if (question.LoaiCauHoi == "SAO")
            {
                var ratings = questionAnswers.Select(answer => int.TryParse(answer.NoiDungTraLoi, out var rating) ? rating : 0)
                    .Where(rating => rating is >= 1 and <= 5).ToList();
                questionResult.AverageRating = ratings.Count == 0 ? null : ratings.Average();
            }
            else if (question.LoaiCauHoi == "TU_LUAN")
            {
                questionResult.TextAnswers = questionAnswers.Select(answer => answer.NoiDungTraLoi!).ToList();
            }
            else
            {
                foreach (var option in question.Options.OrderBy(option => option.ThuTu))
                {
                    var count = questionAnswers.Count(answer => answer.NoiDungTraLoi == option.NoiDung);
                    questionResult.Options.Add(new SurveyOptionResultViewModel
                    {
                        NoiDung = option.NoiDung,
                        Count = count,
                        Percentage = responses.Count == 0 ? 0 : count * 100d / responses.Count
                    });
                }
            }
            result.Questions.Add(questionResult);
        }
        return result;
    }

    private async Task<List<int>> ValidateAudience(SurveyEditorViewModel editor)
    {
        if (editor.DoiTuong == "TatCa") return new List<int>();
        if (editor.DoiTuong != "ChonKhachHang" || editor.MaKH.Count == 0)
        {
            ModelState.AddModelError(nameof(editor.MaKH), "Hãy chọn ít nhất một khách hàng nhận khảo sát.");
            return new List<int>();
        }

        var selectedIds = editor.MaKH.Distinct().ToList();
        var activeIds = await _context.Customers.AsNoTracking()
            .Where(customer => customer.TrangThai && selectedIds.Contains(customer.MaKH))
            .Select(customer => customer.MaKH)
            .ToListAsync();
        if (activeIds.Count != selectedIds.Count)
        {
            ModelState.AddModelError(nameof(editor.MaKH), "Danh sách có khách hàng không tồn tại hoặc đã bị khóa.");
        }
        return activeIds;
    }

    private void ValidateEditor(SurveyEditorViewModel editor, bool requireQuestions)
    {
        editor.TieuDe = editor.TieuDe?.Trim() ?? string.Empty;
        editor.MoTa = string.IsNullOrWhiteSpace(editor.MoTa) ? null : editor.MoTa.Trim();
        if (editor.NgayKetThuc <= editor.NgayBatDau)
        {
            ModelState.AddModelError(nameof(editor.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");
        }
        if (requireQuestions && editor.Questions.Count == 0)
        {
            ModelState.AddModelError(nameof(editor.Questions), "Khảo sát cần có ít nhất một câu hỏi.");
        }
        if (editor.Questions.Count > 30)
        {
            ModelState.AddModelError(nameof(editor.Questions), "Mỗi khảo sát tối đa 30 câu hỏi.");
        }

        foreach (var question in editor.Questions)
        {
            question.NoiDung = question.NoiDung?.Trim() ?? string.Empty;
            question.LuaChon = question.LuaChon
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Select(option => option.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (!QuestionTypes.Contains(question.Loai))
            {
                ModelState.AddModelError(nameof(editor.Questions), "Loại câu hỏi không hợp lệ.");
            }
            if (question.Loai is "MOT_LUA_CHON" or "NHIEU_LUA_CHON"
                && (question.LuaChon.Count < 2 || question.LuaChon.Count > 20))
            {
                ModelState.AddModelError(nameof(editor.Questions), "Câu hỏi trắc nghiệm cần từ 2 đến 20 đáp án.");
            }
        }
    }
}

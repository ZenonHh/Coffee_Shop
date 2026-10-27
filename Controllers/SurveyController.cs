using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers
{
    public class SurveyController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public SurveyController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var surveys = await _context.Surveys
                .Include(s => s.Questions)
                .Where(s => s.NgayBatDau <= now && s.NgayKetThuc >= now)
                .ToListAsync();

            return View(surveys);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSurvey()
        {
            var now = DateTime.Now;
            var activeSurvey = await _context.Surveys
                .AsNoTracking()
                .Include(s => s.Questions)
                .Where(s => s.NgayBatDau <= now && s.NgayKetThuc >= now)
                .OrderByDescending(s => s.NgayBatDau)
                .Select(s => new
                {
                    s.MaKhaoSat,
                    s.TieuDe,
                    s.MoTa,
                    Questions = s.Questions.Select(q => new { q.MaCauHoi, q.NoiDungCauHoi, q.LoaiCauHoi }).ToList()
                })
                .FirstOrDefaultAsync();

            return Json(activeSurvey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuickSurvey(int rating, string? feedback, int? questionId)
        {
            var now = DateTime.Now;
            var activeSurvey = await _context.Surveys
                .Include(s => s.Questions)
                .Where(s => s.NgayBatDau <= now && s.NgayKetThuc >= now)
                .OrderByDescending(s => s.NgayBatDau)
                .FirstOrDefaultAsync();

            if (activeSurvey != null && activeSurvey.Questions.Any())
            {
                var targetQuestion = questionId.HasValue 
                    ? activeSurvey.Questions.FirstOrDefault(q => q.MaCauHoi == questionId.Value) 
                    : activeSurvey.Questions.First();

                if (targetQuestion != null)
                {
                    var answerText = $"{rating} sao" + (!string.IsNullOrWhiteSpace(feedback) ? $" - {feedback.Trim()}" : "");
                    _context.SurveyAnswers.Add(new SurveyAnswer
                    {
                        MaCauHoi = targetQuestion.MaCauHoi,
                        NoiDungTraLoi = answerText,
                        NgayTraLoi = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                int customerId = 1;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == User.Identity.Name);
                    if (customer != null) customerId = customer.MaKH;
                }

                _context.Feedbacks.Add(new Feedback
                {
                    MaKH = customerId,
                    DanhGia = Math.Clamp(rating, 1, 5),
                    NoiDung = string.IsNullOrWhiteSpace(feedback) ? $"Khảo sát nhanh: {rating} sao" : feedback.Trim(),
                    NgayGui = DateTime.Now,
                    TrangThaiXL = "Đã duyệt"
                });
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Cảm ơn bạn đã tham gia khảo sát đóng góp ý kiến!" });
            }

            TempData["Success"] = "Cảm ơn bạn đã tham gia khảo sát đóng góp ý kiến!";
            var referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSurvey(int surveyId, Dictionary<int, string> answers)
        {
            if (answers != null && answers.Any())
            {
                foreach (var kvp in answers)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        var answer = new SurveyAnswer
                        {
                            MaCauHoi = kvp.Key,
                            NoiDungTraLoi = kvp.Value.Trim(),
                            NgayTraLoi = DateTime.Now
                        };
                        _context.SurveyAnswers.Add(answer);
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Tích điểm thưởng cho khách hàng khi làm khảo sát
            if (User.Identity?.IsAuthenticated == true)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == User.Identity.Name);
                if (customer != null)
                {
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
                        NgayThucHien = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cảm ơn bạn đã hoàn thành khảo sát! Bạn được cộng +50 điểm tích lũy vào tài khoản.";
                    return RedirectToAction("Index", "Promotions");
                }
            }

            TempData["Success"] = "Cảm ơn bạn đã hoàn thành bài khảo sát đóng góp ý kiến!";
            return RedirectToAction("Index", "Promotions");
        }
    }
}

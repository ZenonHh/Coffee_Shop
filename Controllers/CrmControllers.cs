using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public FeedbackController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.Product)
                .Where(f => f.TrangThaiXL == "Đã duyệt")
                .OrderByDescending(f => f.NgayGui)
                .Take(20)
                .ToListAsync();

            ViewBag.Products = await _context.Products.Where(p => p.TrangThai).ToListAsync();
            return View(feedbacks);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int? productId, int rating, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || rating is < 1 or > 5)
            {
                TempData["Error"] = "Vui lòng nhập nội dung và chọn mức đánh giá từ 1 đến 5 sao.";
                return RedirectToAction(nameof(Index));
            }

            if (productId.HasValue && !await _context.Products.AnyAsync(p => p.MaSP == productId.Value && p.TrangThai))
            {
                TempData["Error"] = "Món được chọn không tồn tại hoặc hiện không được bán.";
                return RedirectToAction(nameof(Index));
            }

            var username = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == username);
            if (customer == null)
            {
                return Forbid();
            }

            var feedback = new Feedback
            {
                MaKH = customer.MaKH,
                MaSP = productId,
                DanhGia = rating,
                NoiDung = content,
                NgayGui = DateTime.Now,
                TrangThaiXL = "Chờ xử lý"
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã gửi phản hồi và đánh giá!";
            return RedirectToAction(nameof(Index));
        }
    }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSurvey(int surveyId, Dictionary<int, string> answers)
        {
            foreach (var kvp in answers)
            {
                var answer = new SurveyAnswer
                {
                    MaCauHoi = kvp.Key,
                    NoiDungTraLoi = kvp.Value,
                    NgayTraLoi = DateTime.Now
                };
                _context.SurveyAnswers.Add(answer);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cảm ơn bạn đã hoàn thành bài khảo sát đóng góp ý kiến!";
            return RedirectToAction("Index", "Home");
        }
    }
}

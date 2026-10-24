using Microsoft.AspNetCore.Mvc;
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
                .OrderByDescending(f => f.NgayGui)
                .Take(20)
                .ToListAsync();

            ViewBag.Products = await _context.Products.Where(p => p.TrangThai).ToListAsync();
            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int? productId, int rating, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Vui lòng nhập nội dung đánh giá/phản hồi.";
                return RedirectToAction(nameof(Index));
            }

            int customerId = 1; // Default
            if (User.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity.Name;
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == username);
                if (customer != null) customerId = customer.MaKH;
            }

            var feedback = new Feedback
            {
                MaKH = customerId,
                MaSP = productId,
                DanhGia = Math.Clamp(rating, 1, 5),
                NoiDung = content,
                NgayGui = DateTime.Now,
                TrangThaiXL = "Đã duyệt"
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

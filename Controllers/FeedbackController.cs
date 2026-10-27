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
            // Kiểm tra bắt buộc đăng nhập để đánh giá
            if (User.Identity?.IsAuthenticated != true)
            {
                TempData["Error"] = "Vui lòng đăng nhập tài khoản khách hàng để đánh giá sản phẩm.";
                if (productId.HasValue)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = $"/Shop/Detail/{productId.Value}#reviews" });
                }
                return RedirectToAction("Login", "Account", new { returnUrl = "/Feedback" });
            }

            var username = User.Identity.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == username);
            if (customer == null)
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (int.TryParse(customerIdClaim, out var cid))
                {
                    customer = await _context.Customers.FindAsync(cid);
                }
            }

            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản khách hàng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Vui lòng nhập nội dung đánh giá/phản hồi.";
                if (productId.HasValue) return Redirect($"/Shop/Detail/{productId.Value}#reviews");
                return RedirectToAction(nameof(Index));
            }

            var feedback = new Feedback
            {
                MaKH = customer.MaKH,
                MaSP = productId,
                DanhGia = Math.Clamp(rating, 1, 5),
                NoiDung = content.Trim(),
                NgayGui = DateTime.Now,
                TrangThaiXL = "Đã duyệt"
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm!";
            if (productId.HasValue)
            {
                return Redirect($"/Shop/Detail/{productId.Value}#reviews");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

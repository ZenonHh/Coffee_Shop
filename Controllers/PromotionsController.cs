using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Controllers
{
    public class PromotionsController : Controller
    {
        private readonly CoffeeShopDbContext _context;

        public PromotionsController(CoffeeShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Promotions")]
        [Route("UuDai")]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            Customer? customer = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.TaiKhoan == User.Identity.Name);
            }

            var allVouchers = await _context.Vouchers
                .AsNoTracking()
                .Where(v => v.TrangThai && (v.SoLuongToiDa <= 0 || v.SoLuongDaDung < v.SoLuongToiDa))
                .OrderByDescending(v => v.GiaTriGiam)
                .ToListAsync();

            var activeVouchers = allVouchers
                .Where(v => (!v.NgayBatDau.HasValue || v.NgayBatDau <= now) && (!v.NgayKetThuc.HasValue || v.NgayKetThuc >= now))
                .ToList();

            var upcomingVouchers = allVouchers
                .Where(v => v.NgayBatDau.HasValue && v.NgayBatDau > now)
                .ToList();

            var orderRules = new List<OrderDiscountRule>
            {
                new()
                {
                    MinOrderAmount = 100000m,
                    DiscountPercent = 5,
                    DirectDiscountAmount = 5000m,
                    Title = "Đơn từ 100.000đ",
                    Description = "Giảm trực tiếp 5.000đ khi thanh toán đơn hàng từ 100K.",
                    BadgeText = "Giảm 5%",
                    Icon = "☕"
                },
                new()
                {
                    MinOrderAmount = 200000m,
                    DiscountPercent = 10,
                    DirectDiscountAmount = 15000m,
                    Title = "Đơn từ 200.000đ",
                    Description = "Giảm ngay 15.000đ hoặc 10% tổng đơn hàng dành cho nhóm bạn.",
                    BadgeText = "Giảm 10%",
                    Icon = "🍰"
                },
                new()
                {
                    MinOrderAmount = 300000m,
                    DiscountPercent = 15,
                    DirectDiscountAmount = 30000m,
                    Title = "Đơn từ 300.000đ",
                    Description = "Giảm ngay 30.000đ và tặng kèm mã miễn phí vận chuyển 5km.",
                    BadgeText = "Giảm 15%",
                    Icon = "🎉"
                },
                new()
                {
                    MinOrderAmount = 500000m,
                    DiscountPercent = 20,
                    DirectDiscountAmount = 50000m,
                    Title = "Đơn từ 500.000đ",
                    Description = "Giảm ngay 50.000đ và nhân đôi điểm tích lũy vào tài khoản.",
                    BadgeText = "Giảm 20% VIP",
                    Icon = "👑"
                }
            };

            var currentTier = customer?.HangTV ?? "Đồng";
            var memberTiers = new List<MemberTierBenefit>
            {
                new()
                {
                    TierName = "Đồng (Standard)",
                    RequiredPoints = "0 - 49 điểm",
                    DiscountRate = "Tích điểm 1%",
                    Perks = new List<string> { "Tích 1 điểm cho mỗi 10.000đ chi tiêu", "Nhận thông báo ưu đãi sớm qua email", "Tặng +50 điểm khi làm khảo sát ý kiến" },
                    BadgeColor = "#cd7f32",
                    Icon = "🥉",
                    IsCurrent = currentTier == "Đồng"
                },
                new()
                {
                    TierName = "Bạc (Silver)",
                    RequiredPoints = "50 - 199 điểm",
                    DiscountRate = "Giảm 5% toàn menu",
                    Perks = new List<string> { "Giảm 5% cho tất cả đơn hàng trực tiếp hoặc online", "Tặng Voucher 30.000đ trong tháng sinh nhật", "Tích điểm nhân 1.2 lần" },
                    BadgeColor = "#c0c0c0",
                    Icon = "🥈",
                    IsCurrent = currentTier == "Bạc"
                },
                new()
                {
                    TierName = "Vàng (Gold)",
                    RequiredPoints = "200 - 499 điểm",
                    DiscountRate = "Giảm 10% toàn menu",
                    Perks = new List<string> { "Giảm 10% không giới hạn giá trị đơn", "Miễn phí vận chuyển bán kính 5km", "Tặng 1 ly nước Signature miễn phí dịp sinh nhật" },
                    BadgeColor = "#ffd700",
                    Icon = "🥇",
                    IsCurrent = currentTier == "Vàng"
                },
                new()
                {
                    TierName = "Kim Cương (Diamond)",
                    RequiredPoints = "Từ 500 điểm",
                    DiscountRate = "Giảm 15% VIP",
                    Perks = new List<string> { "Giảm 15% VIP mọi hóa đơn", "Freeship toàn bộ các đơn hàng", "Quà tặng tri ân đặc biệt hàng quý", "Ưu tiên phục vụ và đặt bàn VIP" },
                    BadgeColor = "#b9f2ff",
                    Icon = "💎",
                    IsCurrent = currentTier == "Kim Cương"
                }
            };

            var rewards = new List<PointRedeemReward>
            {
                new()
                {
                    PointsRequired = 20,
                    Title = "Mã giảm giá 20.000đ",
                    Description = "Áp dụng cho mọi đơn hàng từ 50.000đ trở lên.",
                    DiscountAmount = 20000m,
                    MinOrder = 50000m,
                    Icon = "🎟️"
                },
                new()
                {
                    PointsRequired = 50,
                    Title = "Mã giảm giá 50.000đ",
                    Description = "Áp dụng cho mọi đơn hàng từ 120.000đ trở lên.",
                    DiscountAmount = 50000m,
                    MinOrder = 120000m,
                    Icon = "🎁"
                },
                new()
                {
                    PointsRequired = 100,
                    Title = "Mã giảm giá 100.000đ",
                    Description = "Áp dụng cho đơn từ 200.000đ, siêu tiết kiệm cho tiệc nhóm.",
                    DiscountAmount = 100000m,
                    MinOrder = 200000m,
                    Icon = "⭐"
                },
                new()
                {
                    PointsRequired = 150,
                    Title = "Ly Đồ Uống Signature Miễn Phí",
                    Description = "Giảm 100% (tối đa 65.000đ) cho ly nước yêu thích của bạn.",
                    DiscountAmount = 65000m,
                    MinOrder = 0m,
                    Icon = "☕"
                }
            };

            var viewModel = new PromotionsPageViewModel
            {
                CurrentCustomer = customer,
                ActiveVouchers = activeVouchers,
                UpcomingVouchers = upcomingVouchers,
                OrderDiscountRules = orderRules,
                MemberTiers = memberTiers,
                RedeemRewards = rewards
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemPoints(int pointsRequired, decimal discountAmount, decimal minOrder, string rewardTitle)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                TempData["Error"] = "Vui lòng đăng nhập tài khoản để đổi điểm lấy ưu đãi.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Promotions#redeem" });
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == User.Identity.Name);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản thành viên.";
                return RedirectToAction("Login", "Account");
            }

            if (customer.DiemTichLuy < pointsRequired)
            {
                TempData["Error"] = $"Bạn hiện có {customer.DiemTichLuy} điểm, cần {pointsRequired} điểm để đổi ưu đãi này.";
                return RedirectToAction(nameof(Index));
            }

            // Trừ điểm và tạo mã voucher cá nhân
            customer.DiemTichLuy -= pointsRequired;

            var randomSuffix = RandomNumberGenerator.GetInt32(1000, 9999);
            var voucherCode = $"CB-P{pointsRequired}-{randomSuffix}";

            var newVoucher = new Voucher
            {
                MaCode = voucherCode,
                TenChuongTrinh = $"Đổi điểm thưởng ({rewardTitle})",
                PhanLoaiGiam = "CASH",
                GiaTriGiam = discountAmount,
                DonToiThieu = minOrder,
                GiamToiDa = discountAmount,
                SoLuongToiDa = 1,
                SoLuongDaDung = 0,
                TrangThai = true,
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddDays(30)
            };

            _context.Vouchers.Add(newVoucher);

            _context.PointHistories.Add(new PointHistory
            {
                MaKH = customer.MaKH,
                DiemThayDoi = -pointsRequired,
                LyDo = $"Đổi {pointsRequired} điểm lấy mã voucher {voucherCode}",
                NgayThucHien = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"🎉 Chúc mừng bạn đã đổi thành công mã ưu đãi: {voucherCode} ({rewardTitle})! Mã có giá trị trong 30 ngày.";
            return RedirectToAction(nameof(Index));
        }
    }
}

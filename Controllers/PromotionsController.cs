using System.Data;
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

            var customerId = customer?.MaKH;
            var allVouchers = await _context.Vouchers
                .AsNoTracking()
                .Where(v => v.TrangThai
                    && (v.SoLuongToiDa <= 0 || v.SoLuongDaDung < v.SoLuongToiDa)
                    && (v.MaKH.HasValue
                        ? customerId.HasValue && v.MaKH == customerId.Value
                        : v.TenChuongTrinh == null || !v.TenChuongTrinh.StartsWith("Đổi điểm thưởng")))
                .OrderByDescending(v => v.GiaTriGiam)
                .ToListAsync();

            var activeVouchers = allVouchers
                .Where(v => (!v.NgayBatDau.HasValue || v.NgayBatDau <= now) && (!v.NgayKetThuc.HasValue || v.NgayKetThuc >= now))
                .ToList();

            var upcomingVouchers = allVouchers
                .Where(v => v.NgayBatDau.HasValue && v.NgayBatDau > now)
                .ToList();

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

            var viewModel = new PromotionsPageViewModel
            {
                CurrentCustomer = customer,
                ActiveVouchers = activeVouchers,
                UpcomingVouchers = upcomingVouchers,
                OrderDiscountRules = OrderDiscountRuleCatalog.All.ToList(),
                MemberTiers = memberTiers,
                RedeemRewards = PointRedeemRewardCatalog.All.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemPoints(int rewardId)
        {
            var reward = PointRedeemRewardCatalog.All.FirstOrDefault(item => item.Id == rewardId);
            if (reward is null)
            {
                TempData["Error"] = "Ưu đãi đổi điểm không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                const string prompt = "Vui lòng đăng nhập tài khoản để đổi điểm lấy ưu đãi.";
                TempData["Error"] = prompt;
                return RedirectToAction("Login", "Account", new { returnUrl = "/Promotions#redeem", message = prompt });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == User.Identity.Name);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản thành viên.";
                return RedirectToAction("Login", "Account");
            }

            if (customer.DiemTichLuy < reward.PointsRequired)
            {
                TempData["Error"] = $"Bạn hiện có {customer.DiemTichLuy} điểm, cần {reward.PointsRequired} điểm để đổi ưu đãi này.";
                return RedirectToAction(nameof(Index));
            }

            customer.DiemTichLuy -= reward.PointsRequired;

            var now = DateTime.Now;
            var voucherCode = $"{reward.CodePrefix}-{Guid.NewGuid():N}".ToUpperInvariant();

            var newVoucher = new Voucher
            {
                MaCode = voucherCode,
                MaKH = customer.MaKH,
                TenChuongTrinh = $"Đổi điểm thưởng ({reward.Title})",
                PhanLoaiGiam = reward.RewardType,
                GiaTriGiam = reward.DiscountAmount,
                DonToiThieu = reward.MinOrder,
                GiamToiDa = reward.DiscountAmount,
                SoLuongToiDa = 1,
                SoLuongDaDung = 0,
                TrangThai = true,
                NgayBatDau = now,
                NgayKetThuc = now.AddDays(30)
            };

            _context.Vouchers.Add(newVoucher);

            _context.PointHistories.Add(new PointHistory
            {
                MaKH = customer.MaKH,
                DiemThayDoi = -reward.PointsRequired,
                LyDo = $"Đổi {reward.PointsRequired} điểm lấy mã voucher {voucherCode}",
                NgayThucHien = now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"🎉 Chúc mừng bạn đã đổi thành công mã ưu đãi: {voucherCode} ({reward.Title})! Mã có giá trị trong 30 ngày.";
            return RedirectToAction(nameof(Index));
        }
    }
}

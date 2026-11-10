using CoffeeShop.Models.Entities;

namespace CoffeeShop.Models.ViewModels
{
    public class PromotionsPageViewModel
    {
        public Customer? CurrentCustomer { get; set; }
        public List<Voucher> ActiveVouchers { get; set; } = new();
        public List<Voucher> UpcomingVouchers { get; set; } = new();
        public List<OrderDiscountRule> OrderDiscountRules { get; set; } = new();
        public List<MemberTierBenefit> MemberTiers { get; set; } = new();
        public List<PointRedeemReward> RedeemRewards { get; set; } = new();
    }

    public class OrderDiscountRule
    {
        public decimal MinOrderAmount { get; set; }
        public int DiscountPercent { get; set; }
        public decimal? DirectDiscountAmount { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string Icon { get; set; } = "🏷️";
    }

    public static class OrderDiscountRuleCatalog
    {
        public static IReadOnlyList<OrderDiscountRule> All { get; } =
        [
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
        ];
    }

    public class MemberTierBenefit
    {
        public string TierName { get; set; } = string.Empty;
        public string RequiredPoints { get; set; } = string.Empty;
        public string DiscountRate { get; set; } = string.Empty;
        public List<string> Perks { get; set; } = new();
        public string BadgeColor { get; set; } = "#c49b63";
        public string Icon { get; set; } = "🥉";
        public bool IsCurrent { get; set; }
    }

    public class PointRedeemReward
    {
        public int Id { get; set; }
        public int PointsRequired { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal MinOrder { get; set; }
        public string RewardType { get; set; } = "CASH";
        public string Icon { get; set; } = "🎟️";
        public string CodePrefix { get; set; } = string.Empty;
    }

    public static class PointRedeemRewardCatalog
    {
        public static IReadOnlyList<PointRedeemReward> All { get; } =
        [
            new()
            {
                Id = 1,
                PointsRequired = 20,
                Title = "Mã giảm giá 20.000đ",
                Description = "Áp dụng cho mọi đơn hàng từ 50.000đ trở lên.",
                DiscountAmount = 20000m,
                MinOrder = 50000m,
                CodePrefix = "CB-P20",
                Icon = "🎟️"
            },
            new()
            {
                Id = 2,
                PointsRequired = 50,
                Title = "Mã giảm giá 50.000đ",
                Description = "Áp dụng cho mọi đơn hàng từ 120.000đ trở lên.",
                DiscountAmount = 50000m,
                MinOrder = 120000m,
                CodePrefix = "CB-P50",
                Icon = "🎁"
            },
            new()
            {
                Id = 3,
                PointsRequired = 100,
                Title = "Mã giảm giá 100.000đ",
                Description = "Áp dụng cho đơn từ 200.000đ, siêu tiết kiệm cho tiệc nhóm.",
                DiscountAmount = 100000m,
                MinOrder = 200000m,
                CodePrefix = "CB-P100",
                Icon = "⭐"
            },
            new()
            {
                Id = 4,
                PointsRequired = 150,
                Title = "Ly Đồ Uống Signature Miễn Phí",
                Description = "Giảm 100% (tối đa 65.000đ) cho ly nước yêu thích của bạn.",
                DiscountAmount = 65000m,
                MinOrder = 0m,
                CodePrefix = "CB-P150",
                Icon = "☕"
            }
        ];
    }
}

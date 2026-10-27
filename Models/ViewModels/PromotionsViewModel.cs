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
        public int PointsRequired { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal MinOrder { get; set; }
        public string RewardType { get; set; } = "CASH";
        public string Icon { get; set; } = "🎟️";
    }
}

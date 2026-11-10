using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public sealed record VoucherDisplayItem(VoucherItem Voucher, bool IsUsed)
{
    public double Opacity => IsUsed ? 0.6 : 1;
    public string Title => Voucher.DisplayTitle;
    public string DiscountText => Voucher.DisplayDiscount;
    public string MinimumText => $"Đơn tối thiểu {Voucher.DonToiThieu:N0} đ";
    public string ExpiryText => Voucher.ExpiryText;
}

public sealed record ExchangeRewardDisplayItem(
    ExchangeRewardItem Reward, bool CanRedeem, int MissingPoints)
{
    public string ButtonText => CanRedeem ? "Đổi Ngay" : $"Cần thêm {MissingPoints} điểm";
    public string ButtonBackgroundColor => CanRedeem ? "#C49B63" : "#3A3530";
    public string ButtonTextColor => CanRedeem ? "#12100D" : "#9E968E";
    public string IconText => Reward.Icon switch
    {
        "ticket.png" or "🎟️" => "🎟️",
        "gift.png" or "🎁" => "🎁",
        "star.png" or "⭐" => "⭐",
        "cup.png" or "☕" => "☕",
        _ => "🎁"
    };
}

public partial class VoucherViewModel(ApiService api, CartService cart) : ObservableObject
{
    private static readonly ExchangeRewardItem[] DefaultRewards =
    [
        new(1, 20, "Mã giảm giá 20.000đ", "Áp dụng cho mọi đơn hàng từ 50.000đ trở lên.", "REWARD20K", "ticket.png"),
        new(2, 50, "Mã giảm giá 50.000đ", "Áp dụng cho mọi đơn hàng từ 120.000đ trở lên.", "REWARD50K", "gift.png"),
        new(3, 100, "Mã giảm giá 100.000đ", "Áp dụng cho đơn từ 200.000đ, siêu tiết kiệm cho tiệc nhóm.", "REWARD100K", "star.png"),
        new(4, 150, "Ly Đồ Uống Signature Miễn Phí", "Giảm 100% (tối đa 65.000đ) cho ly nước yêu thích của bạn.", "REWARDFREE", "cup.png")
    ];

    private IReadOnlyList<ExchangeRewardItem> _rewardDefinitions = DefaultRewards;
    private int? _customerId;

    public ObservableCollection<VoucherDisplayItem> AvailableVouchers { get; } = [];
    public ObservableCollection<VoucherDisplayItem> UsedVouchers { get; } = [];
    public ObservableCollection<ExchangeRewardDisplayItem> ExchangeRewards { get; } = [];

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private int userPoints;

    [ObservableProperty]
    private int selectedTabIndex;

    public bool IsAvailableTab => SelectedTabIndex == 0;
    public bool IsUsedTab => SelectedTabIndex == 1;
    public bool IsExchangeTab => SelectedTabIndex == 2;
    public string TabRedeemedColor => IsAvailableTab ? "#C49B63" : "#1F1A16";
    public string TabUsedColor => IsUsedTab ? "#C49B63" : "#1F1A16";
    public string TabExchangeColor => IsExchangeTab ? "#C49B63" : "#1F1A16";
    public string TabRedeemedTextColor => IsAvailableTab ? "#12100D" : "#FFFFFF";
    public string TabUsedTextColor => IsUsedTab ? "#12100D" : "#FFFFFF";
    public string TabExchangeTextColor => IsExchangeTab ? "#12100D" : "#FFFFFF";

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsAvailableTab));
        OnPropertyChanged(nameof(IsUsedTab));
        OnPropertyChanged(nameof(IsExchangeTab));
        OnPropertyChanged(nameof(TabRedeemedColor));
        OnPropertyChanged(nameof(TabUsedColor));
        OnPropertyChanged(nameof(TabExchangeColor));
        OnPropertyChanged(nameof(TabRedeemedTextColor));
        OnPropertyChanged(nameof(TabUsedTextColor));
        OnPropertyChanged(nameof(TabExchangeTextColor));
    }

    partial void OnUserPointsChanged(int value) => RefreshExchangeRewards();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var profile = await api.RestoreProfileAsync();
            VoucherClassifiedResponse classified;
            if (profile is null)
            {
                _customerId = null;
                var activeVouchers = await api.GetVouchersAsync();
                classified = new VoucherClassifiedResponse(0, activeVouchers, [], DefaultRewards);
            }
            else
            {
                _customerId = profile.MaKH;
                classified = await api.GetClassifiedVouchersAsync(profile.MaKH);
            }

            _rewardDefinitions = classified.ExchangeRewards is { Count: > 0 } rewards
                ? rewards
                : DefaultRewards;
            UserPoints = classified.UserPoints;

            var availableVouchers = classified.AvailableVouchers ?? [];
            var usedVouchers = classified.UsedVouchers ?? [];
            var usedCodes = usedVouchers
                .Select(item => item.MaCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var availableItems = availableVouchers
                .Where(item => !usedCodes.Contains(item.MaCode))
                .ToList();

            AvailableVouchers.Clear();
            foreach (var voucher in availableItems)
                AvailableVouchers.Add(new VoucherDisplayItem(voucher, false));

            UsedVouchers.Clear();
            foreach (var voucher in usedVouchers)
                UsedVouchers.Add(new VoucherDisplayItem(voucher, true));

            RefreshExchangeRewards();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải ưu đãi", exception.Message, "Đóng");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowRedeemed() => SelectedTabIndex = 0;

    [RelayCommand]
    private void ShowUsed() => SelectedTabIndex = 1;

    [RelayCommand]
    private void ShowExchange() => SelectedTabIndex = 2;

    [RelayCommand]
    private async Task UseVoucherAsync(VoucherDisplayItem? item)
    {
        if (item is null || item.IsUsed)
            return;

        cart.VoucherCode = item.Voucher.MaCode;
        await Shell.Current.GoToAsync("//order");
    }

    [RelayCommand]
    private async Task RedeemRewardAsync(ExchangeRewardDisplayItem? item)
    {
        if (item is null || !item.CanRedeem || _customerId is null)
            return;

        try
        {
            var result = await api.RedeemRewardAsync(item.Reward.Id);
            UserPoints = result.UserPoints;
            if (!AvailableVouchers.Any(voucher =>
                    string.Equals(voucher.Voucher.MaCode, result.Voucher.MaCode, StringComparison.OrdinalIgnoreCase)))
                AvailableVouchers.Add(new VoucherDisplayItem(result.Voucher, false));
            SelectedTabIndex = 0;
            await Shell.Current.DisplayAlertAsync(
                "Đổi voucher thành công", $"Mã voucher của bạn: {result.Voucher.MaCode}", "Đóng");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể đổi voucher", exception.Message, "Đóng");
        }
    }

    private void RefreshExchangeRewards()
    {
        ExchangeRewards.Clear();
        foreach (var reward in _rewardDefinitions)
        {
            var missingPoints = Math.Max(0, reward.Points - UserPoints);
            ExchangeRewards.Add(new ExchangeRewardDisplayItem(reward, _customerId.HasValue && missingPoints == 0, missingPoints));
        }
    }

}

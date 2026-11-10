using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using Microsoft.Maui.Storage;

namespace CoffeeShop.Mobile.ViewModels;

public sealed record VoucherDisplayItem(
    VoucherItem Voucher, bool IsUsed, bool IsExchange, int PointsRequired, bool IsRedeemed)
{
    public double Opacity => IsUsed ? 0.6 : 1;
    public string ActionText => IsRedeemed ? "Đã đổi" : $"Đổi ({PointsRequired} điểm)";
    public bool CanUse => !IsUsed && !IsExchange;
    public bool CanRedeem => IsExchange && !IsRedeemed;
}

public partial class VoucherViewModel(ApiService api, CartService cart) : ObservableObject
{
    private const string RedeemedVouchersKey = "MyRedeemedVouchers";
    private IReadOnlyList<VoucherItem> _availableVouchers = [];
    private IReadOnlyList<VoucherItem> _usedVouchers = [];
    private IReadOnlyList<VoucherItem> _exchangeVouchers = [];
    private HashSet<string> _redeemedCodes = new(StringComparer.OrdinalIgnoreCase);

    public ObservableCollection<VoucherDisplayItem> VisibleVouchers { get; } = [];

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private int userPoints;

    [ObservableProperty]
    private int selectedTabIndex;

    public string TabRedeemedColor => SelectedTabIndex == 0 ? "#C49B63" : "#1F1A16";
    public string TabUsedColor => SelectedTabIndex == 1 ? "#C49B63" : "#1F1A16";
    public string TabExchangeColor => SelectedTabIndex == 2 ? "#C49B63" : "#1F1A16";
    public string TabRedeemedTextColor => SelectedTabIndex == 0 ? "#12100D" : "#FFFFFF";
    public string TabUsedTextColor => SelectedTabIndex == 1 ? "#12100D" : "#FFFFFF";
    public string TabExchangeTextColor => SelectedTabIndex == 2 ? "#12100D" : "#FFFFFF";

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(TabRedeemedColor));
        OnPropertyChanged(nameof(TabUsedColor));
        OnPropertyChanged(nameof(TabExchangeColor));
        OnPropertyChanged(nameof(TabRedeemedTextColor));
        OnPropertyChanged(nameof(TabUsedTextColor));
        OnPropertyChanged(nameof(TabExchangeTextColor));
        RefreshVisibleVouchers();
    }

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
                var activeVouchers = await api.GetVouchersAsync();
                classified = new VoucherClassifiedResponse(0, activeVouchers, [], activeVouchers);
            }
            else
            {
                classified = await api.GetClassifiedVouchersAsync(profile.MaKH);
            }

            _availableVouchers = classified.AvailableVouchers;
            _usedVouchers = classified.UsedVouchers;
            _exchangeVouchers = classified.ExchangeVouchers;
            _redeemedCodes = ReadRedeemedCodes();

            var spentPoints = _exchangeVouchers
                .Where(voucher => _redeemedCodes.Contains(voucher.MaCode))
                .Sum(GetPointsRequired);
            UserPoints = Math.Max(0, classified.UserPoints - spentPoints);
            RefreshVisibleVouchers();
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
    private async Task RedeemVoucherAsync(VoucherDisplayItem? item)
    {
        if (item is null || item.IsRedeemed)
            return;

        if (UserPoints < item.PointsRequired)
        {
            await Shell.Current.DisplayAlertAsync(
                "Chưa đủ điểm", $"Bạn cần {item.PointsRequired} điểm để đổi voucher này.", "Đóng");
            return;
        }

        UserPoints -= item.PointsRequired;
        _redeemedCodes.Add(item.Voucher.MaCode);
        Preferences.Default.Set(RedeemedVouchersKey, JsonSerializer.Serialize(_redeemedCodes));
        SelectedTabIndex = 0;
        RefreshVisibleVouchers();
    }

    private void RefreshVisibleVouchers()
    {
        IEnumerable<VoucherDisplayItem> items = SelectedTabIndex switch
        {
            1 => _usedVouchers.Select(voucher => new VoucherDisplayItem(voucher, true, false, 0, false)),
            2 => _exchangeVouchers.Select(voucher => new VoucherDisplayItem(
                voucher, false, true, GetPointsRequired(voucher), _redeemedCodes.Contains(voucher.MaCode))),
            _ => _availableVouchers
                .Where(voucher => _redeemedCodes.Contains(voucher.MaCode))
                .Where(voucher => !_usedVouchers.Any(used =>
                    string.Equals(used.MaCode, voucher.MaCode, StringComparison.OrdinalIgnoreCase)))
                .Select(voucher => new VoucherDisplayItem(voucher, false, false, 0, false))
        };

        VisibleVouchers.Clear();
        foreach (var item in items)
            VisibleVouchers.Add(item);
    }

    private static HashSet<string> ReadRedeemedCodes()
    {
        var storedCodes = Preferences.Default.Get(RedeemedVouchersKey, string.Empty);
        if (string.IsNullOrWhiteSpace(storedCodes))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var codes = JsonSerializer.Deserialize<List<string>>(storedCodes) ?? [];
        return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static int GetPointsRequired(VoucherItem voucher) =>
        Math.Max(20, (int)(voucher.GiaTriGiam / 400m));
}

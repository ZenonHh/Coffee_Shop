using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public partial class VoucherViewModel(ApiService api, CartService cart) : ObservableObject
{
    public ObservableCollection<VoucherItem> Vouchers { get; } = [];

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var vouchers = await api.GetVouchersAsync();
            Vouchers.Clear();
            foreach (var voucher in vouchers)
                Vouchers.Add(voucher);
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
    private async Task UseVoucherAsync(VoucherItem? voucher)
    {
        if (voucher is null)
            return;

        cart.VoucherCode = voucher.MaCode;
        await Shell.Current.GoToAsync("//order");
        await Shell.Current.GoToAsync("menu");
    }
}

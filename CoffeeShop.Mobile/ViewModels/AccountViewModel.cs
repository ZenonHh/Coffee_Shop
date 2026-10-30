using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly CartService _cart;
    private readonly IServiceProvider _services;

    public AccountViewModel(ApiService api, CartService cart, IServiceProvider services)
    {
        _api = api;
        _cart = cart;
        _services = services;
    }

    [RelayCommand]
    private async Task NavigateToProfileAsync() => await Shell.Current.GoToAsync("profile");

    [RelayCommand]
    private async Task NavigateToOrderHistoryAsync() => await Shell.Current.GoToAsync("order-history");

    [RelayCommand]
    private async Task NavigateToVouchersAsync() => await Shell.Current.GoToAsync("//voucher");

    [RelayCommand]
    private async Task NavigateToFavoritesAsync() => await Shell.Current.GoToAsync("favorites");

    [RelayCommand]
    private async Task NavigateToFeedbackAsync() => await Shell.Current.GoToAsync("feedback");

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var current = await Shell.Current.DisplayPromptAsync(
            "Đổi mật khẩu", "Mật khẩu hiện tại", "Tiếp tục", "Hủy", keyboard: Keyboard.Default);
        if (string.IsNullOrWhiteSpace(current))
            return;

        var next = await Shell.Current.DisplayPromptAsync(
            "Đổi mật khẩu", "Mật khẩu mới (ít nhất 8 ký tự)", "Đổi mật khẩu", "Hủy", keyboard: Keyboard.Default);
        if (string.IsNullOrWhiteSpace(next))
            return;
        if (next.Length < 8)
        {
            await Shell.Current.DisplayAlertAsync("Mật khẩu không hợp lệ", "Mật khẩu mới cần ít nhất 8 ký tự.", "Đóng");
            return;
        }

        var confirmation = await Shell.Current.DisplayPromptAsync(
            "Đổi mật khẩu", "Xác nhận mật khẩu mới", "Xác nhận", "Hủy", keyboard: Keyboard.Default);
        if (confirmation != next)
        {
            await Shell.Current.DisplayAlertAsync("Mật khẩu không khớp", "Vui lòng nhập lại chính xác mật khẩu mới.", "Đóng");
            return;
        }

        try
        {
            await _api.ChangePasswordAsync(new ChangePasswordRequest(current, next));
            await Shell.Current.DisplayAlertAsync("Thành công", "Mật khẩu đã được cập nhật.", "Đóng");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể đổi mật khẩu", exception.Message, "Đóng");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _api.LogoutAsync();
        _cart.Clear();

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new NavigationPage(_services.GetRequiredService<SplashPage>());
    }
}

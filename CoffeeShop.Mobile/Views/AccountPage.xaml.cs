using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile.Views;

public partial class AccountPage : ContentPage
{
    private readonly ApiService _api;
    private readonly IServiceProvider _services;
    private CustomerProfile? _profile;

    public AccountPage(ApiService api, IServiceProvider services, AccountViewModel viewModel)
    {
        InitializeComponent();
        _api = api;
        _services = services;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await LoadProfileAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải hồ sơ", exception.Message, "Đóng");
        }
    }

    private async void OnFeatureTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TapGestureRecognizer { CommandParameter: string feature })
            return;

        if (feature == "Vouchers")
        {
            await Shell.Current.GoToAsync("//voucher");
            return;
        }
        if (feature == "Order history")
        {
            await Shell.Current.GoToAsync("order-history");
            return;
        }
        if (feature == "Favorites")
        {
            await Shell.Current.GoToAsync("favorites");
            return;
        }
        if (feature == "Feedback")
        {
            await Shell.Current.GoToAsync("feedback");
            return;
        }
        if (_profile is null)
            return;
        if (feature == "Profile")
        {
            await EditProfileAsync();
            return;
        }
        if (feature == "Address")
            await EditAddressAsync();
        else if (feature == "Password")
            await ChangePasswordAsync();
    }

    private async Task EditProfileAsync()
    {
        var currentProfile = _profile;
        if (currentProfile is null)
            return;
        var fullName = await DisplayPromptAsync("Hồ sơ cá nhân", "Họ và tên", initialValue: currentProfile.HoTen);
        if (fullName is null)
            return;
        var phone = await DisplayPromptAsync("Hồ sơ cá nhân", "Số điện thoại", initialValue: currentProfile.Sdt,
            keyboard: Keyboard.Telephone);
        if (phone is null)
            return;
        var email = await DisplayPromptAsync("Hồ sơ cá nhân", "Email", initialValue: currentProfile.Email ?? "",
            keyboard: Keyboard.Email);
        if (email is null)
            return;
        var address = await DisplayPromptAsync("Hồ sơ cá nhân", "Địa chỉ", initialValue: currentProfile.DiaChi ?? "");
        if (address is null)
            return;

        try
        {
            await _api.UpdateProfileAsync(new UpdateProfileRequest(fullName, phone, email, address));
            await LoadProfileAsync();
            await DisplayAlertAsync("Thành công", "Hồ sơ đã được cập nhật.", "Đóng");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể cập nhật hồ sơ", exception.Message, "Đóng");
        }
    }

    private async Task EditAddressAsync()
    {
        var currentProfile = _profile;
        if (currentProfile is null)
            return;
        var address = await DisplayPromptAsync("Sổ địa chỉ", "Địa chỉ nhận hàng mặc định",
            initialValue: currentProfile.DiaChi ?? "");
        if (address is null)
            return;
        try
        {
            await _api.UpdateProfileAsync(new UpdateProfileRequest(
                currentProfile.HoTen, currentProfile.Sdt, currentProfile.Email, address));
            await LoadProfileAsync();
            await DisplayAlertAsync("Thành công", "Địa chỉ nhận hàng đã được cập nhật.", "Đóng");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể cập nhật địa chỉ", exception.Message, "Đóng");
        }
    }

    private async Task ChangePasswordAsync()
    {
        var current = await DisplayPromptAsync("Đổi mật khẩu", "Mật khẩu hiện tại",
            "Tiếp tục", "Hủy", maxLength: 128, keyboard: Keyboard.Password);
        if (string.IsNullOrEmpty(current))
            return;
        var next = await DisplayPromptAsync("Đổi mật khẩu", "Mật khẩu mới (ít nhất 8 ký tự)",
            "Đổi mật khẩu", "Hủy", maxLength: 128, keyboard: Keyboard.Password);
        if (string.IsNullOrEmpty(next))
            return;
        if (next.Length < 8)
        {
            await DisplayAlertAsync("Mật khẩu không hợp lệ", "Mật khẩu mới cần ít nhất 8 ký tự.", "Đóng");
            return;
        }

        var confirmation = await DisplayPromptAsync("Đổi mật khẩu", "Xác nhận mật khẩu mới",
            "Xác nhận", "Hủy", maxLength: 128, keyboard: Keyboard.Password);
        if (confirmation != next)
        {
            await DisplayAlertAsync("Mật khẩu không khớp", "Vui lòng nhập lại chính xác mật khẩu mới.", "Đóng");
            return;
        }

        try
        {
            await _api.ChangePasswordAsync(new ChangePasswordRequest(current, next));
            await DisplayAlertAsync("Thành công", "Mật khẩu đã được cập nhật.", "Đóng");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể đổi mật khẩu", exception.Message, "Đóng");
        }
    }

    private async Task LoadProfileAsync()
    {
        var profile = await _api.GetProfileAsync();
        if (profile is null)
            throw new InvalidOperationException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        _profile = profile;
        CustomerName.Text = profile.HoTen;
        CustomerPhone.Text = profile.Sdt;
        MemberRank.Text = $"{profile.HangTV.ToUpperInvariant()} MEMBER";
        MemberPoints.Text = $"{profile.DiemTichLuy:N0} điểm";
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        await _api.LogoutAsync();
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new NavigationPage(_services.GetRequiredService<SplashPage>());
    }
}

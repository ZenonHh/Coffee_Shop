using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;

    public ProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var profile = await _api.GetProfileAsync();
            var signedIn = profile is not null;
            ProfileCard.IsVisible = signedIn;
            SignInPrompt.IsVisible = !signedIn;
            SignInButton.IsVisible = !signedIn;
            SignOutButton.IsVisible = signedIn;
            if (profile is null)
                return;
            FullNameLabel.Text = profile.HoTen;
            UsernameLabel.Text = $"Tài khoản: {profile.TenDangNhap}";
            PhoneLabel.Text = $"Điện thoại: {profile.Sdt}";
            AddressLabel.Text = $"Địa chỉ: {profile.DiaChi ?? "Chưa cập nhật"}";
            MemberLabel.Text = $"{profile.HangTV} · {profile.DiemTichLuy} điểm";
        }
        catch (InvalidOperationException)
        {
            ProfileCard.IsVisible = false;
            SignInPrompt.IsVisible = true;
            SignInButton.IsVisible = true;
            SignOutButton.IsVisible = false;
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải hồ sơ", exception.Message, "Đóng");
        }
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var username = await DisplayPromptAsync("Đăng nhập", "Tên đăng nhập");
        if (string.IsNullOrWhiteSpace(username))
            return;
        var password = await DisplayPromptAsync("Đăng nhập", "Mật khẩu", "Đăng nhập", "Hủy", "Mật khẩu", maxLength: 128, keyboard: Keyboard.Default);
        if (string.IsNullOrEmpty(password))
            return;
        try
        {
            await _api.LoginAsync(username, password);
            await LoadProfileAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Đăng nhập thất bại", exception.Message, "Đóng");
        }
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        await _api.LogoutAsync();
        await LoadProfileAsync();
    }
}

using System.Text.RegularExpressions;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;
    private CustomerProfile? _profile;

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
            _profile = profile;
            var signedIn = profile is not null;
            ProfileCard.IsVisible = signedIn;
            ProfileEditForm.IsVisible = false;
            SignInPrompt.IsVisible = !signedIn;
            SignInButton.IsVisible = !signedIn;
            SignOutButton.IsVisible = signedIn;
            if (profile is null)
                return;
            FullNameLabel.Text = profile.HoTen;
            UsernameLabel.Text = $"Tài khoản: {profile.TenDangNhap}";
            PhoneLabel.Text = $"Điện thoại: {profile.Sdt}";
            EmailLabel.Text = $"Email: {profile.Email ?? "Chưa cập nhật"}";
            AddressLabel.Text = $"Địa chỉ: {profile.DiaChi ?? "Chưa cập nhật"}";
            MemberLabel.Text = $"{profile.HangTV} · {profile.DiemTichLuy} điểm";
            EditFullName.Text = profile.HoTen;
            EditUsername.Text = profile.TenDangNhap;
            EditPhone.Text = profile.Sdt;
            EditEmail.Text = profile.Email;
            EditAddress.Text = profile.DiaChi;
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

    private void OnEditProfileClicked(object? sender, EventArgs e)
    {
        ProfileCard.IsVisible = false;
        ProfileEditForm.IsVisible = true;
    }

    private void OnCancelProfileEditClicked(object? sender, EventArgs e)
    {
        ProfileEditForm.IsVisible = false;
        ProfileCard.IsVisible = _profile is not null;
    }

    private async void OnSaveProfileClicked(object? sender, EventArgs e)
    {
        var fullName = EditFullName.Text?.Trim() ?? string.Empty;
        var phone = EditPhone.Text?.Trim() ?? string.Empty;
        var email = EditEmail.Text?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            await DisplayAlertAsync("Thông tin chưa hợp lệ", "Vui lòng nhập họ và tên.", "Đóng");
            return;
        }
        if (!Regex.IsMatch(phone, @"^[0-9]{10}$"))
        {
            await DisplayAlertAsync("Thông tin chưa hợp lệ", "Số điện thoại phải gồm đúng 10 chữ số.", "Đóng");
            return;
        }
        if (!string.IsNullOrWhiteSpace(email)
            && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            await DisplayAlertAsync("Thông tin chưa hợp lệ", "Vui lòng nhập email hợp lệ.", "Đóng");
            return;
        }

        try
        {
            await _api.UpdateProfileAsync(new UpdateProfileRequest(
                fullName, phone, email, string.IsNullOrWhiteSpace(EditAddress.Text) ? null : EditAddress.Text.Trim()));
            await LoadProfileAsync();
            await DisplayAlertAsync("Thành công", "Thông tin cá nhân đã được cập nhật.", "Đóng");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể cập nhật hồ sơ", exception.Message, "Đóng");
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

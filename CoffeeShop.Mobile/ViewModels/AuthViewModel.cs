using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public partial class AuthViewModel(ApiService api) : ObservableObject
{
    public event EventHandler? Authenticated;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string username = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string fullName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string phone = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoginForm))]
    [NotifyPropertyChangedFor(nameof(IsRegistrationForm))]
    [NotifyPropertyChangedFor(nameof(ModeSwitchText))]
    [NotifyPropertyChangedFor(nameof(ContinueText))]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private bool isRegistrationMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsLoginForm => !IsRegistrationMode;
    public bool IsRegistrationForm => IsRegistrationMode;
    public string ModeSwitchText => IsRegistrationMode
        ? "Đã có tài khoản? Đăng nhập"
        : "Chưa có tài khoản? Đăng ký";
    public string ContinueText => IsRegistrationMode ? "Đăng ký tài khoản" : "Đăng nhập";
    public bool CanContinue => !IsBusy
        && !string.IsNullOrWhiteSpace(Username)
        && (IsRegistrationMode
            ? !string.IsNullOrWhiteSpace(FullName)
                && Password.Length >= 8
                && Password == ConfirmPassword
                && !string.IsNullOrWhiteSpace(Phone)
            : !string.IsNullOrWhiteSpace(Password));

    [RelayCommand]
    private void ToggleMode()
    {
        ErrorMessage = null;
        IsRegistrationMode = !IsRegistrationMode;
    }

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task ContinueAsync()
    {
        ErrorMessage = null;
        try
        {
            IsBusy = true;
            if (Username.Trim().Length < 3)
            {
                ErrorMessage = "Tên đăng nhập cần có ít nhất 3 ký tự.";
                return;
            }

            var usernameExists = await api.CheckUsernameAsync(Username.Trim());
            if (IsRegistrationMode)
            {
                if (usernameExists)
                {
                    ErrorMessage = "Tên đăng nhập này đã được sử dụng. Hãy chọn Đăng nhập hoặc dùng tên khác.";
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Mật khẩu xác nhận chưa khớp.";
                    return;
                }

                await api.RegisterAsync(Username.Trim(), Password, FullName.Trim(), Phone.Trim());
            }
            else
            {
                if (!usernameExists)
                {
                    ErrorMessage = "Chưa tìm thấy tài khoản này. Chọn “Chưa có tài khoản? Đăng ký” để tạo tài khoản mới.";
                    return;
                }

                await api.LoginAsync(Username.Trim(), Password);
            }

            Authenticated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

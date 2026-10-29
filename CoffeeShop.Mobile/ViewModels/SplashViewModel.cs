using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public partial class SplashViewModel(ApiService api) : ObservableObject
{
    public event EventHandler? StartRequested;
    public event EventHandler? SessionRestored;

    [ObservableProperty]
    private bool isCheckingSession;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    [RelayCommand]
    private void Start() => StartRequested?.Invoke(this, EventArgs.Empty);

    public async Task CheckSessionAsync()
    {
        if (IsCheckingSession)
            return;

        try
        {
            IsCheckingSession = true;
            if (await api.RestoreProfileAsync() is not null)
                SessionRestored?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Không thể kiểm tra phiên đăng nhập: {exception.Message}";
        }
        finally
        {
            IsCheckingSession = false;
        }
    }
}

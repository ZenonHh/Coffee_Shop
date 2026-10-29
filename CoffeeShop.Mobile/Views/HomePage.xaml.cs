using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private IDispatcherTimer? _carouselTimer;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        _carouselTimer ??= Dispatcher.CreateTimer();
        _carouselTimer.Interval = TimeSpan.FromSeconds(4);
        _carouselTimer.Tick -= OnCarouselTick;
        _carouselTimer.Tick += OnCarouselTick;
        _carouselTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _carouselTimer?.Stop();
    }

    private void OnCarouselTick(object? sender, EventArgs e)
    {
        if (_viewModel.Banners.Count > 1)
            BannerCarousel.Position = (BannerCarousel.Position + 1) % _viewModel.Banners.Count;
    }
}

using CommunityToolkit.Maui.Views;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class ProductDetailBottomSheet : Popup
{
    private readonly ProductDetailViewModel _viewModel;

    public ProductDetailBottomSheet(ProductDetail product, CartService cart, FavoriteService favorites)
    {
        InitializeComponent();
        BindingContext = _viewModel = new ProductDetailViewModel(product, cart, favorites);
        var density = DeviceDisplay.MainDisplayInfo.Density;
        SheetContent.WidthRequest = DeviceDisplay.MainDisplayInfo.Width / density - 24;
        SheetContent.HeightRequest = DeviceDisplay.MainDisplayInfo.Height / density * 0.86;
    }

    private void OnSugarOptionClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string value })
        {
            _viewModel.SugarLevel = value;
            SyncOptionChips(SugarOptionsLayout, value);
        }
    }

    private void OnIceOptionClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string value })
        {
            _viewModel.IceLevel = value;
            SyncOptionChips(IceOptionsLayout, value);
        }
    }

    private void OnOptionChipsLoaded(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, SugarOptionsLayout))
            SyncOptionChips(SugarOptionsLayout, _viewModel.SugarLevel);
        else if (ReferenceEquals(sender, IceOptionsLayout))
            SyncOptionChips(IceOptionsLayout, _viewModel.IceLevel);
    }

    private static void SyncOptionChips(HorizontalStackLayout layout, string selectedValue)
    {
        foreach (var chip in layout.Children.OfType<Button>())
        {
            var isSelected = chip.CommandParameter as string == selectedValue;
            chip.BackgroundColor = isSelected
                ? (Color)Application.Current!.Resources["Accent"]
                : (Color)Application.Current!.Resources["CardSurface"];
            chip.TextColor = isSelected
                ? (Color)Application.Current!.Resources["Background"]
                : (Color)Application.Current!.Resources["TextPrimary"];
            chip.BorderColor = isSelected
                ? (Color)Application.Current!.Resources["Accent"]
                : (Color)Application.Current!.Resources["BorderColor"];
        }
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        _viewModel.AddToCartCommand.Execute(null);
        Close();
    }

    private void OnCloseClicked(object? sender, EventArgs e) => Close();
}

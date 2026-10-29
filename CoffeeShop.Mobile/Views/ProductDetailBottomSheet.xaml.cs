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

    private void OnSizeChecked(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton { BindingContext: ProductVariantSelection variant })
            _viewModel.SelectVariantCommand.Execute(variant);
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        _viewModel.AddToCartCommand.Execute(null);
        Close();
    }

    private void OnCloseClicked(object? sender, EventArgs e) => Close();
}

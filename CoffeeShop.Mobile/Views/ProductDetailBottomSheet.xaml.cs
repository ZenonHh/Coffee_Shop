using CommunityToolkit.Maui.Views;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class ProductDetailBottomSheet : Popup
{
    private readonly ProductDetailViewModel _viewModel;

    public ProductDetailBottomSheet(ProductDetail product, CartService cart)
    {
        InitializeComponent();
        BindingContext = _viewModel = new ProductDetailViewModel(product, cart);
        var density = DeviceDisplay.MainDisplayInfo.Density;
        SheetContent.WidthRequest = DeviceDisplay.MainDisplayInfo.Width / density - 24;
        SheetContent.HeightRequest = DeviceDisplay.MainDisplayInfo.Height / density * 0.86;
    }

    private void OnSizeChecked(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton { Value: int variantId })
            _viewModel.SelectVariantCommand.Execute(_viewModel.Product.Variants.First(item => item.MaBienThe == variantId));
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        _viewModel.AddToCartCommand.Execute(null);
        Close();
    }
}

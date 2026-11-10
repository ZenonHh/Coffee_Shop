using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile.ViewModels;

public sealed record MenuCategory(int MaLoaiSP, string TenLoaiSP);

public sealed partial class MenuFilterOption(string label, string value) : ObservableObject
{
    public string Label { get; } = label;
    public string Value { get; } = value;

    [ObservableProperty]
    private bool isSelected;

    public string ButtonBackgroundColor => IsSelected ? "#C49B63" : "#26211C";
    public string ButtonTextColor => IsSelected ? "#12100D" : "#FFFFFF";

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ButtonBackgroundColor));
        OnPropertyChanged(nameof(ButtonTextColor));
    }
}

public partial class MenuViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly CartService _cart;
    private readonly FavoriteService _favorites;
    private readonly IServiceProvider _services;
    private long _productsRequestVersion;
    public ObservableCollection<MenuCategory> Categories { get; } =
    [
        new MenuCategory(0, "Tất cả")
    ];
    public ObservableCollection<ProductItem> Products { get; } = [];
    public ObservableCollection<MenuFilterOption> SortOptions { get; } =
    [
        new("Tên A → Z", "name-asc"),
        new("Tên Z → A", "name-desc"),
        new("Giá thấp → cao", "price-asc"),
        new("Giá cao → thấp", "price-desc"),
        new("Bán chạy nhất", "best-selling"),
        new("Đánh giá cao nhất", "rating-desc")
    ];
    public ObservableCollection<MenuFilterOption> PriceRangeOptions { get; } =
    [
        new("Không giới hạn", ""),
        new("Dưới 30.000đ", "under-30"),
        new("30.000–40.000đ", "30-40"),
        new("40.000–50.000đ", "40-50"),
        new("Trên 50.000đ", "over-50")
    ];
    public MenuFilterOption MinimumRatingOption { get; } = new("Từ 4 sao trở lên", "4");
    public MenuFilterOption ToppingOption { get; } = new("Có thể thêm topping", "topping");
    public int CartCount => _cart.ItemCount;
    public decimal CartTotal => _cart.Subtotal;
    public bool IsCartVisible => _cart.ItemCount > 0;
    public int CartItemCount => _cart.ItemCount;
    public bool HasItemsInCart => _cart.ItemCount > 0;
    public string TotalPriceFormatted => $"{_cart.Subtotal:N0} đ";

    [ObservableProperty]
    private MenuCategory? selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshingProducts;

    [ObservableProperty]
    private bool isAdvancedFilterVisible;

    [ObservableProperty]
    private bool isSortOptionsVisible;

    [ObservableProperty]
    private MenuFilterOption selectedSortOption = null!;

    [ObservableProperty]
    private MenuFilterOption selectedPriceRange = null!;

    [ObservableProperty]
    private bool isSizeMSelected;

    [ObservableProperty]
    private bool isSizeLSelected;

    [ObservableProperty]
    private bool hasTopping;

    [ObservableProperty]
    private bool isMinimumRatingSelected;

    public string MinimumRatingButtonText =>
        IsMinimumRatingSelected ? "Đánh giá: Từ 4 sao trở lên" : "Đánh giá: Mọi mức";

    public MenuViewModel(ApiService api, CartService cart, FavoriteService favorites, IServiceProvider services)
    {
        _api = api;
        _cart = cart;
        _favorites = favorites;
        _services = services;
        SelectedSortOption = SortOptions[0];
        SelectedPriceRange = PriceRangeOptions[0];
        cart.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CartCount));
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(IsCartVisible));
            OnPropertyChanged(nameof(CartItemCount));
            OnPropertyChanged(nameof(HasItemsInCart));
            OnPropertyChanged(nameof(TotalPriceFormatted));
        };
    }

    public int ActiveFilterCount =>
        (string.IsNullOrEmpty(SelectedPriceRange.Value) ? 0 : 1)
        + (IsSizeMSelected || IsSizeLSelected ? 1 : 0)
        + (HasTopping ? 1 : 0)
        + (IsMinimumRatingSelected ? 1 : 0);

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var categories = await _api.GetCategoriesAsync();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Categories.Clear();
                Categories.Add(new MenuCategory(0, "Tất cả"));
                foreach (var category in categories.Where(category => category is not null))
                    Categories.Add(new MenuCategory(category.MaLoaiSP, category.TenLoaiSP ?? string.Empty));

                _isLoadingCategories = true;
                SelectedCategory ??= Categories[0];
                _isLoadingCategories = false;
            });
            await RefreshProductsAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[MENU ERROR]: {exception}");
            await Shell.Current.DisplayAlertAsync(
                "Không thể tải thực đơn",
                "Không thể tải thực đơn. Vui lòng kiểm tra kết nối và thử lại.",
                "Đóng");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshProductsAsync()
    {
        var requestVersion = Interlocked.Increment(ref _productsRequestVersion);
        IsRefreshingProducts = true;

        try
        {
            var products = await _api.GetProductsAsync(
                SelectedCategory?.MaLoaiSP,
                SearchText,
                SelectedSortOption.Value,
                SelectedPriceRange.Value,
                SelectedSizes,
                HasTopping,
                IsMinimumRatingSelected ? 4 : null);
            if (requestVersion != Volatile.Read(ref _productsRequestVersion))
                return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Products.Clear();
                foreach (var product in products.Where(product => product is not null))
                {
                    Products.Add(product with
                    {
                        TenSP = product.TenSP ?? string.Empty,
                        HinhAnh = _api.ResolveImageUrl(product.HinhAnh),
                        TenLoaiSP = product.TenLoaiSP ?? string.Empty
                    });
                }
            });
        }
        catch (Exception exception)
        {
            if (requestVersion != Volatile.Read(ref _productsRequestVersion))
                return;

            System.Diagnostics.Debug.WriteLine($"[MENU ERROR]: {exception}");
            await Shell.Current.DisplayAlertAsync(
                "Không thể tải thực đơn",
                $"Không thể tải thực đơn. {exception.Message}",
                "Đóng");
        }
        finally
        {
            if (requestVersion == Volatile.Read(ref _productsRequestVersion))
                IsRefreshingProducts = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        IsAdvancedFilterVisible = false;
        await RefreshProductsAsync();
    }

    [RelayCommand]
    private void ToggleAdvancedFilters()
    {
        IsAdvancedFilterVisible = !IsAdvancedFilterVisible;
        IsSortOptionsVisible = false;
    }

    [RelayCommand]
    private void ToggleSortOptions()
    {
        IsSortOptionsVisible = !IsSortOptionsVisible;
        IsAdvancedFilterVisible = false;
    }

    [RelayCommand]
    private async Task SelectSortAsync(MenuFilterOption? option)
    {
        if (option is null)
            return;

        SelectedSortOption = option;
        IsSortOptionsVisible = false;
        await RefreshProductsAsync();
    }

    [RelayCommand]
    private void SelectPriceRange(MenuFilterOption? option)
    {
        if (option is null)
            return;

        SelectedPriceRange = option;
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SelectedSortOption = SortOptions[0];
        SelectedPriceRange = PriceRangeOptions[0];
        IsSizeMSelected = false;
        IsSizeLSelected = false;
        HasTopping = false;
        IsMinimumRatingSelected = false;
        await RefreshProductsAsync();
    }

    [RelayCommand]
    private void ToggleMinimumRating() =>
        IsMinimumRatingSelected = !IsMinimumRatingSelected;

    [RelayCommand]
    private void ToggleTopping() =>
        HasTopping = !HasTopping;

    [RelayCommand]
    private async Task OpenProductAsync(ProductItem? product)
    {
        if (product is null)
            return;
        if (product.IsOutOfStock)
        {
            await Shell.Current.DisplayAlertAsync(
                "Đã hết hàng",
                "Rất tiếc! Món này hiện tại đã hết hàng. Quý khách vui lòng chọn món khác nhé!",
                "Đóng");
            return;
        }

        try
        {
            var detail = await _api.GetProductDetailAsync(product.MaSP);
            detail = detail with { HinhAnh = _api.ResolveImageUrl(detail.HinhAnh) };
            await Shell.Current.ShowPopupAsync(new ProductDetailBottomSheet(detail, _cart, _favorites));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể mở món", exception.Message, "Đóng");
        }
    }

    [RelayCommand]
    private async Task OpenCartAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(_services.GetRequiredService<CartPage>());
    }

    partial void OnSelectedCategoryChanged(MenuCategory? value)
    {
        if (value is not null && !_isLoadingCategories)
            _ = RefreshProductsAsync();
    }

    partial void OnSelectedSortOptionChanged(MenuFilterOption value)
    {
        foreach (var option in SortOptions)
            option.IsSelected = option == value;
        OnPropertyChanged(nameof(ActiveFilterCount));
    }

    partial void OnSelectedPriceRangeChanged(MenuFilterOption value)
    {
        foreach (var option in PriceRangeOptions)
            option.IsSelected = option == value;
        OnPropertyChanged(nameof(ActiveFilterCount));
    }

    partial void OnIsSizeMSelectedChanged(bool value) =>
        OnPropertyChanged(nameof(ActiveFilterCount));

    partial void OnIsSizeLSelectedChanged(bool value) =>
        OnPropertyChanged(nameof(ActiveFilterCount));

    partial void OnHasToppingChanged(bool value)
    {
        ToppingOption.IsSelected = value;
        OnPropertyChanged(nameof(ActiveFilterCount));
    }

    partial void OnIsMinimumRatingSelectedChanged(bool value)
    {
        MinimumRatingOption.IsSelected = value;
        OnPropertyChanged(nameof(ActiveFilterCount));
        OnPropertyChanged(nameof(MinimumRatingButtonText));
    }

    private IReadOnlyList<string> SelectedSizes =>
        new[] { IsSizeMSelected ? "M" : null, IsSizeLSelected ? "L" : null }
            .Where(size => size is not null)
            .Select(size => size!)
            .ToArray();

    private bool _isLoadingCategories;
}

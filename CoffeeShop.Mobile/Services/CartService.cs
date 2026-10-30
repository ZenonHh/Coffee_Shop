using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CoffeeShop.Mobile.Models;

namespace CoffeeShop.Mobile.Services;

public partial class CartService : ObservableObject
{
    public ObservableCollection<CartLine> Items { get; } = [];

    [ObservableProperty]
    private string? voucherCode;

    [ObservableProperty]
    private string selectedFulfillment = "Giao hàng";

    public int ItemCount => Items.Sum(item => item.Quantity);
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public decimal ShippingFee => Subtotal > 0 ? 15000m : 0m;
    public decimal Total => Subtotal + ShippingFee;

    partial void OnSelectedFulfillmentChanged(string value)
    {
        OnPropertyChanged(nameof(ShippingFee));
        OnPropertyChanged(nameof(Total));
    }

    public void Add(CartLine line)
    {
        var existing = Items.FirstOrDefault(item =>
            item.Product.MaSP == line.Product.MaSP
            && item.Variant.MaBienThe == line.Variant.MaBienThe
            && item.Note == line.Note
            && item.Toppings.Select(topping => topping.MaSP).OrderBy(id => id)
                .SequenceEqual(line.Toppings.Select(topping => topping.MaSP).OrderBy(id => id)));

        if (existing is null)
            Items.Add(line);
        else
            existing.Quantity += line.Quantity;

        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(ShippingFee));
        OnPropertyChanged(nameof(Total));
    }

    public void ChangeQuantity(CartLine line, int change)
    {
        line.Quantity = Math.Max(0, line.Quantity + change);
        if (line.Quantity == 0)
            Items.Remove(line);
        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(ShippingFee));
        OnPropertyChanged(nameof(Total));
    }

    public void Clear()
    {
        Items.Clear();
        VoucherCode = null;
        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(ShippingFee));
        OnPropertyChanged(nameof(Total));
    }
}

namespace CoffeeShop.Controllers.Api;

/// <summary>Nearest active voucher threshold above the current cart subtotal.</summary>
public sealed record CartUpsellHintDto(
    bool HasHint,
    decimal AmountNeeded,
    decimal? NextThreshold,
    string? DiscountText,
    decimal Progress,
    string? Message);

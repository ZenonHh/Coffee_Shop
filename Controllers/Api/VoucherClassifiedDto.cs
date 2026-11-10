namespace CoffeeShop.Controllers.Api;

/// <summary>Voucher groups and computed loyalty points for one customer.</summary>
public sealed record VoucherClassifiedDto(
    int UserPoints,
    IReadOnlyList<VoucherListResponse> AvailableVouchers,
    IReadOnlyList<VoucherListResponse> UsedVouchers,
    IReadOnlyList<VoucherListResponse> ExchangeVouchers);

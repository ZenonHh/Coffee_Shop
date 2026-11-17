namespace CoffeeShop.Controllers.Api;

/// <summary>Voucher groups and computed loyalty points for one customer.</summary>
public sealed record VoucherClassifiedDto(
    int UserPoints,
    IReadOnlyList<VoucherListResponse> AvailableVouchers,
    IReadOnlyList<VoucherListResponse> UsedVouchers,
    IReadOnlyList<ExchangeRewardResponse> ExchangeRewards);

public sealed record ExchangeRewardResponse(
    int Id,
    int Points,
    string Title,
    string Description,
    string CodePrefix,
    string Icon);

public sealed record RedeemRewardRequest(int RewardId);

public sealed record RedeemRewardResponse(
    int UserPoints,
    VoucherListResponse Voucher);

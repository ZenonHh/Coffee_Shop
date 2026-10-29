using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoffeeShop.Mobile.Models;

namespace CoffeeShop.Mobile.Services;

public sealed class ApiService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };
    private const string TokenKey = "coffee_shop_access_token";

    public string? ResolveImageUrl(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return null;
        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absolute))
            return absolute.ToString();
        return new Uri(httpClient.BaseAddress!, imagePath.TrimStart('/')).ToString();
    }

    public async Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        await GetRequiredAsync<List<CategoryItem>>("api/menu/categories", cancellationToken);

    public async Task<IReadOnlyList<ProductItem>> GetProductsAsync(
        int? categoryId, string? keyword, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (categoryId is > 0)
            query.Add($"categoryId={categoryId}");
        if (!string.IsNullOrWhiteSpace(keyword))
            query.Add($"keyword={Uri.EscapeDataString(keyword.Trim())}");
        return await GetRequiredAsync<List<ProductItem>>(
            "api/menu/products" + (query.Count == 0 ? "" : "?" + string.Join("&", query)), cancellationToken);
    }

    public Task<ProductDetail> GetProductDetailAsync(int id, CancellationToken cancellationToken = default) =>
        GetRequiredAsync<ProductDetail>($"api/menu/product-detail/{id}", cancellationToken);

    public async Task<AuthResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login",
            new { TenDangNhap = username, MatKhau = password }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Phản hồi đăng nhập không hợp lệ.");
        await SecureStorage.SetAsync(TokenKey, result.AccessToken);
        return result;
    }

    public async Task<CustomerProfile?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/auth/profile", cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            SecureStorage.Remove(TokenKey);
            return null;
        }
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CustomerProfile>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderHistoryItem>> GetOrderHistoryAsync(
        string? username, CancellationToken cancellationToken = default)
    {
        var suffix = string.IsNullOrWhiteSpace(username) ? "" : "?username=" + Uri.EscapeDataString(username);
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/order/history" + suffix, cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<OrderHistoryItem>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<VoucherCheckResult> CheckVoucherAsync(
        string code, decimal subtotal, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/voucher/check",
            new VoucherCheckRequest(code, subtotal), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<VoucherCheckResult>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Phản hồi voucher không hợp lệ.");
    }

    public async Task<IReadOnlyList<VoucherItem>> GetVouchersAsync(CancellationToken cancellationToken = default) =>
        await GetRequiredAsync<List<VoucherItem>>("api/voucher", cancellationToken);

    public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest checkout, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/order/checkout", cancellationToken);
        request.Content = JsonContent.Create(checkout, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CheckoutResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Phản hồi đặt hàng không hợp lệ.");
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(TokenKey);
        await Task.CompletedTask;
    }

    private async Task<T> GetRequiredAsync<T>(string uri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Máy chủ trả về dữ liệu rỗng.");
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(
        HttpMethod method, string uri, CancellationToken cancellationToken)
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Vui lòng đăng nhập để tiếp tục.");
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(detail) ? $"Yêu cầu thất bại ({(int)response.StatusCode})." : detail,
            null, response.StatusCode);
    }
}

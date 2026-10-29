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
            return "coffee_swirl.svg";
        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        var path = imagePath.Trim().Replace('\\', '/').TrimStart('/');
        if (path.StartsWith("~/", StringComparison.Ordinal))
            path = path[2..];
        if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            path = path["wwwroot/".Length..];
        if (!path.StartsWith("images/", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith("picture/", StringComparison.OrdinalIgnoreCase))
            path = $"images/{path}";

        return new Uri(httpClient.BaseAddress!, path).ToString();
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

    public async Task<bool> CheckUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/check-username",
            new { TenDangNhap = username }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UsernameCheckResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Phản hồi kiểm tra tên đăng nhập không hợp lệ.");
        return result.Exists;
    }

    public async Task<AuthResponse> RegisterAsync(
        string username, string password, string fullName, string phone, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/register",
            new { TenDangNhap = username, MatKhau = password, HoTen = fullName, Sdt = phone },
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Phản hồi đăng ký không hợp lệ.");
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

    public async Task<CustomerProfile?> RestoreProfileAsync(CancellationToken cancellationToken = default)
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        return string.IsNullOrWhiteSpace(token) ? null : await GetProfileAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PointHistoryItem>> GetPointHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/auth/points", cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<PointHistoryItem>>(JsonOptions, cancellationToken) ?? [];
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
        await GetRequiredAsync<List<VoucherItem>>("api/voucher/list", cancellationToken);

    public async Task UpdateProfileAsync(
        UpdateProfileRequest profile, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Put, "api/mobile-user/profile", cancellationToken);
        request.Content = JsonContent.Create(profile, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task ChangePasswordAsync(
        ChangePasswordRequest password, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Put, "api/mobile-user/password", cancellationToken);
        request.Content = JsonContent.Create(password, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<SurveyItem>> GetSurveysAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Get, "api/mobile-feedback/surveys", cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<SurveyItem>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task SubmitSurveyAsync(
        int surveyId, SubmitSurveyRequest answers, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Post, $"api/mobile-feedback/surveys/{surveyId}/answers", cancellationToken);
        request.Content = JsonContent.Create(answers, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SubmitReviewAsync(
        SubmitReviewRequest review, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(
            HttpMethod.Post, "api/mobile-feedback/reviews", cancellationToken);
        request.Content = JsonContent.Create(review, options: JsonOptions);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

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
        try
        {
            using var document = JsonDocument.Parse(detail);
            if (document.RootElement.TryGetProperty("detail", out var detailElement)
                && !string.IsNullOrWhiteSpace(detailElement.GetString()))
                detail = detailElement.GetString()!;
            else if (document.RootElement.TryGetProperty("title", out var titleElement)
                && !string.IsNullOrWhiteSpace(titleElement.GetString()))
                detail = titleElement.GetString()!;
        }
        catch (JsonException)
        {
        }

        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(detail) ? $"Yêu cầu thất bại ({(int)response.StatusCode})." : detail,
            null, response.StatusCode);
    }
}

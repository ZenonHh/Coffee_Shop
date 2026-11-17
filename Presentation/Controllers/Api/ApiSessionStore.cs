using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace CoffeeShop.Controllers.Api;

public sealed class ApiSessionStore(IMemoryCache cache)
{
    public string Create(int customerId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        cache.Set($"mobile-session:{token}", customerId, TimeSpan.FromDays(7));
        return token;
    }

    public bool TryGetCustomerId(string token, out int customerId) =>
        cache.TryGetValue($"mobile-session:{token}", out customerId);
}

public sealed class ApiSessionAuthorizationFilter(ApiSessionStore sessions) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authorization = context.HttpContext.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            || !sessions.TryGetCustomerId(authorization["Bearer ".Length..].Trim(), out var customerId))
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn."
            });
            return Task.CompletedTask;
        }

        context.HttpContext.Items["MobileCustomerId"] = customerId;
        return Task.CompletedTask;
    }
}

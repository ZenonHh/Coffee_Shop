using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthApiController(CoffeeShopDbContext db, ApiSessionStore sessions) : ControllerBase
{
    /// <summary>Sign in a customer and issue a seven-day mobile session token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.TenDangNhap.Trim();
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.TaiKhoan == username && c.TrangThai, cancellationToken);
        if (customer is null || !VerifyPassword(request.MatKhau, customer.MatKhau))
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "Tên đăng nhập hoặc mật khẩu không chính xác." });

        var account = await db.Accounts.Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.TenDangNhap == username && item.TrangThai, cancellationToken);
        if (account is not null
            && (!IsCustomerRole(account.MaVaiTro, account.Role?.TenVaiTro)
                || !VerifyPassword(request.MatKhau, account.MatKhau)))
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "Tên đăng nhập hoặc mật khẩu không chính xác." });

        if (!IsBcryptHash(customer.MatKhau))
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);
            customer.MatKhau = hash;
            if (account is not null)
                account.MatKhau = hash;
            await db.SaveChangesAsync(cancellationToken);
        }

        var token = sessions.Create(customer.MaKH);
        return Ok(ToAuthResponse(customer, token));
    }

    /// <summary>Create a customer account shared with the web storefront.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.TenDangNhap.Trim();
        var phone = request.Sdt.Trim();
        if (await db.Customers.AnyAsync(c => c.TaiKhoan == username || c.Sdt == phone, cancellationToken)
            || await db.Accounts.AnyAsync(a => a.TenDangNhap == username, cancellationToken))
            return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "Tên đăng nhập hoặc số điện thoại đã được sử dụng." });

        var customerRoleId = await db.Roles.AsNoTracking()
            .Where(role => role.TenVaiTro == "Khách hàng" || role.TenVaiTro == "Customer"
                || role.TenVaiTro == "KhachHang"
                || (role.MaVaiTro == 1 && role.TenVaiTro != "Admin" && role.TenVaiTro != "Staff" && role.TenVaiTro != "NhanVien"))
            .OrderBy(role => role.MaVaiTro == 1 ? 0 : 1)
            .Select(role => (int?)role.MaVaiTro)
            .FirstOrDefaultAsync(cancellationToken);
        if (customerRoleId is null)
            return Conflict(new ProblemDetails { Status = 409, Title = "Customer role is not configured", Detail = "Cơ sở dữ liệu chưa có vai trò khách hàng. Hãy cấu hình vai trò hiện có trước khi đăng ký; API không tự thay đổi schema hoặc dữ liệu vai trò." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);
        var customer = new Customer
        {
            TaiKhoan = username,
            MatKhau = passwordHash,
            HoTen = request.HoTen.Trim(),
            Sdt = phone,
            Email = request.Email?.Trim(),
            DiaChi = request.DiaChi?.Trim(),
            HangTV = "Đồng",
            DiemTichLuy = 0,
            NgayTao = DateTime.Now
        };
        var account = new Account
        {
            MaVaiTro = customerRoleId.Value,
            TenDangNhap = username,
            MatKhau = passwordHash,
            HoTen = customer.HoTen
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Customers.Add(customer);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var token = sessions.Create(customer.MaKH);
        return CreatedAtAction(nameof(Profile), new { }, ToAuthResponse(customer, token));
    }

    /// <summary>Get the profile of the currently authenticated customer.</summary>
    [HttpGet("profile")]
    [ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
    [ProducesResponseType(typeof(CustomerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerProfileResponse>> Profile(CancellationToken cancellationToken)
    {
        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        var profile = await db.Customers.AsNoTracking()
            .Where(customer => customer.MaKH == customerId)
            .Select(customer => new CustomerProfileResponse(
                customer.MaKH, customer.TaiKhoan!, customer.HoTen, customer.Sdt, customer.Email,
                customer.DiaChi, customer.DiemTichLuy, customer.HangTV))
            .FirstOrDefaultAsync(cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    private static AuthResponse ToAuthResponse(Customer customer, string token) =>
        new(token, new CustomerProfileResponse(customer.MaKH, customer.TaiKhoan ?? "", customer.HoTen,
            customer.Sdt, customer.Email, customer.DiaChi, customer.DiemTichLuy, customer.HangTV));

    internal static bool VerifyPassword(string supplied, string? stored)
    {
        if (string.IsNullOrEmpty(stored))
            return false;
        if (IsBcryptHash(stored))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(supplied, stored);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
        return string.Equals(supplied, stored, StringComparison.Ordinal);
    }

    private static bool IsBcryptHash(string? stored) =>
        stored?.StartsWith("$2", StringComparison.Ordinal) == true;

    internal static bool IsCustomerRole(int roleId, string? roleName) =>
        roleName is "Khách hàng" or "Customer" or "KhachHang"
        || (roleId == 1 && roleName is not ("Admin" or "Staff" or "NhanVien"));
}

/// <summary>Credentials used to sign in.</summary>
public sealed record LoginRequest
{
    [Required, StringLength(50)]
    public required string TenDangNhap { get; init; }

    [Required, StringLength(128)]
    public required string MatKhau { get; init; }
}

/// <summary>Fields required to register a customer.</summary>
public sealed record RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public required string TenDangNhap { get; init; }

    [Required, StringLength(128, MinimumLength = 8)]
    public required string MatKhau { get; init; }

    [Required, StringLength(100)]
    public required string HoTen { get; init; }

    [Required, Phone, StringLength(20)]
    public required string Sdt { get; init; }

    [EmailAddress, StringLength(100)]
    public string? Email { get; init; }

    [StringLength(255)]
    public string? DiaChi { get; init; }
}

/// <summary>Authentication result and customer profile.</summary>
public sealed record AuthResponse(string AccessToken, CustomerProfileResponse Customer);

/// <summary>Customer profile safe for mobile clients.</summary>
public sealed record CustomerProfileResponse(
    int MaKH, string TenDangNhap, string HoTen, string Sdt, string? Email,
    string? DiaChi, int DiemTichLuy, string HangTV);

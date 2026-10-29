using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Services;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthApiController(
    CoffeeShopDbContext db,
    ApiSessionStore sessions,
    ILogger<AuthApiController> logger) : ControllerBase
{
    /// <summary>Check whether a username is already registered before choosing sign-in or registration.</summary>
    [HttpPost("check-username")]
    [ProducesResponseType(typeof(UsernameCheckResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsernameCheckResponse>> CheckUsername(
        UsernameCheckRequest request, CancellationToken cancellationToken)
    {
        var username = request.TenDangNhap.Trim();
        var accountExists = await db.Accounts.AsNoTracking()
            .Where(account => account.TenDangNhap == username)
            .Select(account => account.MaTaiKhoan)
            .AnyAsync(cancellationToken);
        var customerExists = await db.Customers.AsNoTracking()
            .Where(customer => customer.TaiKhoan == username)
            .Select(customer => customer.MaKH)
            .AnyAsync(cancellationToken);

        return Ok(new UsernameCheckResponse(accountExists || customerExists));
    }

    /// <summary>Sign in a customer and issue a seven-day mobile session token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.TenDangNhap.Trim();
        var customer = await db.Customers.AsNoTracking()
            .Where(item => item.TaiKhoan == username && item.TrangThai)
            .Select(item => new CustomerCredential(
                item.MaKH, item.TaiKhoan, item.MatKhau, item.HoTen, item.Sdt,
                item.Email, item.DiaChi, item.DiemTichLuy, item.HangTV))
            .FirstOrDefaultAsync(cancellationToken);
        if (customer is null || !VerifyPassword(request.MatKhau, customer.MatKhau))
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "Tên đăng nhập hoặc mật khẩu không chính xác." });

        var account = await db.Accounts.AsNoTracking()
            .Where(item => item.TenDangNhap == username)
            .Select(item => new AccountCredential(
                item.MaTaiKhoan, item.MaVaiTro, item.MatKhau, item.Role!.TenVaiTro, item.TrangThai))
            .FirstOrDefaultAsync(cancellationToken);
        if (account is not null
            && (!account.IsActive
                || !IsCustomerRole(account.MaVaiTro, account.RoleName)
                || !VerifyPassword(request.MatKhau, account.MatKhau)))
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "Tên đăng nhập hoặc mật khẩu không chính xác." });

        if (!IsBcryptHash(customer.MatKhau))
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);
            await db.Customers.Where(item => item.MaKH == customer.MaKH)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.MatKhau, hash), cancellationToken);
            if (account is not null)
            {
                await db.Accounts.Where(item => item.MaTaiKhoan == account.MaTaiKhoan)
                    .ExecuteUpdateAsync(update => update.SetProperty(item => item.MatKhau, hash), cancellationToken);
            }
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
        if (Encoding.UTF8.GetByteCount(request.MatKhau) > 72)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Mật khẩu không hợp lệ",
                Detail = "Mật khẩu không được vượt quá 72 byte UTF-8."
            });

        try
        {
        var username = request.TenDangNhap.Trim();
        var phone = request.Sdt.Trim();
        if (await db.Customers.AsNoTracking()
                .Where(customer => customer.TaiKhoan == username || customer.Sdt == phone)
                .Select(customer => customer.MaKH)
                .AnyAsync(cancellationToken)
            || await db.Accounts.AsNoTracking()
                .Where(account => account.TenDangNhap == username)
                .Select(account => account.MaTaiKhoan)
                .AnyAsync(cancellationToken))
            return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "Tên đăng nhập hoặc số điện thoại đã được sử dụng." });

        await using var transaction = await db.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var customerRoleId = await CustomerRoleResolver.GetOrCreateAsync(db, cancellationToken);

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
            MaVaiTro = customerRoleId,
            TenDangNhap = username,
            MatKhau = passwordHash,
            HoTen = customer.HoTen
        };

        db.Customers.Add(customer);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var token = sessions.Create(customer.MaKH);
        return CreatedAtAction(nameof(Profile), new { },
            new AuthResponse(token, new CustomerProfileResponse(
                customer.MaKH, customer.TaiKhoan ?? "", customer.HoTen, customer.Sdt,
                customer.Email, customer.DiaChi, customer.DiemTichLuy, customer.HangTV)));
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            logger.LogWarning(exception,
                "Mobile registration conflict. SQL detail: {SqlError}",
                exception.InnerException?.Message ?? exception.Message);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Tài khoản đã tồn tại",
                Detail = "Tên đăng nhập hoặc số điện thoại đã được sử dụng. Vui lòng kiểm tra lại."
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Mobile registration failed. SQL detail: {SqlError}",
                exception.InnerException?.Message ?? exception.Message);
            return Problem(
                title: "Không thể đăng ký tài khoản",
                detail: "Đã xảy ra lỗi khi tạo tài khoản. Vui lòng thử lại sau.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
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

    /// <summary>List point changes for the currently authenticated customer.</summary>
    [HttpGet("points")]
    [ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
    [ProducesResponseType(typeof(IReadOnlyList<PointHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PointHistoryResponse>>> Points(CancellationToken cancellationToken)
    {
        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        var history = await db.PointHistories.AsNoTracking()
            .Where(item => item.MaKH == customerId)
            .OrderByDescending(item => item.NgayThucHien)
            .Select(item => new PointHistoryData(
                item.MaLSD, item.MaHD, item.DiemThayDoi, item.LyDo, item.NgayThucHien))
            .ToListAsync(cancellationToken);

        return Ok(history.Select(item => new PointHistoryResponse(
            item.MaLSD, item.MaHD, item.DiemThayDoi, item.LyDo,
            new DateTimeOffset(DateTime.SpecifyKind(item.NgayThucHien, DateTimeKind.Local))))
            .ToList());
    }

    private static AuthResponse ToAuthResponse(CustomerCredential customer, string token) =>
        new(token, new CustomerProfileResponse(customer.MaKH, customer.TaiKhoan ?? "", customer.HoTen,
            customer.Sdt, customer.Email, customer.DiaChi, customer.DiemTichLuy, customer.HangTV));

    private sealed record CustomerCredential(
        int MaKH, string? TaiKhoan, string? MatKhau, string HoTen, string Sdt,
        string? Email, string? DiaChi, int DiemTichLuy, string HangTV);

    private sealed record AccountCredential(
        int MaTaiKhoan, int MaVaiTro, string MatKhau, string RoleName, bool IsActive);
    private sealed record PointHistoryData(
        int MaLSD, int? MaHD, int DiemThayDoi, string? LyDo, DateTime NgayThucHien);

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
        roleName?.Trim() is "Khách hàng" or "Customer" or "KhachHang";

    internal static bool IsUniqueConstraintViolation(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException { Number: 2601 or 2627 })
                return true;
        }

        return false;
    }
}

/// <summary>Username checked before selecting the authentication flow.</summary>
public sealed record UsernameCheckRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public required string TenDangNhap { get; init; }
}

/// <summary>Indicates whether the username is already registered.</summary>
public sealed record UsernameCheckResponse(bool Exists);

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

/// <summary>Point change in a customer's loyalty history.</summary>
public sealed record PointHistoryResponse(
    int MaLSD, int? MaHD, int DiemThayDoi, string? LyDo, DateTimeOffset NgayThucHien);

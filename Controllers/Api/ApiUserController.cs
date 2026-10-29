using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/mobile-user")]
[ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
public sealed class ApiUserController(CoffeeShopDbContext db) : ControllerBase
{
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProfile(
        UpdateMobileProfileRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        var phone = request.Sdt.Trim();
        if (await db.Customers.AsNoTracking()
                .Where(customer => customer.MaKH != customerId && customer.Sdt == phone)
                .Select(customer => customer.MaKH)
                .AnyAsync(cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Số điện thoại đã được sử dụng"
            });
        }

        var customer = await db.Customers
            .FirstOrDefaultAsync(item => item.MaKH == customerId, cancellationToken);
        if (customer is null)
            return NotFound();

        customer.HoTen = request.HoTen.Trim();
        customer.Sdt = phone;
        customer.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        customer.DiaChi = string.IsNullOrWhiteSpace(request.DiaChi) ? null : request.DiaChi.Trim();

        var username = customer.TaiKhoan;
        if (!string.IsNullOrWhiteSpace(username))
        {
            var account = await db.Accounts
                .FirstOrDefaultAsync(item => item.TenDangNhap == username, cancellationToken);
            if (account is not null)
                account.HoTen = customer.HoTen;
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        ChangeMobilePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (Encoding.UTF8.GetByteCount(request.NewPassword) > 72)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Mật khẩu mới không được vượt quá 72 byte UTF-8"
            });
        }

        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        var customer = await db.Customers
            .FirstOrDefaultAsync(item => item.MaKH == customerId, cancellationToken);
        if (customer is null)
            return NotFound();

        if (!AuthApiController.VerifyPassword(request.CurrentPassword, customer.MatKhau))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Mật khẩu hiện tại không chính xác"
            });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        customer.MatKhau = passwordHash;

        if (!string.IsNullOrWhiteSpace(customer.TaiKhoan))
        {
            var account = await db.Accounts
                .FirstOrDefaultAsync(item => item.TenDangNhap == customer.TaiKhoan, cancellationToken);
            if (account is not null)
                account.MatKhau = passwordHash;
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateMobileProfileRequest
{
    [Required, StringLength(100)]
    public required string HoTen { get; init; }

    [Required, Phone, StringLength(20)]
    public required string Sdt { get; init; }

    [EmailAddress, StringLength(100)]
    public string? Email { get; init; }

    [StringLength(255)]
    public string? DiaChi { get; init; }
}

public sealed record ChangeMobilePasswordRequest
{
    [Required, StringLength(128)]
    public required string CurrentPassword { get; init; }

    [Required, StringLength(128, MinimumLength = 8)]
    public required string NewPassword { get; init; }
}

using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Services;

public sealed class StaffMemberResolver(CoffeeShopDbContext context)
{
    public async Task<StaffMember> EnsureForAccountAsync(int accountId)
    {
        var account = await context.Accounts.AsNoTracking()
            .Where(value => value.MaTaiKhoan == accountId && value.TrangThai)
            .Select(value => new { value.HoTen, RoleName = value.Role!.TenVaiTro })
            .FirstOrDefaultAsync();
        if (account == null)
            throw new InvalidOperationException("Không tìm thấy tài khoản nội bộ đang hoạt động.");

        var fullName = account.HoTen.Trim();
        var separator = fullName.LastIndexOf(' ');
        var givenName = separator >= 0 ? fullName[(separator + 1)..] : "NV";
        var familyName = separator >= 0 ? fullName[..separator].Trim() : fullName;
        givenName = givenName.Length > 20 ? givenName[..20] : givenName;
        familyName = string.IsNullOrWhiteSpace(familyName) ? "Nhân viên" : familyName;
        familyName = familyName.Length > 50 ? familyName[..50] : familyName;

        var employee = await context.StaffMembers
            .FirstOrDefaultAsync(value => value.HoVaDem == familyName && value.Ten == givenName);
        if (employee != null)
            return employee;

        employee = new StaffMember
        {
            HoVaDem = familyName,
            Ten = givenName,
            ChucVu = account.RoleName
        };
        context.StaffMembers.Add(employee);
        await context.SaveChangesAsync();
        return employee;
    }
}

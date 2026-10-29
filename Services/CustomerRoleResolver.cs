using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Services;

public static class CustomerRoleResolver
{
    public static async Task<int> GetOrCreateAsync(
        CoffeeShopDbContext context,
        CancellationToken cancellationToken)
    {
        var roleId = await context.Roles.AsNoTracking()
            .OrderBy(item => item.MaVaiTro)
            .Where(
                item => item.TenVaiTro == "Khách hàng"
                    || item.TenVaiTro == "Customer"
                    || item.TenVaiTro == "KhachHang")
            .Select(item => (int?)item.MaVaiTro)
            .FirstOrDefaultAsync(cancellationToken);
        if (roleId.HasValue)
            return roleId.Value;

        var role = new Role
        {
            TenVaiTro = "Khách hàng",
            MoTa = "Tài khoản khách hàng"
        };
        context.Roles.Add(role);
        await context.SaveChangesAsync(cancellationToken);
        return role.MaVaiTro;
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using CoffeeShop.Data;

namespace CoffeeShop.Migrations;

[DbContext(typeof(CoffeeShopDbContext))]
[Migration("20261108100900_AddCrmRole")]
public partial class AddCrmRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE TenVaiTro = N'CRM')
                INSERT INTO dbo.VaiTro (TenVaiTro, MoTa)
                VALUES (N'CRM', N'Nhân viên chăm sóc khách hàng và CRM');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}

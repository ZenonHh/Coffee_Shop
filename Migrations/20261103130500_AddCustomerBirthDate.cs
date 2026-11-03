using CoffeeShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoffeeShop.Migrations;

[DbContext(typeof(CoffeeShopDbContext))]
[Migration("20261103130500_AddCustomerBirthDate")]
public partial class AddCustomerBirthDate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.KhachHang', N'NgaySinh') IS NULL
            BEGIN
                ALTER TABLE [dbo].[KhachHang] ADD [NgaySinh] date NULL;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}

using CoffeeShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoffeeShop.Migrations;

[DbContext(typeof(CoffeeShopDbContext))]
[Migration("20261103130400_NormalizeCompletedOrderStatus")]
public partial class NormalizeCompletedOrderStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NOT NULL
                UPDATE [dbo].[HoaDon] SET [TrangThai] = N'Hoàn thành' WHERE [TrangThai] = N'Hoàn tất';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}

using CoffeeShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoffeeShop.Migrations;

[DbContext(typeof(CoffeeShopDbContext))]
[Migration("20261105150800_AddStaffAttributionToOrders")]
public partial class AddStaffAttributionToOrders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "MaNV",
            table: "HoaDon",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_HoaDon_MaNV",
            table: "HoaDon",
            column: "MaNV");

        migrationBuilder.AddForeignKey(
            name: "FK_HoaDon_NhanVien_MaNV",
            table: "HoaDon",
            column: "MaNV",
            principalTable: "NhanVien",
            principalColumn: "MaNV",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_HoaDon_NhanVien_MaNV",
            table: "HoaDon");

        migrationBuilder.DropIndex(
            name: "IX_HoaDon_MaNV",
            table: "HoaDon");

        migrationBuilder.DropColumn(
            name: "MaNV",
            table: "HoaDon");
    }
}

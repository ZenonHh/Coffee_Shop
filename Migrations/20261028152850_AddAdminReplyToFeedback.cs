using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Migrations
{
    public partial class AddAdminReplyToFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayPhanHoiQuanTri",
                table: "PhanHoi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhanHoiQuanTri",
                table: "PhanHoi",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayPhanHoiQuanTri",
                table: "PhanHoi");

            migrationBuilder.DropColumn(
                name: "PhanHoiQuanTri",
                table: "PhanHoi");
        }
    }
}

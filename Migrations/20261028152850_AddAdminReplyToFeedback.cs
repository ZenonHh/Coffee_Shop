using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Migrations
{
    public partial class AddAdminReplyToFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhanHoi', N'NgayPhanHoiQuanTri') IS NULL
                    ALTER TABLE [dbo].[PhanHoi] ADD [NgayPhanHoiQuanTri] datetime2 NULL;
                IF COL_LENGTH(N'dbo.PhanHoi', N'PhanHoiQuanTri') IS NULL
                    ALTER TABLE [dbo].[PhanHoi] ADD [PhanHoiQuanTri] nvarchar(2000) NULL;
                """);
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

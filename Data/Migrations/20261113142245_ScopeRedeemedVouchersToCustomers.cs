using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Migrations
{
    /// <inheritdoc />
    public partial class ScopeRedeemedVouchersToCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaKH",
                table: "Voucher",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voucher_MaKH",
                table: "Voucher",
                column: "MaKH");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_KhachHang_MaKH",
                table: "Voucher",
                column: "MaKH",
                principalTable: "KhachHang",
                principalColumn: "MaKH",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                UPDATE voucher
                SET MaKH = history.MaKH
                FROM dbo.Voucher AS voucher
                CROSS APPLY
                (
                    SELECT TOP (1) pointHistory.MaKH
                    FROM dbo.LichSuDiem AS pointHistory
                    WHERE pointHistory.DiemThayDoi < 0
                        AND CHARINDEX(
                            N'mã voucher ' + voucher.MaCode,
                            COALESCE(pointHistory.LyDo, N'')
                        ) > 0
                    ORDER BY pointHistory.NgayThucHien DESC, pointHistory.MaLSD DESC
                ) AS history
                WHERE voucher.TenChuongTrinh LIKE N'Đổi điểm thưởng (%'
                    AND voucher.MaKH IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_KhachHang_MaKH",
                table: "Voucher");

            migrationBuilder.DropIndex(
                name: "IX_Voucher_MaKH",
                table: "Voucher");

            migrationBuilder.DropColumn(
                name: "MaKH",
                table: "Voucher");
        }
    }
}

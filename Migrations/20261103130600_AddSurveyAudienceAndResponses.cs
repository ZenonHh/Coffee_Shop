using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Migrations
{
    public partial class AddSurveyAudienceAndResponses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.KhaoSat', N'DoiTuong') IS NULL
                    ALTER TABLE [dbo].[KhaoSat] ADD [DoiTuong] nvarchar(20) NULL;

                UPDATE [dbo].[KhaoSat]
                SET [DoiTuong] = N'TatCa'
                WHERE [DoiTuong] IS NULL;

                ALTER TABLE [dbo].[KhaoSat] ALTER COLUMN [DoiTuong] nvarchar(20) NOT NULL;
                """);

            migrationBuilder.AddColumn<int>(
                name: "ThuTu",
                table: "CauHoiKhaoSat",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
WITH OrderedQuestions AS
(
    SELECT [MaCauHoi], ROW_NUMBER() OVER (PARTITION BY [MaKhaoSat] ORDER BY [MaCauHoi]) AS [QuestionOrder]
    FROM [CauHoiKhaoSat]
)
UPDATE question
SET [ThuTu] = ordered.[QuestionOrder]
FROM [CauHoiKhaoSat] AS question
INNER JOIN OrderedQuestions AS ordered ON ordered.[MaCauHoi] = question.[MaCauHoi]");

            migrationBuilder.AlterColumn<int>(
                name: "ThuTu",
                table: "CauHoiKhaoSat",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.Sql(@"
UPDATE [CauHoiKhaoSat]
SET [LoaiCauHoi] = CASE [LoaiCauHoi]
    WHEN N'TRAC_NGHIEM' THEN N'SAO'
    WHEN N'TEXT' THEN N'TU_LUAN'
    ELSE [LoaiCauHoi]
END
WHERE [LoaiCauHoi] IN (N'TRAC_NGHIEM', N'TEXT')");

            migrationBuilder.CreateTable(
                name: "DoiTuongKhaoSat",
                columns: table => new
                {
                    MaKhaoSat = table.Column<int>(type: "int", nullable: false),
                    MaKH = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoiTuongKhaoSat", x => new { x.MaKhaoSat, x.MaKH });
                    table.ForeignKey(
                        name: "FK_DoiTuongKhaoSat_KhachHang_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoiTuongKhaoSat_KhaoSat_MaKhaoSat",
                        column: x => x.MaKhaoSat,
                        principalTable: "KhaoSat",
                        principalColumn: "MaKhaoSat",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LuotThamGiaKhaoSat",
                columns: table => new
                {
                    MaLuotTraLoi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhaoSat = table.Column<int>(type: "int", nullable: false),
                    MaKH = table.Column<int>(type: "int", nullable: false),
                    NgayHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuotThamGiaKhaoSat", x => x.MaLuotTraLoi);
                    table.ForeignKey(
                        name: "FK_LuotThamGiaKhaoSat_KhachHang_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LuotThamGiaKhaoSat_KhaoSat_MaKhaoSat",
                        column: x => x.MaKhaoSat,
                        principalTable: "KhaoSat",
                        principalColumn: "MaKhaoSat",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DapAnLuaChonKhaoSat",
                columns: table => new
                {
                    MaLuaChon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCauHoi = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DapAnLuaChonKhaoSat", x => x.MaLuaChon);
                    table.ForeignKey(
                        name: "FK_DapAnLuaChonKhaoSat_CauHoiKhaoSat_MaCauHoi",
                        column: x => x.MaCauHoi,
                        principalTable: "CauHoiKhaoSat",
                        principalColumn: "MaCauHoi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "MaLuotTraLoi",
                table: "CauTraLoi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoiTuongKhaoSat_MaKH",
                table: "DoiTuongKhaoSat",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_LuotThamGiaKhaoSat_MaKH",
                table: "LuotThamGiaKhaoSat",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_LuotThamGiaKhaoSat_MaKhaoSat_MaKH",
                table: "LuotThamGiaKhaoSat",
                columns: new[] { "MaKhaoSat", "MaKH" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DapAnLuaChonKhaoSat_MaCauHoi",
                table: "DapAnLuaChonKhaoSat",
                column: "MaCauHoi");

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_MaLuotTraLoi",
                table: "CauTraLoi",
                column: "MaLuotTraLoi");

            migrationBuilder.AddForeignKey(
                name: "FK_CauTraLoi_LuotThamGiaKhaoSat_MaLuotTraLoi",
                table: "CauTraLoi",
                column: "MaLuotTraLoi",
                principalTable: "LuotThamGiaKhaoSat",
                principalColumn: "MaLuotTraLoi",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauTraLoi_LuotThamGiaKhaoSat_MaLuotTraLoi",
                table: "CauTraLoi");

            migrationBuilder.DropTable(name: "DapAnLuaChonKhaoSat");
            migrationBuilder.DropTable(name: "DoiTuongKhaoSat");
            migrationBuilder.DropTable(name: "LuotThamGiaKhaoSat");

            migrationBuilder.DropIndex(
                name: "IX_CauTraLoi_MaLuotTraLoi",
                table: "CauTraLoi");
            migrationBuilder.DropColumn(
                name: "MaLuotTraLoi",
                table: "CauTraLoi");
            migrationBuilder.DropColumn(
                name: "ThuTu",
                table: "CauHoiKhaoSat");
            migrationBuilder.DropColumn(
                name: "DoiTuong",
                table: "KhaoSat");
        }
    }
}

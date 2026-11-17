using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using CoffeeShop.Data;

namespace CoffeeShop.Migrations;

[DbContext(typeof(CoffeeShopDbContext))]
[Migration("20261109213100_MakeLegacySurveyInvitationOptional")]
public partial class MakeLegacySurveyInvitationOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.CauTraLoi', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CauTraLoi', N'MaLoiMoi') IS NOT NULL
                AND EXISTS
                (
                    SELECT 1
                    FROM sys.columns
                    WHERE [object_id] = OBJECT_ID(N'dbo.CauTraLoi')
                        AND [name] = N'MaLoiMoi'
                        AND [is_nullable] = 0
                )
            BEGIN
                ALTER TABLE dbo.CauTraLoi ALTER COLUMN MaLoiMoi int NULL;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.CauTraLoi', N'U') IS NOT NULL
                AND COL_LENGTH(N'dbo.CauTraLoi', N'MaLoiMoi') IS NOT NULL
                AND EXISTS
                (
                    SELECT 1
                    FROM sys.columns
                    WHERE [object_id] = OBJECT_ID(N'dbo.CauTraLoi')
                        AND [name] = N'MaLoiMoi'
                        AND [is_nullable] = 1
                )
            BEGIN
                IF EXISTS (SELECT 1 FROM dbo.CauTraLoi WHERE MaLoiMoi IS NULL)
                    THROW 51000, N'Cannot restore MaLoiMoi as NOT NULL while survey answer rows have no legacy invitation.', 1;

                ALTER TABLE dbo.CauTraLoi ALTER COLUMN MaLoiMoi int NOT NULL;
            END;
            """);
    }
}

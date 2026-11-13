SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Voucher', N'MaKH') IS NULL
BEGIN
    ALTER TABLE dbo.Voucher ADD MaKH int NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Voucher')
      AND name = N'IX_Voucher_MaKH'
)
BEGIN
    EXEC sys.sp_executesql N'CREATE INDEX IX_Voucher_MaKH ON dbo.Voucher(MaKH);';
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Voucher_KhachHang_MaKH'
)
BEGIN
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.Voucher WITH CHECK
        ADD CONSTRAINT FK_Voucher_KhachHang_MaKH
            FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH);';
END;

DECLARE @AssignedVoucherCount int = 0;
EXEC sys.sp_executesql
    N'
    ;WITH RedeemedVoucherOwners AS
    (
        SELECT
            voucher.MaVoucher,
            pointHistory.MaKH,
            ROW_NUMBER() OVER
            (
                PARTITION BY voucher.MaVoucher
                ORDER BY pointHistory.NgayThucHien DESC, pointHistory.MaLSD DESC
            ) AS RowNumber
        FROM dbo.Voucher AS voucher
        INNER JOIN dbo.LichSuDiem AS pointHistory
            ON pointHistory.DiemThayDoi < 0
           AND CHARINDEX(
                N''mã voucher '' + voucher.MaCode,
                COALESCE(pointHistory.LyDo, N'''')
           ) > 0
        WHERE voucher.TenChuongTrinh LIKE N''Đổi điểm thưởng (%''
          AND voucher.MaKH IS NULL
    )
    UPDATE voucher
    SET MaKH = owners.MaKH
    FROM dbo.Voucher AS voucher
    INNER JOIN RedeemedVoucherOwners AS owners
        ON owners.MaVoucher = voucher.MaVoucher
       AND owners.RowNumber = 1;
    SET @UpdatedCount = @@ROWCOUNT;',
    N'@UpdatedCount int OUTPUT',
    @UpdatedCount = @AssignedVoucherCount OUTPUT;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    ;THROW 50000, 'EF migration history table was not found; run this script against the application database.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20261113142245_ScopeRedeemedVouchersToCustomers'
)
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20261113142245_ScopeRedeemedVouchersToCustomers', N'8.0.0');
END;

COMMIT TRANSACTION;

SELECT @AssignedVoucherCount AS AssignedVoucherCount;

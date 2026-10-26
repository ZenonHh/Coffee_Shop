SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
    THROW 50001, N'Không tìm thấy bảng dbo.HoaDon.', 1;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.HoaDon', N'TrangThai') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDon
        ADD TrangThai NVARCHAR(50) NOT NULL
            CONSTRAINT DF_HoaDon_TrangThai_Workflow DEFAULT (N'Chờ xác nhận');
END
ELSE
BEGIN
    UPDATE dbo.HoaDon
    SET TrangThai = N'Chờ xác nhận'
    WHERE TrangThai IS NULL;

    ALTER TABLE dbo.HoaDon
        ALTER COLUMN TrangThai NVARCHAR(50) NOT NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON c.object_id = dc.parent_object_id
        AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.HoaDon')
        AND c.name = N'TrangThai'
)
BEGIN
    ALTER TABLE dbo.HoaDon
        ADD CONSTRAINT DF_HoaDon_TrangThai_Workflow DEFAULT (N'Chờ xác nhận') FOR TrangThai;
END;

IF COL_LENGTH(N'dbo.HoaDon', N'LyDoHuy') IS NULL
    ALTER TABLE dbo.HoaDon ADD LyDoHuy NVARCHAR(500) NULL;

IF COL_LENGTH(N'dbo.HoaDon', N'NguoiXuLy') IS NULL
    ALTER TABLE dbo.HoaDon ADD NguoiXuLy NVARCHAR(100) NULL;

IF COL_LENGTH(N'dbo.HoaDon', N'NgayCapNhat') IS NULL
    ALTER TABLE dbo.HoaDon ADD NgayCapNhat DATETIME2 NULL;

IF COL_LENGTH(N'dbo.HoaDon', N'PhuongThucThanhToan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDon
        ADD PhuongThucThanhToan NVARCHAR(50) NOT NULL
            CONSTRAINT DF_HoaDon_PhuongThucThanhToan DEFAULT (N'COD');
END;

UPDATE dbo.HoaDon
SET TrangThai = N'Hoàn thành'
WHERE TrangThai IN (N'Hoàn tất', N'Đã thanh toán');

COMMIT TRANSACTION;

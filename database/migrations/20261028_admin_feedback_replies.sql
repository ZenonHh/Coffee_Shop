SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.PhanHoi', N'U') IS NULL
    BEGIN
        THROW 50001, N'Không tìm thấy bảng dbo.PhanHoi trong database hiện tại.', 1;
    END;

    IF COL_LENGTH(N'dbo.PhanHoi', N'PhanHoiQuanTri') IS NULL
    BEGIN
        ALTER TABLE dbo.PhanHoi
        ADD PhanHoiQuanTri NVARCHAR(2000) NULL;
    END;

    IF COL_LENGTH(N'dbo.PhanHoi', N'NgayPhanHoiQuanTri') IS NULL
    BEGIN
        ALTER TABLE dbo.PhanHoi
        ADD NgayPhanHoiQuanTri DATETIME2 NULL;
    END;

    COMMIT TRANSACTION;
    PRINT N'Đã kiểm tra/cập nhật các cột phản hồi admin trong dbo.PhanHoi.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO

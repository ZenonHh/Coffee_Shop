USE CoffeeShopDb;
GO

SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.KhaoSat', N'U') IS NULL
    BEGIN
        ;THROW 51001, N'Khong tim thay bang dbo.KhaoSat trong database hien tai.', 1;
    END;

    IF COL_LENGTH(N'dbo.KhaoSat', N'DoiTuong') IS NULL
    BEGIN
        ALTER TABLE dbo.KhaoSat
            ADD [DoiTuong] NVARCHAR(20) NOT NULL
                CONSTRAINT [DF_KhaoSat_DoiTuong] DEFAULT (N'TatCa') WITH VALUES;
    END;

    IF COL_LENGTH(N'dbo.CauHoiKhaoSat', N'ThuTu') IS NULL
        ALTER TABLE dbo.CauHoiKhaoSat ADD [ThuTu] INT NULL;

    EXEC sys.sp_executesql N'
        ;WITH OrderedQuestions AS
        (
            SELECT [MaCauHoi], ROW_NUMBER() OVER (PARTITION BY [MaKhaoSat] ORDER BY [MaCauHoi]) AS [QuestionOrder]
            FROM dbo.CauHoiKhaoSat
        )
        UPDATE question
        SET [ThuTu] = ordered.[QuestionOrder]
        FROM dbo.CauHoiKhaoSat AS question
        INNER JOIN OrderedQuestions AS ordered ON ordered.[MaCauHoi] = question.[MaCauHoi];';

    EXEC sys.sp_executesql N'ALTER TABLE dbo.CauHoiKhaoSat ALTER COLUMN [ThuTu] INT NOT NULL;';

    UPDATE dbo.CauHoiKhaoSat
    SET [LoaiCauHoi] = CASE [LoaiCauHoi]
        WHEN N'TRAC_NGHIEM' THEN N'SAO'
        WHEN N'TEXT' THEN N'TU_LUAN'
        ELSE [LoaiCauHoi]
    END
    WHERE [LoaiCauHoi] IN (N'TRAC_NGHIEM', N'TEXT');

    IF OBJECT_ID(N'dbo.DoiTuongKhaoSat', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DoiTuongKhaoSat
        (
            [MaKhaoSat] INT NOT NULL,
            [MaKH] INT NOT NULL,
            CONSTRAINT [PK_DoiTuongKhaoSat] PRIMARY KEY ([MaKhaoSat], [MaKH]),
            CONSTRAINT [FK_DoiTuongKhaoSat_KhaoSat_MaKhaoSat]
                FOREIGN KEY ([MaKhaoSat]) REFERENCES dbo.KhaoSat ([MaKhaoSat]) ON DELETE CASCADE,
            CONSTRAINT [FK_DoiTuongKhaoSat_KhachHang_MaKH]
                FOREIGN KEY ([MaKH]) REFERENCES dbo.KhachHang ([MaKH])
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_DoiTuongKhaoSat_MaKH' AND [object_id] = OBJECT_ID(N'dbo.DoiTuongKhaoSat'))
        CREATE INDEX [IX_DoiTuongKhaoSat_MaKH] ON dbo.DoiTuongKhaoSat ([MaKH]);

    IF OBJECT_ID(N'dbo.LuotThamGiaKhaoSat', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.LuotThamGiaKhaoSat
        (
            [MaLuotTraLoi] INT IDENTITY(1,1) NOT NULL,
            [MaKhaoSat] INT NOT NULL,
            [MaKH] INT NOT NULL,
            [NgayHoanThanh] DATETIME2 NOT NULL,
            CONSTRAINT [PK_LuotThamGiaKhaoSat] PRIMARY KEY ([MaLuotTraLoi]),
            CONSTRAINT [FK_LuotThamGiaKhaoSat_KhaoSat_MaKhaoSat]
                FOREIGN KEY ([MaKhaoSat]) REFERENCES dbo.KhaoSat ([MaKhaoSat]) ON DELETE CASCADE,
            CONSTRAINT [FK_LuotThamGiaKhaoSat_KhachHang_MaKH]
                FOREIGN KEY ([MaKH]) REFERENCES dbo.KhachHang ([MaKH])
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_LuotThamGiaKhaoSat_MaKH' AND [object_id] = OBJECT_ID(N'dbo.LuotThamGiaKhaoSat'))
        CREATE INDEX [IX_LuotThamGiaKhaoSat_MaKH] ON dbo.LuotThamGiaKhaoSat ([MaKH]);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_LuotThamGiaKhaoSat_MaKhaoSat_MaKH' AND [object_id] = OBJECT_ID(N'dbo.LuotThamGiaKhaoSat'))
        CREATE UNIQUE INDEX [IX_LuotThamGiaKhaoSat_MaKhaoSat_MaKH]
            ON dbo.LuotThamGiaKhaoSat ([MaKhaoSat], [MaKH]);

    IF OBJECT_ID(N'dbo.DapAnLuaChonKhaoSat', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DapAnLuaChonKhaoSat
        (
            [MaLuaChon] INT IDENTITY(1,1) NOT NULL,
            [MaCauHoi] INT NOT NULL,
            [NoiDung] NVARCHAR(255) NOT NULL,
            [ThuTu] INT NOT NULL,
            CONSTRAINT [PK_DapAnLuaChonKhaoSat] PRIMARY KEY ([MaLuaChon]),
            CONSTRAINT [FK_DapAnLuaChonKhaoSat_CauHoiKhaoSat_MaCauHoi]
                FOREIGN KEY ([MaCauHoi]) REFERENCES dbo.CauHoiKhaoSat ([MaCauHoi]) ON DELETE CASCADE
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_DapAnLuaChonKhaoSat_MaCauHoi' AND [object_id] = OBJECT_ID(N'dbo.DapAnLuaChonKhaoSat'))
        CREATE INDEX [IX_DapAnLuaChonKhaoSat_MaCauHoi] ON dbo.DapAnLuaChonKhaoSat ([MaCauHoi]);

    IF COL_LENGTH(N'dbo.CauTraLoi', N'MaLuotTraLoi') IS NULL
        ALTER TABLE dbo.CauTraLoi ADD [MaLuotTraLoi] INT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_CauTraLoi_MaLuotTraLoi' AND [object_id] = OBJECT_ID(N'dbo.CauTraLoi'))
        CREATE INDEX [IX_CauTraLoi_MaLuotTraLoi] ON dbo.CauTraLoi ([MaLuotTraLoi]);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_CauTraLoi_LuotThamGiaKhaoSat_MaLuotTraLoi')
        ALTER TABLE dbo.CauTraLoi
            ADD CONSTRAINT [FK_CauTraLoi_LuotThamGiaKhaoSat_MaLuotTraLoi]
                FOREIGN KEY ([MaLuotTraLoi]) REFERENCES dbo.LuotThamGiaKhaoSat ([MaLuotTraLoi]);

    COMMIT TRANSACTION;
    PRINT N'Da kiem tra/cap nhat schema khao sat va luot tham gia.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

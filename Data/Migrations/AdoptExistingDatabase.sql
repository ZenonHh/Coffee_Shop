SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @RequiredTables TABLE ([Name] sysname NOT NULL);
INSERT INTO @RequiredTables ([Name])
VALUES
    (N'CoffeeShopBrowserCart'),
    (N'KhachHang'),
    (N'LoaiSanPham'),
    (N'VaiTro'),
    (N'Voucher'),
    (N'HoaDon'),
    (N'LichSuDiem'),
    (N'SanPham'),
    (N'TaiKhoan'),
    (N'BienTheSanPham'),
    (N'PhanHoi'),
    (N'KhaoSat'),
    (N'ChiTietHoaDon'),
    (N'CauHoiKhaoSat'),
    (N'ChiTietHoaDon_Topping'),
    (N'CauTraLoi');

IF EXISTS
(
    SELECT 1
    FROM @RequiredTables AS required
    WHERE OBJECT_ID(N'dbo.' + QUOTENAME(required.[Name]), N'U') IS NULL
)
BEGIN
    SELECT required.[Name] AS [MissingTable]
    FROM @RequiredTables AS required
    WHERE OBJECT_ID(N'dbo.' + QUOTENAME(required.[Name]), N'U') IS NULL;

    THROW 51000, 'Existing database is missing tables from InitialCreate. No migration baseline was recorded.', 1;
END;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory]
    (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM [dbo].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024104705_InitialCreate'
)
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261024104705_InitialCreate', N'8.0.0');
END;

COMMIT TRANSACTION;

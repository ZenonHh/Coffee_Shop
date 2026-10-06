USE [Coffee_Shop_DB];
GO

IF COL_LENGTH('dbo.SanPham', 'HinhAnh') IS NULL
BEGIN
    ALTER TABLE dbo.SanPham ADD HinhAnh NVARCHAR(255) NULL;
END;
GO

IF COL_LENGTH('dbo.HoaDon', 'TenNguoiNhan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDon ADD TenNguoiNhan NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH('dbo.HoaDon', 'SdtNguoiNhan') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDon ADD SdtNguoiNhan VARCHAR(20) NULL;
END;
GO

IF COL_LENGTH('dbo.HoaDon', 'DiaChiGiaoHang') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDon ADD DiaChiGiaoHang NVARCHAR(255) NULL;
END;
GO

IF COL_LENGTH('dbo.HoaDon', 'GhiChuGiaoHang') IS NULL
BEGIN
    ALTER TABLE dbo.HoaDon ADD GhiChuGiaoHang NVARCHAR(500) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LoaiSanPham WHERE TenLoaiSP = N'Cà phê & Matcha')
    INSERT INTO dbo.LoaiSanPham (TenLoaiSP, ThuTuHienThi) VALUES (N'Cà phê & Matcha', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.LoaiSanPham WHERE TenLoaiSP = N'Trà & Đồ uống')
    INSERT INTO dbo.LoaiSanPham (TenLoaiSP, ThuTuHienThi) VALUES (N'Trà & Đồ uống', 2);
IF NOT EXISTS (SELECT 1 FROM dbo.LoaiSanPham WHERE TenLoaiSP = N'Ăn nhẹ')
    INSERT INTO dbo.LoaiSanPham (TenLoaiSP, ThuTuHienThi) VALUES (N'Ăn nhẹ', 3);
IF NOT EXISTS (SELECT 1 FROM dbo.LoaiSanPham WHERE TenLoaiSP = N'Tráng miệng')
    INSERT INTO dbo.LoaiSanPham (TenLoaiSP, ThuTuHienThi) VALUES (N'Tráng miệng', 4);
IF NOT EXISTS (SELECT 1 FROM dbo.LoaiSanPham WHERE TenLoaiSP = N'Topping')
    INSERT INTO dbo.LoaiSanPham (TenLoaiSP, ThuTuHienThi) VALUES (N'Topping', 5);
GO

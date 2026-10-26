USE CoffeeShopDB;
GO

IF COL_LENGTH(N'dbo.sanpham', N'SoLuongTon') IS NULL
    ALTER TABLE dbo.[sanpham] ADD [SoLuongTon] INT NOT NULL CONSTRAINT [DF_sanpham_SoLuongTon] DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.phieunhap', N'TrangThai') IS NULL
    ALTER TABLE dbo.[phieunhap] ADD [TrangThai] NVARCHAR(50) NOT NULL CONSTRAINT [DF_phieunhap_TrangThai] DEFAULT (N'Chờ duyệt');
IF COL_LENGTH(N'dbo.phieunhap', N'NguoiLap') IS NULL
    ALTER TABLE dbo.[phieunhap] ADD [NguoiLap] NVARCHAR(100) NOT NULL CONSTRAINT [DF_phieunhap_NguoiLap] DEFAULT (N'');
IF COL_LENGTH(N'dbo.phieunhap', N'NguoiDuyetId') IS NULL
    ALTER TABLE dbo.[phieunhap] ADD [NguoiDuyetId] INT NULL;
IF COL_LENGTH(N'dbo.phieunhap', N'NguoiDuyet') IS NULL
    ALTER TABLE dbo.[phieunhap] ADD [NguoiDuyet] NVARCHAR(100) NULL;
IF COL_LENGTH(N'dbo.phieunhap', N'NgayDuyet') IS NULL
    ALTER TABLE dbo.[phieunhap] ADD [NgayDuyet] DATETIME2 NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_phieunhap_nguoiduyet')
    ALTER TABLE dbo.[phieunhap] ADD CONSTRAINT [fk_phieunhap_nguoiduyet]
        FOREIGN KEY ([NguoiDuyetId]) REFERENCES dbo.[taikhoan] ([MaTaiKhoan]);
GO

IF COL_LENGTH(N'dbo.chitietphieunhap', N'MaSP') IS NULL
    ALTER TABLE dbo.[chitietphieunhap] ADD [MaSP] INT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.chitietphieunhap') AND name = N'MaNL' AND is_nullable = 0)
    ALTER TABLE dbo.[chitietphieunhap] ALTER COLUMN [MaNL] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_ctpn_sanpham')
    ALTER TABLE dbo.[chitietphieunhap] ADD CONSTRAINT [fk_ctpn_sanpham]
        FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
GO

IF COL_LENGTH(N'dbo.voucher', N'DonToiThieu') IS NULL
    ALTER TABLE dbo.[voucher] ADD [DonToiThieu] DECIMAL(12,2) NOT NULL CONSTRAINT [DF_voucher_DonToiThieu] DEFAULT (0);
IF COL_LENGTH(N'dbo.voucher', N'TenChuongTrinh') IS NULL
    ALTER TABLE dbo.[voucher] ADD [TenChuongTrinh] NVARCHAR(150) NULL;
IF COL_LENGTH(N'dbo.voucher', N'GiamToiDa') IS NULL
    ALTER TABLE dbo.[voucher] ADD [GiamToiDa] DECIMAL(12,2) NULL;
IF COL_LENGTH(N'dbo.voucher', N'SoLuongToiDa') IS NULL
    ALTER TABLE dbo.[voucher] ADD [SoLuongToiDa] INT NOT NULL CONSTRAINT [DF_voucher_SoLuongToiDa] DEFAULT (100);
IF COL_LENGTH(N'dbo.voucher', N'SoLuongDaDung') IS NULL
    ALTER TABLE dbo.[voucher] ADD [SoLuongDaDung] INT NOT NULL CONSTRAINT [DF_voucher_SoLuongDaDung] DEFAULT (0);
IF COL_LENGTH(N'dbo.voucher', N'NgayBatDau') IS NULL
    ALTER TABLE dbo.[voucher] ADD [NgayBatDau] DATETIME2 NULL;
IF COL_LENGTH(N'dbo.voucher', N'NgayKetThuc') IS NULL
    ALTER TABLE dbo.[voucher] ADD [NgayKetThuc] DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.hoadon', N'MaVoucher') IS NULL
    ALTER TABLE dbo.[hoadon] ADD [MaVoucher] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_hoadon_voucher')
    ALTER TABLE dbo.[hoadon] ADD CONSTRAINT [fk_hoadon_voucher]
        FOREIGN KEY ([MaVoucher]) REFERENCES dbo.[voucher] ([MaVoucher]);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.[vaitro] WHERE [MaVaiTro] = 4)
BEGIN
    SET IDENTITY_INSERT dbo.[vaitro] ON;
    INSERT INTO dbo.[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (4, N'Admin', N'Quản trị hệ thống');
    SET IDENTITY_INSERT dbo.[vaitro] OFF;
END
ELSE
    UPDATE dbo.[vaitro] SET [TenVaiTro] = N'Admin' WHERE [MaVaiTro] = 4 AND [TenVaiTro] <> N'Admin';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.[vaitro] WHERE [MaVaiTro] = 3)
BEGIN
    SET IDENTITY_INSERT dbo.[vaitro] ON;
    INSERT INTO dbo.[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (3, N'NhanVien', N'Nhân viên vận hành');
    SET IDENTITY_INSERT dbo.[vaitro] OFF;
END;
GO
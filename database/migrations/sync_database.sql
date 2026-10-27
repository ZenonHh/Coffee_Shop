USE CoffeeShopDB;
GO

-- 1. Tao cac bang con thieu
IF OBJECT_ID(N'dbo.nhacungcap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[nhacungcap] (
        [MaNCC] INT IDENTITY(1,1) NOT NULL,
        [TenNCC] NVARCHAR(150) NOT NULL,
        [Sdt] NVARCHAR(20) NULL,
        [DiaChi] NVARCHAR(255) NULL,
        [TrangThai] BIT NULL DEFAULT 1,
        CONSTRAINT [PK_nhacungcap] PRIMARY KEY ([MaNCC])
    );
END;
GO

IF OBJECT_ID(N'dbo.nguyenlieu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[nguyenlieu] (
        [MaNL] INT IDENTITY(1,1) NOT NULL,
        [TenNL] NVARCHAR(100) NOT NULL,
        [DonViTinh] NVARCHAR(20) NULL,
        [GiaVon] DECIMAL(12,2) NULL DEFAULT 0.00,
        CONSTRAINT [PK_nguyenlieu] PRIMARY KEY ([MaNL])
    );
END;
GO

IF OBJECT_ID(N'dbo.nhanvien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[nhanvien] (
        [MaNV] INT IDENTITY(1,1) NOT NULL,
        [HoVaDem] NVARCHAR(50) NOT NULL,
        [Ten] NVARCHAR(20) NOT NULL,
        [Sdt] NVARCHAR(20) NULL,
        [ChucVu] NVARCHAR(50) NULL,
        CONSTRAINT [PK_nhanvien] PRIMARY KEY ([MaNV])
    );
END;
GO

IF OBJECT_ID(N'dbo.phieunhap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[phieunhap] (
        [MaPN] INT IDENTITY(1,1) NOT NULL,
        [MaNCC] INT NOT NULL,
        [MaNV] INT NOT NULL,
        [NgayNhap] DATETIME2 NULL DEFAULT GETDATE(),
        [TongTien] DECIMAL(12,2) NULL DEFAULT 0.00,
        [GhiChu] NVARCHAR(255) NULL,
        [NguoiLap] NVARCHAR(100) NOT NULL DEFAULT (N''),
        [NguoiDuyet] NVARCHAR(100) NULL,
        [TrangThai] NVARCHAR(50) NOT NULL DEFAULT (N'Chờ duyệt'),
        [NguoiDuyetId] INT NULL,
        [NgayDuyet] DATETIME2 NULL,
        CONSTRAINT [PK_phieunhap] PRIMARY KEY ([MaPN])
    );
END;
GO

IF OBJECT_ID(N'dbo.chitietphieunhap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[chitietphieunhap] (
        [MaCTPN] INT IDENTITY(1,1) NOT NULL,
        [MaPN] INT NOT NULL,
        [MaNL] INT NULL,
        [MaSP] INT NULL,
        [SoLuong] DECIMAL(10,2) NOT NULL,
        [DonGia] DECIMAL(12,2) NOT NULL,
        [ThanhTien] DECIMAL(12,2) NOT NULL,
        CONSTRAINT [PK_chitietphieunhap] PRIMARY KEY ([MaCTPN])
    );
END;
GO

-- 2. Tao khoa ngoai
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_phieunhap_ncc')
    ALTER TABLE dbo.[phieunhap] ADD CONSTRAINT [fk_phieunhap_ncc]
        FOREIGN KEY ([MaNCC]) REFERENCES dbo.[nhacungcap] ([MaNCC]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_phieunhap_nhanvien')
    ALTER TABLE dbo.[phieunhap] ADD CONSTRAINT [fk_phieunhap_nhanvien]
        FOREIGN KEY ([MaNV]) REFERENCES dbo.[nhanvien] ([MaNV]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_phieunhap_nguoiduyet')
    ALTER TABLE dbo.[phieunhap] ADD CONSTRAINT [fk_phieunhap_nguoiduyet]
        FOREIGN KEY ([NguoiDuyetId]) REFERENCES dbo.[taikhoan] ([MaTaiKhoan]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_ctpn_phieunhap')
    ALTER TABLE dbo.[chitietphieunhap] ADD CONSTRAINT [fk_ctpn_phieunhap]
        FOREIGN KEY ([MaPN]) REFERENCES dbo.[phieunhap] ([MaPN]) ON DELETE CASCADE;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_ctpn_nguyenlieu')
    ALTER TABLE dbo.[chitietphieunhap] ADD CONSTRAINT [fk_ctpn_nguyenlieu]
        FOREIGN KEY ([MaNL]) REFERENCES dbo.[nguyenlieu] ([MaNL]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_ctpn_sanpham')
    ALTER TABLE dbo.[chitietphieunhap] ADD CONSTRAINT [fk_ctpn_sanpham]
        FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
GO

-- 3. Bo sung cot cho SanPham
IF COL_LENGTH(N'dbo.sanpham', N'SoLuongTon') IS NULL
BEGIN
    ALTER TABLE dbo.[sanpham] ADD [SoLuongTon] INT NOT NULL CONSTRAINT [DF_sanpham_SoLuongTon] DEFAULT (50);
END;
GO
UPDATE dbo.[sanpham] SET [SoLuongTon] = 50 WHERE [SoLuongTon] <= 0;
GO

-- 4. Bo sung cot cho Voucher
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

-- 5. Bo sung cot cho HoaDon
IF COL_LENGTH(N'dbo.hoadon', N'MaVoucher') IS NULL
    ALTER TABLE dbo.[hoadon] ADD [MaVoucher] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'fk_hoadon_voucher')
    ALTER TABLE dbo.[hoadon] ADD CONSTRAINT [fk_hoadon_voucher]
        FOREIGN KEY ([MaVoucher]) REFERENCES dbo.[voucher] ([MaVoucher]);

IF COL_LENGTH(N'dbo.hoadon', N'LyDoHuy') IS NULL
    ALTER TABLE dbo.[hoadon] ADD [LyDoHuy] NVARCHAR(500) NULL;

IF COL_LENGTH(N'dbo.hoadon', N'NguoiXuLy') IS NULL
    ALTER TABLE dbo.[hoadon] ADD [NguoiXuLy] NVARCHAR(100) NULL;

IF COL_LENGTH(N'dbo.hoadon', N'NgayCapNhat') IS NULL
    ALTER TABLE dbo.[hoadon] ADD [NgayCapNhat] DATETIME2 NULL;

IF COL_LENGTH(N'dbo.hoadon', N'PhuongThucThanhToan') IS NULL
    ALTER TABLE dbo.[hoadon] ADD [PhuongThucThanhToan] NVARCHAR(50) NOT NULL CONSTRAINT [DF_hoadon_PhuongThucThanhToan] DEFAULT (N'COD');

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON c.object_id = dc.parent_object_id
        AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.hoadon')
        AND c.name = N'TrangThai'
)
BEGIN
    ALTER TABLE dbo.hoadon
        ADD CONSTRAINT DF_HoaDon_TrangThai_Workflow DEFAULT (N'Chờ xác nhận') FOR TrangThai;
END;

UPDATE dbo.hoadon
SET TrangThai = N'Hoàn thành'
WHERE TrangThai IN (N'Hoàn tất', N'Đã thanh toán');
GO

-- 6. Vai tro va tai khoan
IF NOT EXISTS (SELECT 1 FROM dbo.[vaitro] WHERE [MaVaiTro] = 4)
BEGIN
    SET IDENTITY_INSERT dbo.[vaitro] ON;
    INSERT INTO dbo.[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (4, N'Admin', N'Quản trị hệ thống');
    SET IDENTITY_INSERT dbo.[vaitro] OFF;
END
ELSE
    UPDATE dbo.[vaitro] SET [TenVaiTro] = N'Admin' WHERE [MaVaiTro] = 4 AND [TenVaiTro] <> N'Admin';

IF NOT EXISTS (SELECT 1 FROM dbo.[vaitro] WHERE [MaVaiTro] = 3)
BEGIN
    SET IDENTITY_INSERT dbo.[vaitro] ON;
    INSERT INTO dbo.[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (3, N'NhanVien', N'Nhân viên vận hành');
    SET IDENTITY_INSERT dbo.[vaitro] OFF;
END;

-- Cap nhat tai khoan admin len vai tro 4 neu dang o vai tro 1
UPDATE dbo.[taikhoan] SET [MaVaiTro] = 4 WHERE [TenDangNhap] = 'admin' AND [MaVaiTro] = 1;

-- Du lieu ban dau nha cung cap & nhan vien neu chua co
IF NOT EXISTS (SELECT 1 FROM dbo.[nhacungcap])
BEGIN
    INSERT INTO dbo.[nhacungcap] ([TenNCC], [Sdt], [DiaChi], [TrangThai])
    VALUES (N'Công ty TNHH Cà phê Việt', N'02838123456', N'123 Lê Lợi, Q.1, TP.HCM', 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[nhanvien])
BEGIN
    INSERT INTO dbo.[nhanvien] ([HoVaDem], [Ten], [Sdt], [ChucVu])
    VALUES (N'Nguyễn', N'Văn Quản Lý', N'0901234567', N'Quản lý');
END;
GO

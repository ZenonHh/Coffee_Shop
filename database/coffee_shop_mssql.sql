-- =====================================================================
-- Coffee Shop - SQL Server (LocalDB) - day du 31 bang, chuyen tu coffee_shop_db.sql (MySQL)
-- CANH BAO: script xoa va tao lai 31 bang nay trong CoffeeShopDB.
-- Mat khau trong du lieu mau la hash bcrypt tu PHP ($2y$...), dung voi BCrypt.Net-Next.
-- =====================================================================
IF DB_ID(N'CoffeeShopDB') IS NULL CREATE DATABASE CoffeeShopDb;
GO
USE CoffeeShopDb;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Xoa bang cu (theo thu tu nguoc phu thuoc)
IF OBJECT_ID(N'dbo.cautraloi', 'U') IS NOT NULL DROP TABLE dbo.[cautraloi];
IF OBJECT_ID(N'dbo.danhmucluachon', 'U') IS NOT NULL DROP TABLE dbo.[danhmucluachon];
IF OBJECT_ID(N'dbo.cauhoikhaosat', 'U') IS NOT NULL DROP TABLE dbo.[cauhoikhaosat];
IF OBJECT_ID(N'dbo.nhatkyhoatdong', 'U') IS NOT NULL DROP TABLE dbo.[nhatkyhoatdong];
IF OBJECT_ID(N'dbo.loimoikhaosat', 'U') IS NOT NULL DROP TABLE dbo.[loimoikhaosat];
IF OBJECT_ID(N'dbo.khaosat', 'U') IS NOT NULL DROP TABLE dbo.[khaosat];
IF OBJECT_ID(N'dbo.chitiethoadon_topping', 'U') IS NOT NULL DROP TABLE dbo.[chitiethoadon_topping];
IF OBJECT_ID(N'dbo.chitiethoadon', 'U') IS NOT NULL DROP TABLE dbo.[chitiethoadon];
IF OBJECT_ID(N'dbo.chitietgiohang_topping', 'U') IS NOT NULL DROP TABLE dbo.[chitietgiohang_topping];
IF OBJECT_ID(N'dbo.chitietgiohang', 'U') IS NOT NULL DROP TABLE dbo.[chitietgiohang];
IF OBJECT_ID(N'dbo.thanhtoan', 'U') IS NOT NULL DROP TABLE dbo.[thanhtoan];
IF OBJECT_ID(N'dbo.taikhoan', 'U') IS NOT NULL DROP TABLE dbo.[taikhoan];
IF OBJECT_ID(N'dbo.phanhoi', 'U') IS NOT NULL DROP TABLE dbo.[phanhoi];
IF OBJECT_ID(N'dbo.lichsudiem', 'U') IS NOT NULL DROP TABLE dbo.[lichsudiem];
IF OBJECT_ID(N'dbo.hoadon', 'U') IS NOT NULL DROP TABLE dbo.[hoadon];
IF OBJECT_ID(N'dbo.giohang', 'U') IS NOT NULL DROP TABLE dbo.[giohang];
IF OBJECT_ID(N'dbo.ctkm_theo_sp', 'U') IS NOT NULL DROP TABLE dbo.[ctkm_theo_sp];
IF OBJECT_ID(N'dbo.chitietphieunhap', 'U') IS NOT NULL DROP TABLE dbo.[chitietphieunhap];
IF OBJECT_ID(N'dbo.calam', 'U') IS NOT NULL DROP TABLE dbo.[calam];
IF OBJECT_ID(N'dbo.bienthesanpham', 'U') IS NOT NULL DROP TABLE dbo.[bienthesanpham];
IF OBJECT_ID(N'dbo.voucher', 'U') IS NOT NULL DROP TABLE dbo.[voucher];
IF OBJECT_ID(N'dbo.vaitro', 'U') IS NOT NULL DROP TABLE dbo.[vaitro];
IF OBJECT_ID(N'dbo.sanpham', 'U') IS NOT NULL DROP TABLE dbo.[sanpham];
IF OBJECT_ID(N'dbo.phieunhap', 'U') IS NOT NULL DROP TABLE dbo.[phieunhap];
IF OBJECT_ID(N'dbo.nhanvien', 'U') IS NOT NULL DROP TABLE dbo.[nhanvien];
IF OBJECT_ID(N'dbo.nhacungcap', 'U') IS NOT NULL DROP TABLE dbo.[nhacungcap];
IF OBJECT_ID(N'dbo.nguyenlieu', 'U') IS NOT NULL DROP TABLE dbo.[nguyenlieu];
IF OBJECT_ID(N'dbo.loaisanpham', 'U') IS NOT NULL DROP TABLE dbo.[loaisanpham];
IF OBJECT_ID(N'dbo.khachhang', 'U') IS NOT NULL DROP TABLE dbo.[khachhang];
IF OBJECT_ID(N'dbo.ctkm_theo_hoadon', 'U') IS NOT NULL DROP TABLE dbo.[ctkm_theo_hoadon];
IF OBJECT_ID(N'dbo.chitietkm', 'U') IS NOT NULL DROP TABLE dbo.[chitietkm];
GO

-- 2. Tao bang
CREATE TABLE dbo.[chitietkm] (
    [MaCTKM] INT IDENTITY(1,1) NOT NULL,
    [TenCTKM] NVARCHAR(150) NOT NULL,
    [NgayBatDau] DATETIME2 NOT NULL,
    [NgayKetThuc] DATETIME2 NOT NULL,
    [MoTa] NVARCHAR(255) NULL,
    [TrangThai] BIT NULL DEFAULT 1,
    CONSTRAINT [PK_chitietkm] PRIMARY KEY ([MaCTKM])
);
GO
CREATE TABLE dbo.[ctkm_theo_hoadon] (
    [MaKMDon] INT IDENTITY(1,1) NOT NULL,
    [MaCTKM] INT NOT NULL,
    [DonToiThieu] DECIMAL(12,2) NOT NULL,
    [KieuGiam] NVARCHAR(10) NULL DEFAULT N'AMOUNT',
    [GiaTriGiam] DECIMAL(12,2) NOT NULL,
    [GiamToiDa] DECIMAL(12,2) NULL,
    CONSTRAINT [PK_ctkm_theo_hoadon] PRIMARY KEY ([MaKMDon])
);
GO
CREATE TABLE dbo.[khachhang] (
    [MaKH] INT IDENTITY(1,1) NOT NULL,
    [TaiKhoan] NVARCHAR(50) NULL,
    [MatKhau] NVARCHAR(255) NULL,
    [HoTen] NVARCHAR(100) NOT NULL,
    [Sdt] NVARCHAR(20) NOT NULL,
    [Email] NVARCHAR(100) NULL,
    [DiemTichLuy] INT NULL DEFAULT 0,
    [HangTV] NVARCHAR(20) NULL DEFAULT N'Đồng',
    [NgayTao] DATETIME2 NULL DEFAULT GETDATE(),
    [DiaChi] NVARCHAR(255) NULL,
    [TrangThai] BIT NOT NULL DEFAULT 1,
    [NgaySinh] DATE NULL,
    CONSTRAINT [PK_khachhang] PRIMARY KEY ([MaKH]),
    CONSTRAINT [UQ_khachhang_Sdt] UNIQUE ([Sdt])
);
GO
CREATE TABLE dbo.[loaisanpham] (
    [MaLoaiSP] INT IDENTITY(1,1) NOT NULL,
    [TenLoaiSP] NVARCHAR(100) NOT NULL,
    [ThuTuHienThi] INT NULL DEFAULT 1,
    CONSTRAINT [PK_loaisanpham] PRIMARY KEY ([MaLoaiSP])
);
GO
CREATE TABLE dbo.[nguyenlieu] (
    [MaNL] INT IDENTITY(1,1) NOT NULL,
    [TenNL] NVARCHAR(100) NOT NULL,
    [DonViTinh] NVARCHAR(20) NULL,
    [GiaVon] DECIMAL(12,2) NULL DEFAULT 0.00,
    CONSTRAINT [PK_nguyenlieu] PRIMARY KEY ([MaNL])
);
GO
CREATE TABLE dbo.[nhacungcap] (
    [MaNCC] INT IDENTITY(1,1) NOT NULL,
    [TenNCC] NVARCHAR(150) NOT NULL,
    [Sdt] NVARCHAR(20) NULL,
    [DiaChi] NVARCHAR(255) NULL,
    [TrangThai] BIT NULL DEFAULT 1,
    CONSTRAINT [PK_nhacungcap] PRIMARY KEY ([MaNCC])
);
GO
CREATE TABLE dbo.[nhanvien] (
    [MaNV] INT IDENTITY(1,1) NOT NULL,
    [HoVaDem] NVARCHAR(50) NOT NULL,
    [Ten] NVARCHAR(20) NOT NULL,
    [Sdt] NVARCHAR(20) NULL,
    [ChucVu] NVARCHAR(50) NULL,
    CONSTRAINT [PK_nhanvien] PRIMARY KEY ([MaNV])
);
GO
CREATE TABLE dbo.[phieunhap] (
    [MaPN] INT IDENTITY(1,1) NOT NULL,
    [MaNCC] INT NOT NULL,
    [MaNV] INT NOT NULL,
    [NgayNhap] DATETIME2 NULL DEFAULT GETDATE(),
    [TongTien] DECIMAL(12,2) NULL DEFAULT 0.00,
    [GhiChu] NVARCHAR(255) NULL,
    CONSTRAINT [PK_phieunhap] PRIMARY KEY ([MaPN])
);
GO
CREATE TABLE dbo.[sanpham] (
    [MaSP] INT IDENTITY(1,1) NOT NULL,
    [MaLoaiSP] INT NOT NULL,
    [TenSP] NVARCHAR(100) NOT NULL,
    [TrangThai] BIT NULL DEFAULT 1,
    [HinhAnh] NVARCHAR(255) NULL,
    CONSTRAINT [PK_sanpham] PRIMARY KEY ([MaSP])
);
GO
CREATE TABLE dbo.[vaitro] (
    [MaVaiTro] INT IDENTITY(1,1) NOT NULL,
    [TenVaiTro] NVARCHAR(50) NOT NULL,
    [MoTa] NVARCHAR(255) NULL,
    CONSTRAINT [PK_vaitro] PRIMARY KEY ([MaVaiTro])
);
GO
CREATE TABLE dbo.[voucher] (
    [MaVoucher] INT IDENTITY(1,1) NOT NULL,
    [MaCode] NVARCHAR(50) NOT NULL,
    [PhanLoaiGiam] NVARCHAR(20) NULL DEFAULT N'PERCENT',
    [GiaTriGiam] DECIMAL(12,2) NOT NULL,
    [TenChuongTrinh] NVARCHAR(150) NULL,
    [NgayBatDau] DATETIME2 NULL,
    [NgayKetThuc] DATETIME2 NULL,
    [DonToiThieu] DECIMAL(12,2) NOT NULL CONSTRAINT [DF_Voucher_DonToiThieu] DEFAULT (0),
    [TrangThai] BIT NULL DEFAULT 1,
    CONSTRAINT [PK_voucher] PRIMARY KEY ([MaVoucher]),
    CONSTRAINT [UQ_voucher_MaCode] UNIQUE ([MaCode])
);
GO
CREATE TABLE dbo.[bienthesanpham] (
    [MaBienThe] INT IDENTITY(1,1) NOT NULL,
    [MaSP] INT NOT NULL,
    [KichCo] NVARCHAR(10) NOT NULL,
    [GiaBan] DECIMAL(12,2) NOT NULL,
    [TrangThai] BIT NULL DEFAULT 1,
    CONSTRAINT [PK_bienthesanpham] PRIMARY KEY ([MaBienThe])
);
GO
CREATE TABLE dbo.[calam] (
    [MaCaLam] INT IDENTITY(1,1) NOT NULL,
    [MaNV] INT NOT NULL,
    [GioMoCa] DATETIME2 NULL DEFAULT GETDATE(),
    [GioDongCa] DATETIME2 NULL,
    [TienDauCa] DECIMAL(12,2) NULL DEFAULT 0.00,
    [TienCuoiCa] DECIMAL(12,2) NULL,
    CONSTRAINT [PK_calam] PRIMARY KEY ([MaCaLam])
);
GO
CREATE TABLE dbo.[chitietphieunhap] (
    [MaCTPN] INT IDENTITY(1,1) NOT NULL,
    [MaPN] INT NOT NULL,
    [MaNL] INT NOT NULL,
    [SoLuong] DECIMAL(10,2) NOT NULL,
    [DonGia] DECIMAL(12,2) NOT NULL,
    [ThanhTien] DECIMAL(12,2) NOT NULL,
    CONSTRAINT [PK_chitietphieunhap] PRIMARY KEY ([MaCTPN])
);
GO
CREATE TABLE dbo.[ctkm_theo_sp] (
    [MaKMSP] INT IDENTITY(1,1) NOT NULL,
    [MaCTKM] INT NOT NULL,
    [MaSP] INT NOT NULL,
    [KieuGiam] NVARCHAR(10) NULL DEFAULT N'PERCENT',
    [GiaTriGiam] DECIMAL(12,2) NOT NULL,
    CONSTRAINT [PK_ctkm_theo_sp] PRIMARY KEY ([MaKMSP])
);
GO
CREATE TABLE dbo.[giohang] (
    [MaGioHang] INT IDENTITY(1,1) NOT NULL,
    [MaKH] INT NOT NULL,
    [NgayTao] DATETIME2 NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_giohang] PRIMARY KEY ([MaGioHang])
);
GO
CREATE TABLE dbo.[CoffeeShopBrowserCart] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [CartJson] NVARCHAR(MAX) NOT NULL,
    [UpdatedAtUtc] DATETIME2 NOT NULL,
    CONSTRAINT [PK_CoffeeShopBrowserCart] PRIMARY KEY ([Id])
);
GO
CREATE TABLE dbo.[hoadon] (
    [MaHD] INT IDENTITY(1,1) NOT NULL,
    [MaCaLam] INT NULL,
    [MaKH] INT NULL,
    [MaNV] INT NULL,
    [MaCTKM] INT NULL,
    [LoaiDonHang] NVARCHAR(50) NULL DEFAULT N'Tại quán',
    [TongTien] DECIMAL(12,2) NULL DEFAULT 0.00,
    [TienGiamGia] DECIMAL(12,2) NULL DEFAULT 0.00,
    [TrangThai] NVARCHAR(50) NULL DEFAULT N'Đã thanh toán',
    [NgayTao] DATETIME2 NULL DEFAULT GETDATE(),
    [PhiGiao] INT NOT NULL DEFAULT 0,
    [HoTenNguoiNhan] NVARCHAR(100) NOT NULL DEFAULT N'',
    [SdtNguoiNhan] NVARCHAR(15) NOT NULL DEFAULT N'',
    [DiaChiGiao] NVARCHAR(255) NULL,
    [GhiChu] NVARCHAR(255) NULL,
    [TamTinh] INT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_hoadon] PRIMARY KEY ([MaHD])
);
GO
CREATE TABLE dbo.[lichsudiem] (
    [MaLSD] INT IDENTITY(1,1) NOT NULL,
    [MaHD] INT NULL,
    [MaKH] INT NOT NULL,
    [DiemThayDoi] INT NOT NULL,
    [LyDo] NVARCHAR(255) NULL,
    [NgayThucHien] DATETIME2 NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_lichsudiem] PRIMARY KEY ([MaLSD])
);
GO
CREATE TABLE dbo.[phanhoi] (
    [MaPhanHoi] INT IDENTITY(1,1) NOT NULL,
    [MaKH] INT NOT NULL,
    [MaSP] INT NULL,
    [NoiDung] NVARCHAR(MAX) NOT NULL,
    [DanhGia] INT NULL CHECK (DanhGia BETWEEN 1 AND 5),
    [NgayGui] DATETIME2 NULL DEFAULT GETDATE(),
    [TrangThaiXL] NVARCHAR(50) NULL DEFAULT N'Chờ xử lý',
    CONSTRAINT [PK_phanhoi] PRIMARY KEY ([MaPhanHoi])
);
GO
CREATE TABLE dbo.[taikhoan] (
    [MaTaiKhoan] INT IDENTITY(1,1) NOT NULL,
    [MaVaiTro] INT NOT NULL,
    [TenDangNhap] NVARCHAR(50) NOT NULL,
    [MatKhau] NVARCHAR(255) NOT NULL,
    [HoTen] NVARCHAR(100) NOT NULL,
    [TrangThai] BIT NULL DEFAULT 1,
    CONSTRAINT [PK_taikhoan] PRIMARY KEY ([MaTaiKhoan]),
    CONSTRAINT [UQ_taikhoan_TenDangNhap] UNIQUE ([TenDangNhap])
);
GO
CREATE TABLE dbo.[thanhtoan] (
    [MaThanhToan] INT IDENTITY(1,1) NOT NULL,
    [MaHD] INT NOT NULL,
    [PhuongThuc] NVARCHAR(50) NULL DEFAULT N'Tiền mặt',
    [SoTien] DECIMAL(12,2) NOT NULL,
    [MaGiaoDich] NVARCHAR(100) NULL,
    [ThoiGian] DATETIME2 NULL DEFAULT GETDATE(),
    [TrangThai] TINYINT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_thanhtoan] PRIMARY KEY ([MaThanhToan])
);
GO
CREATE TABLE dbo.[chitietgiohang] (
    [MaCTGH] INT IDENTITY(1,1) NOT NULL,
    [MaGioHang] INT NOT NULL,
    [MaBienThe] INT NOT NULL,
    [MaSP] INT NOT NULL,
    [SoLuong] INT NULL DEFAULT 1,
    [KhoaDong] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_chitietgiohang] PRIMARY KEY ([MaCTGH]),
    CONSTRAINT [UQ_chitietgiohang_uq_dong] UNIQUE ([MaGioHang], [KhoaDong])
);
GO
CREATE TABLE dbo.[chitietgiohang_topping] (
    [MaCTGH] INT NOT NULL,
    [MaSP] INT NOT NULL,
    CONSTRAINT [PK_chitietgiohang_topping] PRIMARY KEY ([MaCTGH], [MaSP])
);
GO
CREATE TABLE dbo.[chitiethoadon] (
    [MaCTHD] INT IDENTITY(1,1) NOT NULL,
    [MaHD] INT NOT NULL,
    [MaSP] INT NOT NULL,
    [MaBienThe] INT NOT NULL,
    [SoLuong] INT NOT NULL,
    [GiaBan] DECIMAL(12,2) NOT NULL,
    [ThanhTien] DECIMAL(12,2) NOT NULL,
    [TienTopping] INT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_chitiethoadon] PRIMARY KEY ([MaCTHD])
);
GO
CREATE TABLE dbo.[chitiethoadon_topping] (
    [MaCTHD] INT NOT NULL,
    [MaSP] INT NOT NULL,
    [GiaBan] INT NOT NULL,
    CONSTRAINT [PK_chitiethoadon_topping] PRIMARY KEY ([MaCTHD], [MaSP])
);
GO
CREATE TABLE dbo.[khaosat] (
    [MaKhaoSat] INT IDENTITY(1,1) NOT NULL,
    [MaTaiKhoan] INT NOT NULL,
    [TieuDe] NVARCHAR(255) NOT NULL,
    [MoTa] NVARCHAR(MAX) NULL,
    [NgayBatDau] DATETIME2 NOT NULL,
    [NgayKetThuc] DATETIME2 NOT NULL,
    CONSTRAINT [PK_khaosat] PRIMARY KEY ([MaKhaoSat])
);
GO
CREATE TABLE dbo.[loimoikhaosat] (
    [MaLoiMoi] INT IDENTITY(1,1) NOT NULL,
    [MaKhaoSat] INT NOT NULL,
    [MaKH] INT NOT NULL,
    [ThoiGianGui] DATETIME2 NULL DEFAULT GETDATE(),
    [TrangThai] NVARCHAR(50) NULL DEFAULT N'Chưa làm',
    CONSTRAINT [PK_loimoikhaosat] PRIMARY KEY ([MaLoiMoi])
);
GO
CREATE TABLE dbo.[nhatkyhoatdong] (
    [MaNhatKy] INT IDENTITY(1,1) NOT NULL,
    [MaTaiKhoan] INT NOT NULL,
    [HanhDong] NVARCHAR(255) NOT NULL,
    [ThoiGian] DATETIME2 NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_nhatkyhoatdong] PRIMARY KEY ([MaNhatKy])
);
GO
CREATE TABLE dbo.[cauhoikhaosat] (
    [MaCauHoi] INT IDENTITY(1,1) NOT NULL,
    [MaKhaoSat] INT NOT NULL,
    [NoiDungCauHoi] NVARCHAR(255) NOT NULL,
    [LoaiCauHoi] NVARCHAR(50) NULL DEFAULT N'TRAC_NGHIEM',
    CONSTRAINT [PK_cauhoikhaosat] PRIMARY KEY ([MaCauHoi])
);
GO
CREATE TABLE dbo.[danhmucluachon] (
    [MaDanhMuc] INT IDENTITY(1,1) NOT NULL,
    [MaCauHoi] INT NOT NULL,
    [NoiDungLuaChon] NVARCHAR(255) NOT NULL,
    [ThuTuHienThi] INT NULL DEFAULT 1,
    CONSTRAINT [PK_danhmucluachon] PRIMARY KEY ([MaDanhMuc])
);
GO
CREATE TABLE dbo.[cautraloi] (
    [MaCauTraLoi] INT IDENTITY(1,1) NOT NULL,
    [MaLoiMoi] INT NOT NULL,
    [MaCauHoi] INT NOT NULL,
    [MaDanhMuc] INT NULL,
    [NoiDungTraLoi] NVARCHAR(MAX) NULL,
    [NgayTraLoi] DATETIME2 NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_cautraloi] PRIMARY KEY ([MaCauTraLoi])
);
GO

-- 3. Chi muc duy nhat (cot cho phep NULL)
CREATE UNIQUE INDEX [UQ_khachhang_TaiKhoan] ON dbo.[khachhang] ([TaiKhoan]) WHERE [TaiKhoan] IS NOT NULL;
CREATE UNIQUE INDEX [UQ_khachhang_Email] ON dbo.[khachhang] ([Email]) WHERE [Email] IS NOT NULL;
GO

-- 4. Khoa ngoai
ALTER TABLE dbo.[ctkm_theo_hoadon] ADD CONSTRAINT [fk_ktkm_hoadon_ctkm] FOREIGN KEY ([MaCTKM]) REFERENCES dbo.[chitietkm] ([MaCTKM]) ON DELETE CASCADE;
ALTER TABLE dbo.[phieunhap] ADD CONSTRAINT [fk_phieunhap_ncc] FOREIGN KEY ([MaNCC]) REFERENCES dbo.[nhacungcap] ([MaNCC]);
ALTER TABLE dbo.[phieunhap] ADD CONSTRAINT [fk_phieunhap_nhanvien] FOREIGN KEY ([MaNV]) REFERENCES dbo.[nhanvien] ([MaNV]);
ALTER TABLE dbo.[sanpham] ADD CONSTRAINT [fk_sanpham_loaisp] FOREIGN KEY ([MaLoaiSP]) REFERENCES dbo.[loaisanpham] ([MaLoaiSP]);
ALTER TABLE dbo.[bienthesanpham] ADD CONSTRAINT [fk_bienthe_sanpham] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]) ON DELETE CASCADE;
ALTER TABLE dbo.[calam] ADD CONSTRAINT [fk_calam_nhanvien] FOREIGN KEY ([MaNV]) REFERENCES dbo.[nhanvien] ([MaNV]);
ALTER TABLE dbo.[chitietphieunhap] ADD CONSTRAINT [fk_ctpn_nguyenlieu] FOREIGN KEY ([MaNL]) REFERENCES dbo.[nguyenlieu] ([MaNL]);
ALTER TABLE dbo.[chitietphieunhap] ADD CONSTRAINT [fk_ctpn_phieunhap] FOREIGN KEY ([MaPN]) REFERENCES dbo.[phieunhap] ([MaPN]) ON DELETE CASCADE;
ALTER TABLE dbo.[ctkm_theo_sp] ADD CONSTRAINT [fk_ctkm_sp_ctkm] FOREIGN KEY ([MaCTKM]) REFERENCES dbo.[chitietkm] ([MaCTKM]) ON DELETE CASCADE;
ALTER TABLE dbo.[ctkm_theo_sp] ADD CONSTRAINT [fk_ctkm_sp_sanpham] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
ALTER TABLE dbo.[giohang] ADD CONSTRAINT [fk_giohang_khachhang] FOREIGN KEY ([MaKH]) REFERENCES dbo.[khachhang] ([MaKH]);
ALTER TABLE dbo.[hoadon] ADD CONSTRAINT [fk_hoadon_calam] FOREIGN KEY ([MaCaLam]) REFERENCES dbo.[calam] ([MaCaLam]);
ALTER TABLE dbo.[hoadon] ADD CONSTRAINT [fk_hoadon_ctkm] FOREIGN KEY ([MaCTKM]) REFERENCES dbo.[chitietkm] ([MaCTKM]);
ALTER TABLE dbo.[hoadon] ADD CONSTRAINT [fk_hoadon_khachhang] FOREIGN KEY ([MaKH]) REFERENCES dbo.[khachhang] ([MaKH]);
ALTER TABLE dbo.[hoadon] ADD CONSTRAINT [fk_hoadon_nhanvien] FOREIGN KEY ([MaNV]) REFERENCES dbo.[nhanvien] ([MaNV]);
ALTER TABLE dbo.[lichsudiem] ADD CONSTRAINT [fk_lsd_hoadon] FOREIGN KEY ([MaHD]) REFERENCES dbo.[hoadon] ([MaHD]);
ALTER TABLE dbo.[lichsudiem] ADD CONSTRAINT [fk_lsd_khachhang] FOREIGN KEY ([MaKH]) REFERENCES dbo.[khachhang] ([MaKH]);
ALTER TABLE dbo.[phanhoi] ADD CONSTRAINT [fk_phanhoi_khachhang] FOREIGN KEY ([MaKH]) REFERENCES dbo.[khachhang] ([MaKH]);
ALTER TABLE dbo.[phanhoi] ADD CONSTRAINT [fk_phanhoi_sanpham] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
ALTER TABLE dbo.[taikhoan] ADD CONSTRAINT [fk_taikhoan_vaitro] FOREIGN KEY ([MaVaiTro]) REFERENCES dbo.[vaitro] ([MaVaiTro]);
ALTER TABLE dbo.[thanhtoan] ADD CONSTRAINT [fk_thanhtoan_hoadon] FOREIGN KEY ([MaHD]) REFERENCES dbo.[hoadon] ([MaHD]);
ALTER TABLE dbo.[chitietgiohang] ADD CONSTRAINT [fk_ctgh_giohang] FOREIGN KEY ([MaGioHang]) REFERENCES dbo.[giohang] ([MaGioHang]) ON DELETE CASCADE;
ALTER TABLE dbo.[chitietgiohang] ADD CONSTRAINT [fk_ctgh_sanpham] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
ALTER TABLE dbo.[chitietgiohang_topping] ADD CONSTRAINT [chitietgiohang_topping_ibfk_1] FOREIGN KEY ([MaCTGH]) REFERENCES dbo.[chitietgiohang] ([MaCTGH]) ON DELETE CASCADE;
ALTER TABLE dbo.[chitietgiohang_topping] ADD CONSTRAINT [chitietgiohang_topping_ibfk_2] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
ALTER TABLE dbo.[chitiethoadon] ADD CONSTRAINT [fk_cthd_hoadon] FOREIGN KEY ([MaHD]) REFERENCES dbo.[hoadon] ([MaHD]) ON DELETE CASCADE;
ALTER TABLE dbo.[chitiethoadon] ADD CONSTRAINT [fk_cthd_sanpham] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
ALTER TABLE dbo.[chitiethoadon_topping] ADD CONSTRAINT [chitiethoadon_topping_ibfk_1] FOREIGN KEY ([MaCTHD]) REFERENCES dbo.[chitiethoadon] ([MaCTHD]) ON DELETE CASCADE;
ALTER TABLE dbo.[chitiethoadon_topping] ADD CONSTRAINT [chitiethoadon_topping_ibfk_2] FOREIGN KEY ([MaSP]) REFERENCES dbo.[sanpham] ([MaSP]);
ALTER TABLE dbo.[khaosat] ADD CONSTRAINT [fk_khaosat_taikhoan] FOREIGN KEY ([MaTaiKhoan]) REFERENCES dbo.[taikhoan] ([MaTaiKhoan]);
ALTER TABLE dbo.[loimoikhaosat] ADD CONSTRAINT [fk_loimoi_khachhang] FOREIGN KEY ([MaKH]) REFERENCES dbo.[khachhang] ([MaKH]);
ALTER TABLE dbo.[loimoikhaosat] ADD CONSTRAINT [fk_loimoi_khaosat] FOREIGN KEY ([MaKhaoSat]) REFERENCES dbo.[khaosat] ([MaKhaoSat]);
ALTER TABLE dbo.[nhatkyhoatdong] ADD CONSTRAINT [fk_nhatky_taikhoan] FOREIGN KEY ([MaTaiKhoan]) REFERENCES dbo.[taikhoan] ([MaTaiKhoan]);
ALTER TABLE dbo.[cauhoikhaosat] ADD CONSTRAINT [fk_cauhoi_khaosat] FOREIGN KEY ([MaKhaoSat]) REFERENCES dbo.[khaosat] ([MaKhaoSat]) ON DELETE CASCADE;
ALTER TABLE dbo.[danhmucluachon] ADD CONSTRAINT [fk_danhmuc_cauhoi] FOREIGN KEY ([MaCauHoi]) REFERENCES dbo.[cauhoikhaosat] ([MaCauHoi]) ON DELETE CASCADE;
ALTER TABLE dbo.[cautraloi] ADD CONSTRAINT [fk_cautraloi_cauhoi] FOREIGN KEY ([MaCauHoi]) REFERENCES dbo.[cauhoikhaosat] ([MaCauHoi]);
ALTER TABLE dbo.[cautraloi] ADD CONSTRAINT [fk_cautraloi_danhmuc] FOREIGN KEY ([MaDanhMuc]) REFERENCES dbo.[danhmucluachon] ([MaDanhMuc]);
ALTER TABLE dbo.[cautraloi] ADD CONSTRAINT [fk_cautraloi_loimoi] FOREIGN KEY ([MaLoiMoi]) REFERENCES dbo.[loimoikhaosat] ([MaLoiMoi]) ON DELETE CASCADE;
GO

-- 5. Du lieu
SET IDENTITY_INSERT dbo.[khachhang] ON;
INSERT INTO dbo.[khachhang] ([MaKH], [TaiKhoan], [MatKhau], [HoTen], [Sdt], [Email], [DiemTichLuy], [HangTV], [NgayTao], [DiaChi]) VALUES
(1, N'user01', N'$2y$10$x0gqtT./APiYudtODvg/IewGSv2PxjNDxDWQVq8GuMcz7mGeL8Yxy', N'nguyen van a', N'0909090909', N'', 0, N'Đồng', N'2026-10-06 14:44:33', NULL),
(2, N'user02', N'$2y$10$EntbLUusT30vc/LeUP.3RegMY4E8djOnX6ojiS9ehBwkGDMxiD88.', N'Nguyen van b', N'0909090909222', N'user02@gmail.com', 0, N'Đồng', N'2026-10-13 13:35:17', NULL);
SET IDENTITY_INSERT dbo.[khachhang] OFF;
GO
SET IDENTITY_INSERT dbo.[loaisanpham] ON;
INSERT INTO dbo.[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES
(1, N'Cà Phê Espresso', 10),
(2, N'Cà Phê Phin Mê', 20),
(3, N'Cold Brew', 30),
(4, N'Matcha', 40),
(5, N'Trà Sữa', 50),
(6, N'Trà Trái Cây', 60),
(7, N'Fruit Boost', 70),
(8, N'Bánh Ngọt', 80),
(9, N'Topping', 90);
SET IDENTITY_INSERT dbo.[loaisanpham] OFF;
GO
SET IDENTITY_INSERT dbo.[sanpham] ON;
INSERT INTO dbo.[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh]) VALUES
(1, 2, N'Phin Mê Đen', 1, N'me-den.png'),
(2, 2, N'Phin Mê Sữa', 1, N'me-sua.png'),
(3, 2, N'Phin Mê Xỉu', 1, N'me-xiu.png'),
(4, 2, N'Phin Mê Dừa Non', 1, N'me-dua-non.png'),
(5, 1, N'Americano', 1, N'americano.png'),
(6, 1, N'Espresso Đen Đá', 1, N'espresso-den-da.png'),
(7, 1, N'Espresso Sữa Đá', 1, N'espresso-sua-da.png'),
(8, 1, N'Espresso Bạc Xỉu', 1, N'espresso-bac-xiu.png'),
(9, 1, N'Latte Nguyên Bản', 1, N'latte-nguyen-ban.png'),
(10, 1, N'Cà Phê Kem Mây Chanh Vàng', 1, N'cafe-kem-may-chanh-vang.png'),
(11, 3, N'Cold Brew Chanh', 1, N'cold-brew-chanh.png'),
(12, 3, N'Cold Brew Quýt', 1, N'cold-brew-quyt.png'),
(13, 3, N'Cold Brew Tằm', 1, N'cold-brew-tam.png'),
(14, 4, N'Matcha Latte', 1, N'matcha-latte.png'),
(15, 4, N'Matcha Dừa Xiêm', 1, N'matcha-dua-xiem.png'),
(16, 4, N'Matcha Bắp Bắp', 1, N'matcha-bap-bi.png'),
(17, 4, N'Matcha Tofu', 1, N'matcha-tofu.png'),
(18, 5, N'Trà Sữa Oolong Nướng', 1, N'tra-sua-oolong-nuong.png'),
(19, 5, N'Trà Sữa Oolong Ba Lá', 1, N'tra-sua-oolong-ba-la.png'),
(20, 5, N'Trà Sữa Chôm Chôm', 1, N'tra-sua-chom-chom.png'),
(21, 5, N'Trà Sữa Thanh Hương', 1, N'tra-sua-thanh-huong.png'),
(22, 5, N'Nhài Sữa Dẻ Cười', 1, N'nhai-sua-de-cuoi.png'),
(23, 6, N'Trà Đào Hồng Đài', 1, N'tra-dao-hong-dai.png'),
(24, 6, N'Trà Quýt', 1, N'tra-quyt.png'),
(25, 6, N'Cóc Cóc Đác Đác', 1, N'coc-coc-dac-dac.png'),
(26, 6, N'Lệ Chi Thanh Trà', 1, N'le-chi-thanh-tra.png'),
(27, 7, N'Socola', 1, N'so-co-la.png'),
(28, 7, N'Sữa Chua Tằm', 1, N'sua-chua-tam.png'),
(29, 8, N'Bánh Chuối', 1, N'banh-chuoi.png'),
(30, 8, N'Bánh Tráng Muối Tỏi', 1, N'banh-trang-muoi-toi.png'),
(31, 8, N'Bánh Tráng Tôm Hành', 1, N'banh-trang-tom-hanh.png'),
(32, 8, N'Bông Lan Trứng Muối', 1, N'bong-lan-trung-muoi.png'),
(33, 8, N'Bông Lan Trứng Muối Dầu Trứng', 1, N'bong-lan-trung-muoi-dau-trung.png'),
(34, 8, N'Crème Flan', 1, N'creme-plan.png'),
(35, 8, N'Bánh Flan', 1, N'plan.png'),
(36, 8, N'Bánh Phô Mai Chanh Dây', 1, N'pho-mai-chanh-day.png'),
(37, 8, N'Bánh Phô Mai Tan Chảy', 1, N'pho-mai-tan-chay.png'),
(38, 8, N'Bánh Phô Mai Tom & Jerry', 1, N'tom-and-jerry.png'),
(39, 9, N'Huyền Châu', 1, N'huyen-chau.png'),
(40, 9, N'Phô Mai Dẻo', 1, N'pho-mai-deo.png'),
(41, 9, N'Tofu', 1, N'tofu.png'),
(42, 9, N'Thạch Hồng Đài', 1, N'thach-hong-dai.png'),
(43, 9, N'Thốt Nốt', 1, N'thot-not.png'),
(44, 9, N'Trân Châu Dừa', 1, N'tran-chau-dua.png'),
(45, 9, N'Trân Châu Matcha', 1, N'tran-chau-matcha.png'),
(46, 9, N'Trân Châu Trắng', 1, N'tranchau-trang.png'),
(47, 7, N'Bơ Già Dừa Non', 1, N'bo-dua-non.png'),
(48, 7, N' Bơ Sáp Khoai Môn Macchiato', 1, N'bo-khoaimon.png');
SET IDENTITY_INSERT dbo.[sanpham] OFF;
GO
SET IDENTITY_INSERT dbo.[vaitro] ON;
INSERT INTO dbo.[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES
(4, N'Admin', N'Quản trị hệ thống');
SET IDENTITY_INSERT dbo.[vaitro] OFF;
GO
SET IDENTITY_INSERT dbo.[bienthesanpham] ON;
INSERT INTO dbo.[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES
(1, 1, N'M', 29000.00, 1),
(2, 1, N'L', 35000.00, 1),
(3, 2, N'M', 32000.00, 1),
(4, 2, N'L', 39000.00, 1),
(5, 3, N'M', 32000.00, 1),
(6, 3, N'L', 39000.00, 1),
(7, 4, N'M', 39000.00, 1),
(8, 4, N'L', 45000.00, 1),
(9, 5, N'M', 35000.00, 1),
(10, 5, N'L', 42000.00, 1),
(11, 6, N'M', 32000.00, 1),
(12, 6, N'L', 39000.00, 1),
(13, 7, N'M', 35000.00, 1),
(14, 7, N'L', 42000.00, 1),
(15, 8, N'M', 38000.00, 1),
(16, 8, N'L', 45000.00, 1),
(17, 9, N'M', 45000.00, 1),
(18, 9, N'L', 52000.00, 1),
(19, 10, N'M', 49000.00, 1),
(20, 10, N'L', 56000.00, 1),
(21, 11, N'M', 45000.00, 1),
(22, 11, N'L', 52000.00, 1),
(23, 12, N'M', 45000.00, 1),
(24, 12, N'L', 52000.00, 1),
(25, 13, N'M', 49000.00, 1),
(26, 13, N'L', 56000.00, 1),
(27, 14, N'M', 45000.00, 1),
(28, 14, N'L', 55000.00, 1),
(29, 15, N'M', 49000.00, 1),
(30, 15, N'L', 59000.00, 1),
(31, 16, N'M', 49000.00, 1),
(32, 16, N'L', 59000.00, 1),
(33, 17, N'M', 52000.00, 1),
(34, 17, N'L', 62000.00, 1),
(35, 18, N'M', 39000.00, 1),
(36, 18, N'L', 49000.00, 1),
(37, 19, N'M', 39000.00, 1),
(38, 19, N'L', 49000.00, 1),
(39, 20, N'M', 45000.00, 1),
(40, 20, N'L', 55000.00, 1),
(41, 21, N'M', 42000.00, 1),
(42, 21, N'L', 52000.00, 1),
(43, 22, N'M', 45000.00, 1),
(44, 22, N'L', 55000.00, 1),
(45, 23, N'M', 39000.00, 1),
(46, 23, N'L', 49000.00, 1),
(47, 24, N'M', 39000.00, 1),
(48, 24, N'L', 49000.00, 1),
(49, 25, N'M', 45000.00, 1),
(50, 25, N'L', 55000.00, 1),
(51, 26, N'M', 42000.00, 1),
(52, 26, N'L', 52000.00, 1),
(53, 27, N'M', 42000.00, 1),
(54, 27, N'L', 52000.00, 1),
(55, 28, N'M', 45000.00, 1),
(56, 28, N'L', 55000.00, 1),
(57, 29, N'Mặc định', 25000.00, 1),
(58, 30, N'Mặc định', 22000.00, 1),
(59, 31, N'Mặc định', 25000.00, 1),
(60, 32, N'Mặc định', 35000.00, 1),
(61, 33, N'Mặc định', 38000.00, 1),
(62, 34, N'Mặc định', 29000.00, 1),
(63, 35, N'Mặc định', 25000.00, 1),
(64, 36, N'Mặc định', 39000.00, 1),
(65, 37, N'Mặc định', 42000.00, 1),
(66, 38, N'Mặc định', 45000.00, 1),
(67, 39, N'Mặc định', 10000.00, 1),
(68, 40, N'Mặc định', 12000.00, 1),
(69, 41, N'Mặc định', 12000.00, 1),
(70, 42, N'Mặc định', 10000.00, 1),
(71, 43, N'Mặc định', 12000.00, 1),
(72, 44, N'Mặc định', 10000.00, 1),
(73, 45, N'Mặc định', 10000.00, 1),
(74, 46, N'Mặc định', 10000.00, 1),
(75, 47, N'M', 47000.00, 1),
(76, 47, N'L', 69000.00, 1),
(77, 48, N'M', 45000.00, 1),
(78, 1, N'L', 65000.00, 1);
SET IDENTITY_INSERT dbo.[bienthesanpham] OFF;
GO
SET IDENTITY_INSERT dbo.[giohang] ON;
INSERT INTO dbo.[giohang] ([MaGioHang], [MaKH], [NgayTao]) VALUES
(5, 1, N'2026-10-10 22:27:01');
SET IDENTITY_INSERT dbo.[giohang] OFF;
GO
SET IDENTITY_INSERT dbo.[hoadon] ON;
INSERT INTO dbo.[hoadon] ([MaHD], [MaCaLam], [MaKH], [MaNV], [MaCTKM], [LoaiDonHang], [TongTien], [TienGiamGia], [TrangThai], [NgayTao], [PhiGiao], [HoTenNguoiNhan], [SdtNguoiNhan], [DiaChiGiao], [GhiChu], [TamTinh]) VALUES
(2, NULL, 1, NULL, NULL, N'1', 105000.00, 0.00, N'Hoàn tất', N'2026-10-12 09:26:09', 0, N'nguyen van a', N'0909090909', N'120 yen lang thanh pho ha noi', NULL, 105000);
SET IDENTITY_INSERT dbo.[hoadon] OFF;
GO
SET IDENTITY_INSERT dbo.[taikhoan] ON;
INSERT INTO dbo.[taikhoan] ([MaTaiKhoan], [MaVaiTro], [TenDangNhap], [MatKhau], [HoTen], [TrangThai]) VALUES
(4, 4, N'admin', N'$2y$10$k23UXslDcs947rkCallV5OIgKO6A2HZCBtD9WUev7buorq8oVtz1i', N'Quản trị viên', 1);
SET IDENTITY_INSERT dbo.[taikhoan] OFF;
GO
SET IDENTITY_INSERT dbo.[thanhtoan] ON;
INSERT INTO dbo.[thanhtoan] ([MaThanhToan], [MaHD], [PhuongThuc], [SoTien], [MaGiaoDich], [ThoiGian], [TrangThai]) VALUES
(1, 2, N'COD', 105000.00, NULL, N'2026-10-12 09:26:09', 0);
SET IDENTITY_INSERT dbo.[thanhtoan] OFF;
GO
SET IDENTITY_INSERT dbo.[chitietgiohang] ON;
INSERT INTO dbo.[chitietgiohang] ([MaCTGH], [MaGioHang], [MaBienThe], [MaSP], [SoLuong], [KhoaDong]) VALUES
(3, 5, 13, 7, 1, N'13:42');
SET IDENTITY_INSERT dbo.[chitietgiohang] OFF;
GO
INSERT INTO dbo.[chitietgiohang_topping] ([MaCTGH], [MaSP]) VALUES
(3, 42);
GO
SET IDENTITY_INSERT dbo.[chitiethoadon] ON;
INSERT INTO dbo.[chitiethoadon] ([MaCTHD], [MaHD], [MaSP], [MaBienThe], [SoLuong], [GiaBan], [ThanhTien], [TienTopping]) VALUES
(1, 2, 7, 13, 3, 35000.00, 105000.00, 0);
SET IDENTITY_INSERT dbo.[chitiethoadon] OFF;
GO

-- 6. Email rong luu thanh NULL (de nhieu khach khong co email khong bi trung)
UPDATE dbo.[khachhang] SET [Email] = NULL WHERE [Email] = N'';
GO
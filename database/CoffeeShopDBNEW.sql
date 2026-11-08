-- =====================================================================
-- Coffee Shop DB - MSSQL Setup Script (All-in-One Database)
-- Tu dong tao CSDL CoffeeShopDb, xoa bang cu (neu co) va tao lai toan bo
-- 33 bang, du lieu mau, rang buoc va chi muc hoan chinh.
-- Hoat dong tren moi may (LocalDB, SQL Server, SQLEXPRESS) ma khong bi loi duong dan.
-- =====================================================================
USE [master];
GO

IF DB_ID(N'CoffeeShopDb') IS NULL
BEGIN
    CREATE DATABASE [CoffeeShopDb];
END;
GO

USE [CoffeeShopDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Xoa khoa ngoai cu va xoa bang cu de dam bao chay lai duoc nhieu lan (idempotent)
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql += N'DROP TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N';' + CHAR(13)
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
GO

/****** Object:  Table [dbo].[bienthesanpham]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[bienthesanpham](
	[MaBienThe] [int] IDENTITY(1,1) NOT NULL,
	[MaSP] [int] NOT NULL,
	[KichCo] [nvarchar](10) NOT NULL,
	[GiaBan] [decimal](12, 2) NOT NULL,
	[TrangThai] [bit] NULL,
 CONSTRAINT [PK_bienthesanpham] PRIMARY KEY CLUSTERED 
(
	[MaBienThe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[calam]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[calam](
	[MaCaLam] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [int] NOT NULL,
	[GioMoCa] [datetime2](7) NULL,
	[GioDongCa] [datetime2](7) NULL,
	[TienDauCa] [decimal](12, 2) NULL,
	[TienCuoiCa] [decimal](12, 2) NULL,
 CONSTRAINT [PK_calam] PRIMARY KEY CLUSTERED 
(
	[MaCaLam] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cauhoikhaosat]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cauhoikhaosat](
	[MaCauHoi] [int] IDENTITY(1,1) NOT NULL,
	[MaKhaoSat] [int] NOT NULL,
	[NoiDungCauHoi] [nvarchar](255) NOT NULL,
	[LoaiCauHoi] [nvarchar](50) NULL,
 CONSTRAINT [PK_cauhoikhaosat] PRIMARY KEY CLUSTERED 
(
	[MaCauHoi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cautraloi]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cautraloi](
	[MaCauTraLoi] [int] IDENTITY(1,1) NOT NULL,
	[MaLoiMoi] [int] NULL,
	[MaCauHoi] [int] NOT NULL,
	[MaDanhMuc] [int] NULL,
	[NoiDungTraLoi] [nvarchar](max) NULL,
	[NgayTraLoi] [datetime2](7) NULL,
 CONSTRAINT [PK_cautraloi] PRIMARY KEY CLUSTERED 
(
	[MaCauTraLoi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chitietgiohang]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chitietgiohang](
	[MaCTGH] [int] IDENTITY(1,1) NOT NULL,
	[MaGioHang] [int] NOT NULL,
	[MaBienThe] [int] NOT NULL,
	[MaSP] [int] NOT NULL,
	[SoLuong] [int] NULL,
	[KhoaDong] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_chitietgiohang] PRIMARY KEY CLUSTERED 
(
	[MaCTGH] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chitietgiohang_topping]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chitietgiohang_topping](
	[MaCTGH] [int] NOT NULL,
	[MaSP] [int] NOT NULL,
 CONSTRAINT [PK_chitietgiohang_topping] PRIMARY KEY CLUSTERED 
(
	[MaCTGH] ASC,
	[MaSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chitiethoadon]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chitiethoadon](
	[MaCTHD] [int] IDENTITY(1,1) NOT NULL,
	[MaHD] [int] NOT NULL,
	[MaSP] [int] NOT NULL,
	[MaBienThe] [int] NOT NULL,
	[SoLuong] [int] NOT NULL,
	[GiaBan] [decimal](12, 2) NOT NULL,
	[ThanhTien] [decimal](12, 2) NOT NULL,
	[TienTopping] [int] NOT NULL,
 CONSTRAINT [PK_chitiethoadon] PRIMARY KEY CLUSTERED 
(
	[MaCTHD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chitiethoadon_topping]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chitiethoadon_topping](
	[MaCTHD] [int] NOT NULL,
	[MaSP] [int] NOT NULL,
	[GiaBan] [int] NOT NULL,
 CONSTRAINT [PK_chitiethoadon_topping] PRIMARY KEY CLUSTERED 
(
	[MaCTHD] ASC,
	[MaSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chitietkm]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chitietkm](
	[MaCTKM] [int] IDENTITY(1,1) NOT NULL,
	[TenCTKM] [nvarchar](150) NOT NULL,
	[NgayBatDau] [datetime2](7) NOT NULL,
	[NgayKetThuc] [datetime2](7) NOT NULL,
	[MoTa] [nvarchar](255) NULL,
	[TrangThai] [bit] NULL,
 CONSTRAINT [PK_chitietkm] PRIMARY KEY CLUSTERED 
(
	[MaCTKM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chitietphieunhap]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chitietphieunhap](
	[MaCTPN] [int] IDENTITY(1,1) NOT NULL,
	[MaPN] [int] NOT NULL,
	[MaNL] [int] NULL,
	[SoLuong] [decimal](10, 2) NOT NULL,
	[DonGia] [decimal](12, 2) NOT NULL,
	[ThanhTien] [decimal](12, 2) NOT NULL,
	[MaSP] [int] NULL,
 CONSTRAINT [PK_chitietphieunhap] PRIMARY KEY CLUSTERED 
(
	[MaCTPN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CoffeeShopBrowserCart]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoffeeShopBrowserCart](
	[Id] [uniqueidentifier] NOT NULL,
	[CartJson] [nvarchar](max) NOT NULL,
	[UpdatedAtUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CoffeeShopBrowserCart] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ctkm_theo_hoadon]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ctkm_theo_hoadon](
	[MaKMDon] [int] IDENTITY(1,1) NOT NULL,
	[MaCTKM] [int] NOT NULL,
	[DonToiThieu] [decimal](12, 2) NOT NULL,
	[KieuGiam] [nvarchar](10) NULL,
	[GiaTriGiam] [decimal](12, 2) NOT NULL,
	[GiamToiDa] [decimal](12, 2) NULL,
 CONSTRAINT [PK_ctkm_theo_hoadon] PRIMARY KEY CLUSTERED 
(
	[MaKMDon] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ctkm_theo_sp]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ctkm_theo_sp](
	[MaKMSP] [int] IDENTITY(1,1) NOT NULL,
	[MaCTKM] [int] NOT NULL,
	[MaSP] [int] NOT NULL,
	[KieuGiam] [nvarchar](10) NULL,
	[GiaTriGiam] [decimal](12, 2) NOT NULL,
 CONSTRAINT [PK_ctkm_theo_sp] PRIMARY KEY CLUSTERED 
(
	[MaKMSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[danhmucluachon]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[danhmucluachon](
	[MaDanhMuc] [int] IDENTITY(1,1) NOT NULL,
	[MaCauHoi] [int] NOT NULL,
	[NoiDungLuaChon] [nvarchar](255) NOT NULL,
	[ThuTuHienThi] [int] NULL,
 CONSTRAINT [PK_danhmucluachon] PRIMARY KEY CLUSTERED 
(
	[MaDanhMuc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[giohang]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[giohang](
	[MaGioHang] [int] IDENTITY(1,1) NOT NULL,
	[MaKH] [int] NOT NULL,
	[NgayTao] [datetime2](7) NULL,
 CONSTRAINT [PK_giohang] PRIMARY KEY CLUSTERED 
(
	[MaGioHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[hoadon]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hoadon](
	[MaHD] [int] IDENTITY(1,1) NOT NULL,
	[MaCaLam] [int] NULL,
	[MaKH] [int] NULL,
	[MaNV] [int] NULL,
	[MaCTKM] [int] NULL,
	[LoaiDonHang] [nvarchar](50) NULL,
	[TongTien] [decimal](12, 2) NULL,
	[TienGiamGia] [decimal](12, 2) NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
	[NgayTao] [datetime2](7) NULL,
	[PhiGiao] [int] NOT NULL,
	[HoTenNguoiNhan] [nvarchar](100) NOT NULL,
	[SdtNguoiNhan] [nvarchar](15) NOT NULL,
	[DiaChiGiao] [nvarchar](255) NULL,
	[GhiChu] [nvarchar](255) NULL,
	[TamTinh] [int] NOT NULL,
	[MaVoucher] [int] NULL,
	[LyDoHuy] [nvarchar](500) NULL,
	[NguoiXuLy] [nvarchar](100) NULL,
	[NgayCapNhat] [datetime2](7) NULL,
	[PhuongThucThanhToan] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_hoadon] PRIMARY KEY CLUSTERED 
(
	[MaHD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[khachhang]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[khachhang](
	[MaKH] [int] IDENTITY(1,1) NOT NULL,
	[TaiKhoan] [nvarchar](50) NULL,
	[MatKhau] [nvarchar](255) NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[Sdt] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[DiemTichLuy] [int] NULL,
	[HangTV] [nvarchar](20) NULL,
	[NgayTao] [datetime2](7) NULL,
	[DiaChi] [nvarchar](255) NULL,
	[TrangThai] [bit] NOT NULL,
	[NgaySinh] [date] NULL,
 CONSTRAINT [PK_khachhang] PRIMARY KEY CLUSTERED 
(
	[MaKH] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[khaosat]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[khaosat](
	[MaKhaoSat] [int] IDENTITY(1,1) NOT NULL,
	[MaTaiKhoan] [int] NOT NULL,
	[TieuDe] [nvarchar](255) NOT NULL,
	[MoTa] [nvarchar](max) NULL,
	[NgayBatDau] [datetime2](7) NOT NULL,
	[NgayKetThuc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_khaosat] PRIMARY KEY CLUSTERED 
(
	[MaKhaoSat] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lichsudiem]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lichsudiem](
	[MaLSD] [int] IDENTITY(1,1) NOT NULL,
	[MaHD] [int] NULL,
	[MaKH] [int] NOT NULL,
	[DiemThayDoi] [int] NOT NULL,
	[LyDo] [nvarchar](255) NULL,
	[NgayThucHien] [datetime2](7) NULL,
 CONSTRAINT [PK_lichsudiem] PRIMARY KEY CLUSTERED 
(
	[MaLSD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LienHe]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LienHe](
	[MaLienHe] [int] IDENTITY(1,1) NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](150) NOT NULL,
	[ChuDe] [nvarchar](255) NULL,
	[NoiDung] [nvarchar](max) NOT NULL,
	[NgayGui] [datetime2](7) NOT NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
	[GhiChu] [nvarchar](500) NULL,
 CONSTRAINT [PK_LienHe] PRIMARY KEY CLUSTERED 
(
	[MaLienHe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[loaisanpham]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[loaisanpham](
	[MaLoaiSP] [int] IDENTITY(1,1) NOT NULL,
	[TenLoaiSP] [nvarchar](100) NOT NULL,
	[ThuTuHienThi] [int] NULL,
 CONSTRAINT [PK_loaisanpham] PRIMARY KEY CLUSTERED 
(
	[MaLoaiSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[loimoikhaosat]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[loimoikhaosat](
	[MaLoiMoi] [int] IDENTITY(1,1) NOT NULL,
	[MaKhaoSat] [int] NOT NULL,
	[MaKH] [int] NOT NULL,
	[ThoiGianGui] [datetime2](7) NULL,
	[TrangThai] [nvarchar](50) NULL,
 CONSTRAINT [PK_loimoikhaosat] PRIMARY KEY CLUSTERED 
(
	[MaLoiMoi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[nguyenlieu]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nguyenlieu](
	[MaNL] [int] IDENTITY(1,1) NOT NULL,
	[TenNL] [nvarchar](100) NOT NULL,
	[DonViTinh] [nvarchar](20) NULL,
	[GiaVon] [decimal](12, 2) NULL,
 CONSTRAINT [PK_nguyenlieu] PRIMARY KEY CLUSTERED 
(
	[MaNL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[nhacungcap]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nhacungcap](
	[MaNCC] [int] IDENTITY(1,1) NOT NULL,
	[TenNCC] [nvarchar](150) NOT NULL,
	[Sdt] [nvarchar](20) NULL,
	[DiaChi] [nvarchar](255) NULL,
	[TrangThai] [bit] NULL,
 CONSTRAINT [PK_nhacungcap] PRIMARY KEY CLUSTERED 
(
	[MaNCC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[nhanvien]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nhanvien](
	[MaNV] [int] IDENTITY(1,1) NOT NULL,
	[HoVaDem] [nvarchar](50) NOT NULL,
	[Ten] [nvarchar](20) NOT NULL,
	[Sdt] [nvarchar](20) NULL,
	[ChucVu] [nvarchar](50) NULL,
 CONSTRAINT [PK_nhanvien] PRIMARY KEY CLUSTERED 
(
	[MaNV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[nhatkyhoatdong]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nhatkyhoatdong](
	[MaNhatKy] [int] IDENTITY(1,1) NOT NULL,
	[MaTaiKhoan] [int] NOT NULL,
	[HanhDong] [nvarchar](255) NOT NULL,
	[ThoiGian] [datetime2](7) NULL,
 CONSTRAINT [PK_nhatkyhoatdong] PRIMARY KEY CLUSTERED 
(
	[MaNhatKy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[phanhoi]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[phanhoi](
	[MaPhanHoi] [int] IDENTITY(1,1) NOT NULL,
	[MaKH] [int] NOT NULL,
	[MaSP] [int] NULL,
	[NoiDung] [nvarchar](max) NOT NULL,
	[DanhGia] [int] NULL,
	[NgayGui] [datetime2](7) NULL,
	[TrangThaiXL] [nvarchar](50) NULL,
 CONSTRAINT [PK_phanhoi] PRIMARY KEY CLUSTERED 
(
	[MaPhanHoi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[phieunhap]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[phieunhap](
	[MaPN] [int] IDENTITY(1,1) NOT NULL,
	[MaNCC] [int] NOT NULL,
	[MaNV] [int] NOT NULL,
	[NgayNhap] [datetime2](7) NULL,
	[TongTien] [decimal](12, 2) NULL,
	[GhiChu] [nvarchar](255) NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
	[NguoiLap] [nvarchar](100) NOT NULL,
	[NguoiDuyetId] [int] NULL,
	[NguoiDuyet] [nvarchar](100) NULL,
	[NgayDuyet] [datetime2](7) NULL,
 CONSTRAINT [PK_phieunhap] PRIMARY KEY CLUSTERED 
(
	[MaPN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[sanpham]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[sanpham](
	[MaSP] [int] IDENTITY(1,1) NOT NULL,
	[MaLoaiSP] [int] NOT NULL,
	[TenSP] [nvarchar](100) NOT NULL,
	[TrangThai] [bit] NULL,
	[HinhAnh] [nvarchar](255) NULL,
	[SoLuongTon] [int] NOT NULL,
 CONSTRAINT [PK_sanpham] PRIMARY KEY CLUSTERED 
(
	[MaSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[taikhoan]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[taikhoan](
	[MaTaiKhoan] [int] IDENTITY(1,1) NOT NULL,
	[MaVaiTro] [int] NOT NULL,
	[TenDangNhap] [nvarchar](50) NOT NULL,
	[MatKhau] [nvarchar](255) NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[TrangThai] [bit] NULL,
 CONSTRAINT [PK_taikhoan] PRIMARY KEY CLUSTERED 
(
	[MaTaiKhoan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[thanhtoan]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[thanhtoan](
	[MaThanhToan] [int] IDENTITY(1,1) NOT NULL,
	[MaHD] [int] NOT NULL,
	[PhuongThuc] [nvarchar](50) NULL,
	[SoTien] [decimal](12, 2) NOT NULL,
	[MaGiaoDich] [nvarchar](100) NULL,
	[ThoiGian] [datetime2](7) NULL,
	[TrangThai] [tinyint] NOT NULL,
 CONSTRAINT [PK_thanhtoan] PRIMARY KEY CLUSTERED 
(
	[MaThanhToan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[vaitro]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[vaitro](
	[MaVaiTro] [int] IDENTITY(1,1) NOT NULL,
	[TenVaiTro] [nvarchar](50) NOT NULL,
	[MoTa] [nvarchar](255) NULL,
 CONSTRAINT [PK_vaitro] PRIMARY KEY CLUSTERED 
(
	[MaVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[voucher]    Script Date: 28/10/2026 2:36:00 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[voucher](
	[MaVoucher] [int] IDENTITY(1,1) NOT NULL,
	[MaCode] [nvarchar](50) NOT NULL,
	[PhanLoaiGiam] [nvarchar](20) NULL,
	[GiaTriGiam] [decimal](12, 2) NOT NULL,
	[TrangThai] [bit] NULL,
	[DonToiThieu] [decimal](12, 2) NOT NULL,
	[TenChuongTrinh] [nvarchar](150) NULL,
	[GiamToiDa] [decimal](12, 2) NULL,
	[SoLuongToiDa] [int] NOT NULL,
	[SoLuongDaDung] [int] NOT NULL,
	[NgayBatDau] [datetime2](7) NULL,
	[NgayKetThuc] [datetime2](7) NULL,
 CONSTRAINT [PK_voucher] PRIMARY KEY CLUSTERED 
(
	[MaVoucher] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[bienthesanpham] ON 

INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (1, 1, N'M', CAST(29000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (2, 1, N'L', CAST(35000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (3, 2, N'M', CAST(32000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (4, 2, N'L', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (5, 3, N'M', CAST(32000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (6, 3, N'L', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (7, 4, N'M', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (8, 4, N'L', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (9, 5, N'M', CAST(35000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (10, 5, N'L', CAST(42000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (11, 6, N'M', CAST(32000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (12, 6, N'L', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (13, 7, N'M', CAST(35000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (14, 7, N'L', CAST(42000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (15, 8, N'M', CAST(38000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (16, 8, N'L', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (17, 9, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (18, 9, N'L', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (19, 10, N'M', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (20, 10, N'L', CAST(56000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (21, 11, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (22, 11, N'L', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (23, 12, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (24, 12, N'L', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (25, 13, N'M', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (26, 13, N'L', CAST(56000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (27, 14, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (28, 14, N'L', CAST(55000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (29, 15, N'M', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (30, 15, N'L', CAST(59000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (31, 16, N'M', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (32, 16, N'L', CAST(59000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (33, 17, N'M', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (34, 17, N'L', CAST(62000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (35, 18, N'M', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (36, 18, N'L', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (37, 19, N'M', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (38, 19, N'L', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (39, 20, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (40, 20, N'L', CAST(55000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (41, 21, N'M', CAST(42000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (42, 21, N'L', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (43, 22, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (44, 22, N'L', CAST(55000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (45, 23, N'M', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (46, 23, N'L', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (47, 24, N'M', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (48, 24, N'L', CAST(49000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (49, 25, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (50, 25, N'L', CAST(55000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (51, 26, N'M', CAST(42000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (52, 26, N'L', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (53, 27, N'M', CAST(42000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (54, 27, N'L', CAST(52000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (55, 28, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (56, 28, N'L', CAST(55000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (57, 29, N'Mặc định', CAST(25000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (58, 30, N'Mặc định', CAST(22000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (59, 31, N'Mặc định', CAST(25000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (60, 32, N'Mặc định', CAST(35000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (61, 33, N'Mặc định', CAST(38000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (62, 34, N'Mặc định', CAST(29000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (63, 35, N'Mặc định', CAST(25000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (64, 36, N'Mặc định', CAST(39000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (65, 37, N'Mặc định', CAST(42000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (66, 38, N'Mặc định', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (67, 39, N'Mặc định', CAST(10000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (68, 40, N'Mặc định', CAST(12000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (69, 41, N'Mặc định', CAST(12000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (70, 42, N'Mặc định', CAST(10000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (71, 43, N'Mặc định', CAST(12000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (72, 44, N'Mặc định', CAST(10000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (73, 45, N'Mặc định', CAST(10000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (74, 46, N'Mặc định', CAST(10000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (75, 47, N'M', CAST(47000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (76, 47, N'L', CAST(69000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (77, 48, N'M', CAST(45000.00 AS Decimal(12, 2)), 1)
INSERT [dbo].[bienthesanpham] ([MaBienThe], [MaSP], [KichCo], [GiaBan], [TrangThai]) VALUES (78, 1, N'L', CAST(65000.00 AS Decimal(12, 2)), 1)
SET IDENTITY_INSERT [dbo].[bienthesanpham] OFF
GO
SET IDENTITY_INSERT [dbo].[chitietgiohang] ON 

INSERT [dbo].[chitietgiohang] ([MaCTGH], [MaGioHang], [MaBienThe], [MaSP], [SoLuong], [KhoaDong]) VALUES (3, 5, 13, 7, 1, N'13:42')
SET IDENTITY_INSERT [dbo].[chitietgiohang] OFF
GO
INSERT [dbo].[chitietgiohang_topping] ([MaCTGH], [MaSP]) VALUES (3, 42)
GO
SET IDENTITY_INSERT [dbo].[chitiethoadon] ON 

INSERT [dbo].[chitiethoadon] ([MaCTHD], [MaHD], [MaSP], [MaBienThe], [SoLuong], [GiaBan], [ThanhTien], [TienTopping]) VALUES (1, 2, 7, 13, 3, CAST(35000.00 AS Decimal(12, 2)), CAST(105000.00 AS Decimal(12, 2)), 0)
SET IDENTITY_INSERT [dbo].[chitiethoadon] OFF
GO
SET IDENTITY_INSERT [dbo].[giohang] ON 

INSERT [dbo].[giohang] ([MaGioHang], [MaKH], [NgayTao]) VALUES (5, 1, CAST(N'2026-10-10T22:27:01.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[giohang] OFF
GO
SET IDENTITY_INSERT [dbo].[hoadon] ON 

INSERT [dbo].[hoadon] ([MaHD], [MaCaLam], [MaKH], [MaNV], [MaCTKM], [LoaiDonHang], [TongTien], [TienGiamGia], [TrangThai], [NgayTao], [PhiGiao], [HoTenNguoiNhan], [SdtNguoiNhan], [DiaChiGiao], [GhiChu], [TamTinh], [MaVoucher], [LyDoHuy], [NguoiXuLy], [NgayCapNhat], [PhuongThucThanhToan]) VALUES (2, NULL, 1, 1, NULL, N'1', CAST(105000.00 AS Decimal(12, 2)), CAST(0.00 AS Decimal(12, 2)), N'Hoàn tất', CAST(N'2026-10-12T09:26:09.0000000' AS DateTime2), 0, N'nguyen van a', N'0909090909', N'120 yen lang thanh pho ha noi', NULL, 105000, NULL, NULL, NULL, NULL, N'COD')
SET IDENTITY_INSERT [dbo].[hoadon] OFF
GO
SET IDENTITY_INSERT [dbo].[khachhang] ON 

INSERT [dbo].[khachhang] ([MaKH], [TaiKhoan], [MatKhau], [HoTen], [Sdt], [Email], [DiemTichLuy], [HangTV], [NgayTao], [DiaChi], [TrangThai], [NgaySinh]) VALUES (1, N'user01', N'$2y$10$x0gqtT./APiYudtODvg/IewGSv2PxjNDxDWQVq8GuMcz7mGeL8Yxy', N'nguyen van a', N'0909090909', NULL, 0, N'Đồng', CAST(N'2026-10-06T14:44:33.0000000' AS DateTime2), NULL, 1, NULL)
INSERT [dbo].[khachhang] ([MaKH], [TaiKhoan], [MatKhau], [HoTen], [Sdt], [Email], [DiemTichLuy], [HangTV], [NgayTao], [DiaChi], [TrangThai], [NgaySinh]) VALUES (2, N'user02', N'$2y$10$EntbLUusT30vc/LeUP.3RegMY4E8djOnX6ojiS9ehBwkGDMxiD88.', N'Nguyen van b', N'0909090909222', N'user02@gmail.com', 0, N'Đồng', CAST(N'2026-10-13T13:35:17.0000000' AS DateTime2), NULL, 1, NULL)
SET IDENTITY_INSERT [dbo].[khachhang] OFF
GO
SET IDENTITY_INSERT [dbo].[loaisanpham] ON 

INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (1, N'Cà Phê Espresso', 10)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (2, N'Cà Phê Phin Mê', 20)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (3, N'Cold Brew', 30)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (4, N'Matcha', 40)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (5, N'Trà Sữa', 50)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (6, N'Trà Trái Cây', 60)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (7, N'Fruit Boost', 70)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (8, N'Bánh Ngọt', 80)
INSERT [dbo].[loaisanpham] ([MaLoaiSP], [TenLoaiSP], [ThuTuHienThi]) VALUES (9, N'Topping', 90)
SET IDENTITY_INSERT [dbo].[loaisanpham] OFF
GO
SET IDENTITY_INSERT [dbo].[nhacungcap] ON 

INSERT [dbo].[nhacungcap] ([MaNCC], [TenNCC], [Sdt], [DiaChi], [TrangThai]) VALUES (1, N'Công ty TNHH Cà phê Việt', N'02838123456', N'123 Lê Lợi, Q.1, TP.HCM', 1)
SET IDENTITY_INSERT [dbo].[nhacungcap] OFF
GO
SET IDENTITY_INSERT [dbo].[nhanvien] ON 

INSERT [dbo].[nhanvien] ([MaNV], [HoVaDem], [Ten], [Sdt], [ChucVu]) VALUES (1, N'Nguyễn', N'Văn Quản Lý', N'0901234567', N'Quản lý')
SET IDENTITY_INSERT [dbo].[nhanvien] OFF
GO
SET IDENTITY_INSERT [dbo].[sanpham] ON 

INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (1, 2, N'Phin Mê Đen', 1, N'me-den.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (2, 2, N'Phin Mê Sữa', 1, N'me-sua.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (3, 2, N'Phin Mê Xỉu', 1, N'me-xiu.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (4, 2, N'Phin Mê Dừa Non', 1, N'me-dua-non.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (5, 1, N'Americano', 1, N'americano.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (6, 1, N'Espresso Đen Đá', 1, N'espresso-den-da.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (7, 1, N'Espresso Sữa Đá', 1, N'espresso-sua-da.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (8, 1, N'Espresso Bạc Xỉu', 1, N'espresso-bac-xiu.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (9, 1, N'Latte Nguyên Bản', 1, N'latte-nguyen-ban.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (10, 1, N'Cà Phê Kem Mây Chanh Vàng', 1, N'cafe-kem-may-chanh-vang.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (11, 3, N'Cold Brew Chanh', 1, N'cold-brew-chanh.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (12, 3, N'Cold Brew Quýt', 1, N'cold-brew-quyt.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (13, 3, N'Cold Brew Tằm', 1, N'cold-brew-tam.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (14, 4, N'Matcha Latte', 1, N'matcha-latte.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (15, 4, N'Matcha Dừa Xiêm', 1, N'matcha-dua-xiem.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (16, 4, N'Matcha Bắp Bắp', 1, N'matcha-bap-bi.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (17, 4, N'Matcha Tofu', 1, N'matcha-tofu.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (18, 5, N'Trà Sữa Oolong Nướng', 1, N'tra-sua-oolong-nuong.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (19, 5, N'Trà Sữa Oolong Ba Lá', 1, N'tra-sua-oolong-ba-la.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (20, 5, N'Trà Sữa Chôm Chôm', 1, N'tra-sua-chom-chom.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (21, 5, N'Trà Sữa Thanh Hương', 1, N'tra-sua-thanh-huong.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (22, 5, N'Nhài Sữa Dẻ Cười', 1, N'nhai-sua-de-cuoi.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (23, 6, N'Trà Đào Hồng Đài', 1, N'tra-dao-hong-dai.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (24, 6, N'Trà Quýt', 1, N'tra-quyt.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (25, 6, N'Cóc Cóc Đác Đác', 1, N'coc-coc-dac-dac.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (26, 6, N'Lệ Chi Thanh Trà', 1, N'le-chi-thanh-tra.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (27, 7, N'Socola', 1, N'so-co-la.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (28, 7, N'Sữa Chua Tằm', 1, N'sua-chua-tam.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (29, 8, N'Bánh Chuối', 1, N'banh-chuoi.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (30, 8, N'Bánh Tráng Muối Tỏi', 1, N'banh-trang-muoi-toi.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (31, 8, N'Bánh Tráng Tôm Hành', 1, N'banh-trang-tom-hanh.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (32, 8, N'Bông Lan Trứng Muối', 1, N'bong-lan-trung-muoi.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (33, 8, N'Bông Lan Trứng Muối Dầu Trứng', 1, N'bong-lan-trung-muoi-dau-trung.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (34, 8, N'Crème Flan', 1, N'creme-plan.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (35, 8, N'Bánh Flan', 1, N'plan.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (36, 8, N'Bánh Phô Mai Chanh Dây', 1, N'pho-mai-chanh-day.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (37, 8, N'Bánh Phô Mai Tan Chảy', 1, N'pho-mai-tan-chay.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (38, 8, N'Bánh Phô Mai Tom & Jerry', 1, N'tom-and-jerry.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (39, 9, N'Huyền Châu', 1, N'huyen-chau.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (40, 9, N'Phô Mai Dẻo', 1, N'pho-mai-deo.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (41, 9, N'Tofu', 1, N'tofu.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (42, 9, N'Thạch Hồng Đài', 1, N'thach-hong-dai.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (43, 9, N'Thốt Nốt', 1, N'thot-not.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (44, 9, N'Trân Châu Dừa', 1, N'tran-chau-dua.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (45, 9, N'Trân Châu Matcha', 1, N'tran-chau-matcha.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (46, 9, N'Trân Châu Trắng', 1, N'tranchau-trang.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (47, 7, N'Bơ Già Dừa Non', 1, N'bo-dua-non.png', 50)
INSERT [dbo].[sanpham] ([MaSP], [MaLoaiSP], [TenSP], [TrangThai], [HinhAnh], [SoLuongTon]) VALUES (48, 7, N' Bơ Sáp Khoai Môn Macchiato', 1, N'bo-khoaimon.png', 50)
SET IDENTITY_INSERT [dbo].[sanpham] OFF
GO
SET IDENTITY_INSERT [dbo].[taikhoan] ON 

INSERT [dbo].[taikhoan] ([MaTaiKhoan], [MaVaiTro], [TenDangNhap], [MatKhau], [HoTen], [TrangThai]) VALUES (4, 4, N'admin', N'$2y$10$k23UXslDcs947rkCallV5OIgKO6A2HZCBtD9WUev7buorq8oVtz1i', N'Quản trị viên', 1)
SET IDENTITY_INSERT [dbo].[taikhoan] OFF
GO
SET IDENTITY_INSERT [dbo].[thanhtoan] ON 

INSERT [dbo].[thanhtoan] ([MaThanhToan], [MaHD], [PhuongThuc], [SoTien], [MaGiaoDich], [ThoiGian], [TrangThai]) VALUES (1, 2, N'COD', CAST(105000.00 AS Decimal(12, 2)), NULL, CAST(N'2026-10-12T09:26:09.0000000' AS DateTime2), 0)
SET IDENTITY_INSERT [dbo].[thanhtoan] OFF
GO
SET IDENTITY_INSERT [dbo].[vaitro] ON 

INSERT [dbo].[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (3, N'NhanVien', N'Nhân viên vận hành')
INSERT [dbo].[vaitro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (4, N'Admin', N'Quản trị hệ thống')
SET IDENTITY_INSERT [dbo].[vaitro] OFF
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[vaitro] WHERE [TenVaiTro] = N'CRM')
    INSERT [dbo].[vaitro] ([TenVaiTro], [MoTa]) VALUES (N'CRM', N'Nhân viên chăm sóc khách hàng và CRM')
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_chitietgiohang_uq_dong]    Script Date: 28/10/2026 2:36:00 CH ******/
ALTER TABLE [dbo].[chitietgiohang] ADD  CONSTRAINT [UQ_chitietgiohang_uq_dong] UNIQUE NONCLUSTERED 
(
	[MaGioHang] ASC,
	[KhoaDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_khachhang_Sdt]    Script Date: 28/10/2026 2:36:00 CH ******/
ALTER TABLE [dbo].[khachhang] ADD  CONSTRAINT [UQ_khachhang_Sdt] UNIQUE NONCLUSTERED 
(
	[Sdt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_khachhang_Email]    Script Date: 28/10/2026 2:36:00 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_khachhang_Email] ON [dbo].[khachhang]
(
	[Email] ASC
)
WHERE ([Email] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_khachhang_TaiKhoan]    Script Date: 28/10/2026 2:36:00 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_khachhang_TaiKhoan] ON [dbo].[khachhang]
(
	[TaiKhoan] ASC
)
WHERE ([TaiKhoan] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_taikhoan_TenDangNhap]    Script Date: 28/10/2026 2:36:00 CH ******/
ALTER TABLE [dbo].[taikhoan] ADD  CONSTRAINT [UQ_taikhoan_TenDangNhap] UNIQUE NONCLUSTERED 
(
	[TenDangNhap] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_voucher_MaCode]    Script Date: 28/10/2026 2:36:00 CH ******/
ALTER TABLE [dbo].[voucher] ADD  CONSTRAINT [UQ_voucher_MaCode] UNIQUE NONCLUSTERED 
(
	[MaCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[bienthesanpham] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[calam] ADD  DEFAULT (getdate()) FOR [GioMoCa]
GO
ALTER TABLE [dbo].[calam] ADD  DEFAULT ((0.00)) FOR [TienDauCa]
GO
ALTER TABLE [dbo].[cauhoikhaosat] ADD  DEFAULT (N'TRAC_NGHIEM') FOR [LoaiCauHoi]
GO
ALTER TABLE [dbo].[cautraloi] ADD  DEFAULT (getdate()) FOR [NgayTraLoi]
GO
ALTER TABLE [dbo].[chitietgiohang] ADD  DEFAULT ((1)) FOR [SoLuong]
GO
ALTER TABLE [dbo].[chitiethoadon] ADD  DEFAULT ((0)) FOR [TienTopping]
GO
ALTER TABLE [dbo].[chitietkm] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[ctkm_theo_hoadon] ADD  DEFAULT (N'AMOUNT') FOR [KieuGiam]
GO
ALTER TABLE [dbo].[ctkm_theo_sp] ADD  DEFAULT (N'PERCENT') FOR [KieuGiam]
GO
ALTER TABLE [dbo].[danhmucluachon] ADD  DEFAULT ((1)) FOR [ThuTuHienThi]
GO
ALTER TABLE [dbo].[giohang] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT (N'Tại quán') FOR [LoaiDonHang]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT ((0.00)) FOR [TongTien]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT ((0.00)) FOR [TienGiamGia]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT (N'Đã thanh toán') FOR [TrangThai]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT ((0)) FOR [PhiGiao]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT (N'') FOR [HoTenNguoiNhan]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT (N'') FOR [SdtNguoiNhan]
GO
ALTER TABLE [dbo].[hoadon] ADD  DEFAULT ((0)) FOR [TamTinh]
GO
ALTER TABLE [dbo].[hoadon] ADD  CONSTRAINT [DF_hoadon_PhuongThucThanhToan]  DEFAULT (N'COD') FOR [PhuongThucThanhToan]
GO
ALTER TABLE [dbo].[khachhang] ADD  DEFAULT ((0)) FOR [DiemTichLuy]
GO
ALTER TABLE [dbo].[khachhang] ADD  DEFAULT (N'Đồng') FOR [HangTV]
GO
ALTER TABLE [dbo].[khachhang] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[khachhang] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[lichsudiem] ADD  DEFAULT (getdate()) FOR [NgayThucHien]
GO
ALTER TABLE [dbo].[LienHe] ADD  CONSTRAINT [DF_LienHe_NgayGui]  DEFAULT (getdate()) FOR [NgayGui]
GO
ALTER TABLE [dbo].[LienHe] ADD  CONSTRAINT [DF_LienHe_TrangThai]  DEFAULT (N'ChÆ°a xá»­ lĂ½') FOR [TrangThai]
GO
ALTER TABLE [dbo].[loaisanpham] ADD  DEFAULT ((1)) FOR [ThuTuHienThi]
GO
ALTER TABLE [dbo].[loimoikhaosat] ADD  DEFAULT (getdate()) FOR [ThoiGianGui]
GO
ALTER TABLE [dbo].[loimoikhaosat] ADD  DEFAULT (N'Chưa làm') FOR [TrangThai]
GO
ALTER TABLE [dbo].[nguyenlieu] ADD  DEFAULT ((0.00)) FOR [GiaVon]
GO
ALTER TABLE [dbo].[nhacungcap] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[nhatkyhoatdong] ADD  DEFAULT (getdate()) FOR [ThoiGian]
GO
ALTER TABLE [dbo].[phanhoi] ADD  DEFAULT (getdate()) FOR [NgayGui]
GO
ALTER TABLE [dbo].[phanhoi] ADD  DEFAULT (N'Chờ xử lý') FOR [TrangThaiXL]
GO
ALTER TABLE [dbo].[phieunhap] ADD  DEFAULT (getdate()) FOR [NgayNhap]
GO
ALTER TABLE [dbo].[phieunhap] ADD  DEFAULT ((0.00)) FOR [TongTien]
GO
ALTER TABLE [dbo].[phieunhap] ADD  CONSTRAINT [DF_phieunhap_TrangThai]  DEFAULT (N'Chá» duyá»‡t') FOR [TrangThai]
GO
ALTER TABLE [dbo].[phieunhap] ADD  CONSTRAINT [DF_phieunhap_NguoiLap]  DEFAULT (N'') FOR [NguoiLap]
GO
ALTER TABLE [dbo].[sanpham] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[sanpham] ADD  CONSTRAINT [DF_sanpham_SoLuongTon]  DEFAULT ((0)) FOR [SoLuongTon]
GO
ALTER TABLE [dbo].[taikhoan] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[thanhtoan] ADD  DEFAULT (N'Tiền mặt') FOR [PhuongThuc]
GO
ALTER TABLE [dbo].[thanhtoan] ADD  DEFAULT (getdate()) FOR [ThoiGian]
GO
ALTER TABLE [dbo].[thanhtoan] ADD  DEFAULT ((0)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[voucher] ADD  DEFAULT (N'PERCENT') FOR [PhanLoaiGiam]
GO
ALTER TABLE [dbo].[voucher] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[voucher] ADD  CONSTRAINT [DF_voucher_DonToiThieu]  DEFAULT ((0)) FOR [DonToiThieu]
GO
ALTER TABLE [dbo].[voucher] ADD  CONSTRAINT [DF_voucher_SoLuongToiDa]  DEFAULT ((100)) FOR [SoLuongToiDa]
GO
ALTER TABLE [dbo].[voucher] ADD  CONSTRAINT [DF_voucher_SoLuongDaDung]  DEFAULT ((0)) FOR [SoLuongDaDung]
GO
ALTER TABLE [dbo].[bienthesanpham]  WITH CHECK ADD  CONSTRAINT [fk_bienthe_sanpham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[bienthesanpham] CHECK CONSTRAINT [fk_bienthe_sanpham]
GO
ALTER TABLE [dbo].[calam]  WITH CHECK ADD  CONSTRAINT [fk_calam_nhanvien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[nhanvien] ([MaNV])
GO
ALTER TABLE [dbo].[calam] CHECK CONSTRAINT [fk_calam_nhanvien]
GO
ALTER TABLE [dbo].[cauhoikhaosat]  WITH CHECK ADD  CONSTRAINT [fk_cauhoi_khaosat] FOREIGN KEY([MaKhaoSat])
REFERENCES [dbo].[khaosat] ([MaKhaoSat])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cauhoikhaosat] CHECK CONSTRAINT [fk_cauhoi_khaosat]
GO
ALTER TABLE [dbo].[cautraloi]  WITH CHECK ADD  CONSTRAINT [fk_cautraloi_cauhoi] FOREIGN KEY([MaCauHoi])
REFERENCES [dbo].[cauhoikhaosat] ([MaCauHoi])
GO
ALTER TABLE [dbo].[cautraloi] CHECK CONSTRAINT [fk_cautraloi_cauhoi]
GO
ALTER TABLE [dbo].[cautraloi]  WITH CHECK ADD  CONSTRAINT [fk_cautraloi_danhmuc] FOREIGN KEY([MaDanhMuc])
REFERENCES [dbo].[danhmucluachon] ([MaDanhMuc])
GO
ALTER TABLE [dbo].[cautraloi] CHECK CONSTRAINT [fk_cautraloi_danhmuc]
GO
ALTER TABLE [dbo].[cautraloi]  WITH CHECK ADD  CONSTRAINT [fk_cautraloi_loimoi] FOREIGN KEY([MaLoiMoi])
REFERENCES [dbo].[loimoikhaosat] ([MaLoiMoi])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cautraloi] CHECK CONSTRAINT [fk_cautraloi_loimoi]
GO
ALTER TABLE [dbo].[chitietgiohang]  WITH CHECK ADD  CONSTRAINT [fk_ctgh_giohang] FOREIGN KEY([MaGioHang])
REFERENCES [dbo].[giohang] ([MaGioHang])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chitietgiohang] CHECK CONSTRAINT [fk_ctgh_giohang]
GO
ALTER TABLE [dbo].[chitietgiohang]  WITH CHECK ADD  CONSTRAINT [fk_ctgh_sanpham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[chitietgiohang] CHECK CONSTRAINT [fk_ctgh_sanpham]
GO
ALTER TABLE [dbo].[chitietgiohang_topping]  WITH CHECK ADD  CONSTRAINT [chitietgiohang_topping_ibfk_1] FOREIGN KEY([MaCTGH])
REFERENCES [dbo].[chitietgiohang] ([MaCTGH])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chitietgiohang_topping] CHECK CONSTRAINT [chitietgiohang_topping_ibfk_1]
GO
ALTER TABLE [dbo].[chitietgiohang_topping]  WITH CHECK ADD  CONSTRAINT [chitietgiohang_topping_ibfk_2] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[chitietgiohang_topping] CHECK CONSTRAINT [chitietgiohang_topping_ibfk_2]
GO
ALTER TABLE [dbo].[chitiethoadon]  WITH CHECK ADD  CONSTRAINT [fk_cthd_hoadon] FOREIGN KEY([MaHD])
REFERENCES [dbo].[hoadon] ([MaHD])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chitiethoadon] CHECK CONSTRAINT [fk_cthd_hoadon]
GO
ALTER TABLE [dbo].[chitiethoadon]  WITH CHECK ADD  CONSTRAINT [fk_cthd_sanpham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[chitiethoadon] CHECK CONSTRAINT [fk_cthd_sanpham]
GO
ALTER TABLE [dbo].[chitiethoadon_topping]  WITH CHECK ADD  CONSTRAINT [chitiethoadon_topping_ibfk_1] FOREIGN KEY([MaCTHD])
REFERENCES [dbo].[chitiethoadon] ([MaCTHD])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chitiethoadon_topping] CHECK CONSTRAINT [chitiethoadon_topping_ibfk_1]
GO
ALTER TABLE [dbo].[chitiethoadon_topping]  WITH CHECK ADD  CONSTRAINT [chitiethoadon_topping_ibfk_2] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[chitiethoadon_topping] CHECK CONSTRAINT [chitiethoadon_topping_ibfk_2]
GO
ALTER TABLE [dbo].[chitietphieunhap]  WITH CHECK ADD  CONSTRAINT [fk_ctpn_nguyenlieu] FOREIGN KEY([MaNL])
REFERENCES [dbo].[nguyenlieu] ([MaNL])
GO
ALTER TABLE [dbo].[chitietphieunhap] CHECK CONSTRAINT [fk_ctpn_nguyenlieu]
GO
ALTER TABLE [dbo].[chitietphieunhap]  WITH CHECK ADD  CONSTRAINT [fk_ctpn_phieunhap] FOREIGN KEY([MaPN])
REFERENCES [dbo].[phieunhap] ([MaPN])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chitietphieunhap] CHECK CONSTRAINT [fk_ctpn_phieunhap]
GO
ALTER TABLE [dbo].[chitietphieunhap]  WITH CHECK ADD  CONSTRAINT [fk_ctpn_sanpham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[chitietphieunhap] CHECK CONSTRAINT [fk_ctpn_sanpham]
GO
ALTER TABLE [dbo].[ctkm_theo_hoadon]  WITH CHECK ADD  CONSTRAINT [fk_ktkm_hoadon_ctkm] FOREIGN KEY([MaCTKM])
REFERENCES [dbo].[chitietkm] ([MaCTKM])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ctkm_theo_hoadon] CHECK CONSTRAINT [fk_ktkm_hoadon_ctkm]
GO
ALTER TABLE [dbo].[ctkm_theo_sp]  WITH CHECK ADD  CONSTRAINT [fk_ctkm_sp_ctkm] FOREIGN KEY([MaCTKM])
REFERENCES [dbo].[chitietkm] ([MaCTKM])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ctkm_theo_sp] CHECK CONSTRAINT [fk_ctkm_sp_ctkm]
GO
ALTER TABLE [dbo].[ctkm_theo_sp]  WITH CHECK ADD  CONSTRAINT [fk_ctkm_sp_sanpham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[ctkm_theo_sp] CHECK CONSTRAINT [fk_ctkm_sp_sanpham]
GO
ALTER TABLE [dbo].[danhmucluachon]  WITH CHECK ADD  CONSTRAINT [fk_danhmuc_cauhoi] FOREIGN KEY([MaCauHoi])
REFERENCES [dbo].[cauhoikhaosat] ([MaCauHoi])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[danhmucluachon] CHECK CONSTRAINT [fk_danhmuc_cauhoi]
GO
ALTER TABLE [dbo].[giohang]  WITH CHECK ADD  CONSTRAINT [fk_giohang_khachhang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[khachhang] ([MaKH])
GO
ALTER TABLE [dbo].[giohang] CHECK CONSTRAINT [fk_giohang_khachhang]
GO
ALTER TABLE [dbo].[hoadon]  WITH CHECK ADD  CONSTRAINT [fk_hoadon_calam] FOREIGN KEY([MaCaLam])
REFERENCES [dbo].[calam] ([MaCaLam])
GO
ALTER TABLE [dbo].[hoadon] CHECK CONSTRAINT [fk_hoadon_calam]
GO
ALTER TABLE [dbo].[hoadon]  WITH CHECK ADD  CONSTRAINT [fk_hoadon_ctkm] FOREIGN KEY([MaCTKM])
REFERENCES [dbo].[chitietkm] ([MaCTKM])
GO
ALTER TABLE [dbo].[hoadon] CHECK CONSTRAINT [fk_hoadon_ctkm]
GO
ALTER TABLE [dbo].[hoadon]  WITH CHECK ADD  CONSTRAINT [fk_hoadon_khachhang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[khachhang] ([MaKH])
GO
ALTER TABLE [dbo].[hoadon] CHECK CONSTRAINT [fk_hoadon_khachhang]
GO
ALTER TABLE [dbo].[hoadon]  WITH CHECK ADD  CONSTRAINT [fk_hoadon_nhanvien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[nhanvien] ([MaNV])
GO
ALTER TABLE [dbo].[hoadon] CHECK CONSTRAINT [fk_hoadon_nhanvien]
GO
ALTER TABLE [dbo].[hoadon]  WITH CHECK ADD  CONSTRAINT [fk_hoadon_voucher] FOREIGN KEY([MaVoucher])
REFERENCES [dbo].[voucher] ([MaVoucher])
GO
ALTER TABLE [dbo].[hoadon] CHECK CONSTRAINT [fk_hoadon_voucher]
GO
ALTER TABLE [dbo].[khaosat]  WITH CHECK ADD  CONSTRAINT [fk_khaosat_taikhoan] FOREIGN KEY([MaTaiKhoan])
REFERENCES [dbo].[taikhoan] ([MaTaiKhoan])
GO
ALTER TABLE [dbo].[khaosat] CHECK CONSTRAINT [fk_khaosat_taikhoan]
GO
ALTER TABLE [dbo].[lichsudiem]  WITH CHECK ADD  CONSTRAINT [fk_lsd_hoadon] FOREIGN KEY([MaHD])
REFERENCES [dbo].[hoadon] ([MaHD])
GO
ALTER TABLE [dbo].[lichsudiem] CHECK CONSTRAINT [fk_lsd_hoadon]
GO
ALTER TABLE [dbo].[lichsudiem]  WITH CHECK ADD  CONSTRAINT [fk_lsd_khachhang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[khachhang] ([MaKH])
GO
ALTER TABLE [dbo].[lichsudiem] CHECK CONSTRAINT [fk_lsd_khachhang]
GO
ALTER TABLE [dbo].[loimoikhaosat]  WITH CHECK ADD  CONSTRAINT [fk_loimoi_khachhang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[khachhang] ([MaKH])
GO
ALTER TABLE [dbo].[loimoikhaosat] CHECK CONSTRAINT [fk_loimoi_khachhang]
GO
ALTER TABLE [dbo].[loimoikhaosat]  WITH CHECK ADD  CONSTRAINT [fk_loimoi_khaosat] FOREIGN KEY([MaKhaoSat])
REFERENCES [dbo].[khaosat] ([MaKhaoSat])
GO
ALTER TABLE [dbo].[loimoikhaosat] CHECK CONSTRAINT [fk_loimoi_khaosat]
GO
ALTER TABLE [dbo].[nhatkyhoatdong]  WITH CHECK ADD  CONSTRAINT [fk_nhatky_taikhoan] FOREIGN KEY([MaTaiKhoan])
REFERENCES [dbo].[taikhoan] ([MaTaiKhoan])
GO
ALTER TABLE [dbo].[nhatkyhoatdong] CHECK CONSTRAINT [fk_nhatky_taikhoan]
GO
ALTER TABLE [dbo].[phanhoi]  WITH CHECK ADD  CONSTRAINT [fk_phanhoi_khachhang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[khachhang] ([MaKH])
GO
ALTER TABLE [dbo].[phanhoi] CHECK CONSTRAINT [fk_phanhoi_khachhang]
GO
ALTER TABLE [dbo].[phanhoi]  WITH CHECK ADD  CONSTRAINT [fk_phanhoi_sanpham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[sanpham] ([MaSP])
GO
ALTER TABLE [dbo].[phanhoi] CHECK CONSTRAINT [fk_phanhoi_sanpham]
GO
ALTER TABLE [dbo].[phieunhap]  WITH CHECK ADD  CONSTRAINT [fk_phieunhap_ncc] FOREIGN KEY([MaNCC])
REFERENCES [dbo].[nhacungcap] ([MaNCC])
GO
ALTER TABLE [dbo].[phieunhap] CHECK CONSTRAINT [fk_phieunhap_ncc]
GO
ALTER TABLE [dbo].[phieunhap]  WITH CHECK ADD  CONSTRAINT [fk_phieunhap_nguoiduyet] FOREIGN KEY([NguoiDuyetId])
REFERENCES [dbo].[taikhoan] ([MaTaiKhoan])
GO
ALTER TABLE [dbo].[phieunhap] CHECK CONSTRAINT [fk_phieunhap_nguoiduyet]
GO
ALTER TABLE [dbo].[phieunhap]  WITH CHECK ADD  CONSTRAINT [fk_phieunhap_nhanvien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[nhanvien] ([MaNV])
GO
ALTER TABLE [dbo].[phieunhap] CHECK CONSTRAINT [fk_phieunhap_nhanvien]
GO
ALTER TABLE [dbo].[sanpham]  WITH CHECK ADD  CONSTRAINT [fk_sanpham_loaisp] FOREIGN KEY([MaLoaiSP])
REFERENCES [dbo].[loaisanpham] ([MaLoaiSP])
GO
ALTER TABLE [dbo].[sanpham] CHECK CONSTRAINT [fk_sanpham_loaisp]
GO
ALTER TABLE [dbo].[taikhoan]  WITH CHECK ADD  CONSTRAINT [fk_taikhoan_vaitro] FOREIGN KEY([MaVaiTro])
REFERENCES [dbo].[vaitro] ([MaVaiTro])
GO
ALTER TABLE [dbo].[taikhoan] CHECK CONSTRAINT [fk_taikhoan_vaitro]
GO
ALTER TABLE [dbo].[thanhtoan]  WITH CHECK ADD  CONSTRAINT [fk_thanhtoan_hoadon] FOREIGN KEY([MaHD])
REFERENCES [dbo].[hoadon] ([MaHD])
GO
ALTER TABLE [dbo].[thanhtoan] CHECK CONSTRAINT [fk_thanhtoan_hoadon]
GO
ALTER TABLE [dbo].[phanhoi]  WITH CHECK ADD CHECK  (([DanhGia]>=(1) AND [DanhGia]<=(5)))
GO


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'fk_cthd_bienthe')
BEGIN
    ALTER TABLE [dbo].[chitiethoadon] WITH CHECK ADD CONSTRAINT [fk_cthd_bienthe] FOREIGN KEY([MaBienThe])
    REFERENCES [dbo].[bienthesanpham] ([MaBienThe]);
    ALTER TABLE [dbo].[chitiethoadon] CHECK CONSTRAINT [fk_cthd_bienthe];
END
GO

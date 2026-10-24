-- Script tạo CSDL Microsoft SQL Server cho Coffee Shop
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CoffeeShopDb')
BEGIN
    CREATE DATABASE CoffeeShopDb;
END
GO

USE CoffeeShopDb;
GO

-- 1. Bảng Loại Sản Phẩm (Categories)
IF OBJECT_ID('LoaiSanPham', 'U') IS NOT NULL DROP TABLE LoaiSanPham;
CREATE TABLE LoaiSanPham (
    MaLoaiSP INT IDENTITY(1,1) PRIMARY KEY,
    TenLoaiSP NVARCHAR(100) NOT NULL,
    ThuTuHienThi INT DEFAULT 1
);
GO

-- 2. Bảng Sản Phẩm (Products)
IF OBJECT_ID('SanPham', 'U') IS NOT NULL DROP TABLE SanPham;
CREATE TABLE SanPham (
    MaSP INT IDENTITY(1,1) PRIMARY KEY,
    MaLoaiSP INT NOT NULL,
    TenSP NVARCHAR(100) NOT NULL,
    TrangThai BIT DEFAULT 1,
    HinhAnh NVARCHAR(255) NULL,
    CONSTRAINT FK_SanPham_LoaiSanPham FOREIGN KEY (MaLoaiSP) REFERENCES LoaiSanPham(MaLoaiSP)
);
GO

-- 3. Bảng Biến Thể Sản Phẩm (Product Variants: Size M, L...)
IF OBJECT_ID('BienTheSanPham', 'U') IS NOT NULL DROP TABLE BienTheSanPham;
CREATE TABLE BienTheSanPham (
    MaBienThe INT IDENTITY(1,1) PRIMARY KEY,
    MaSP INT NOT NULL,
    KichCo NVARCHAR(20) NOT NULL,
    GiaBan DECIMAL(12,2) NOT NULL,
    TrangThai BIT DEFAULT 1,
    CONSTRAINT FK_BienThe_SanPham FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 4. Bảng Vai Trò (Roles)
IF OBJECT_ID('VaiTro', 'U') IS NOT NULL DROP TABLE VaiTro;
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(255) NULL
);
GO

-- 5. Bảng Tài Khoản Hệ Thống / Nhân Viên (Accounts)
IF OBJECT_ID('TaiKhoan', 'U') IS NOT NULL DROP TABLE TaiKhoan;
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    MaVaiTro INT NOT NULL,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    TrangThai BIT DEFAULT 1,
    CONSTRAINT FK_TaiKhoan_VaiTro FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- 6. Bảng Khách Hàng (Customers)
IF OBJECT_ID('KhachHang', 'U') IS NOT NULL DROP TABLE KhachHang;
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoan NVARCHAR(50) NULL UNIQUE,
    MatKhau NVARCHAR(255) NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Sdt NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NULL,
    DiemTichLuy INT DEFAULT 0,
    HangTV NVARCHAR(20) DEFAULT N'Đồng',
    NgayTao DATETIME2 DEFAULT GETDATE(),
    DiaChi NVARCHAR(255) NULL
);
GO

-- 7. Bảng Voucher
IF OBJECT_ID('Voucher', 'U') IS NOT NULL DROP TABLE Voucher;
CREATE TABLE Voucher (
    MaVoucher INT IDENTITY(1,1) PRIMARY KEY,
    MaCode NVARCHAR(50) NOT NULL UNIQUE,
    PhanLoaiGiam NVARCHAR(20) DEFAULT 'PERCENT', -- PERCENT hoặc AMOUNT
    GiaTriGiam DECIMAL(12,2) NOT NULL,
    TenChuongTrinh NVARCHAR(150) NULL,
    NgayBatDau DATETIME2 NULL,
    NgayKetThuc DATETIME2 NULL,
    DonToiThieu DECIMAL(12,2) NOT NULL DEFAULT 0,
    TrangThai BIT DEFAULT 1
);
GO

-- 8. Bảng Hóa Đơn (Orders)
IF OBJECT_ID('HoaDon', 'U') IS NOT NULL DROP TABLE HoaDon;
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NULL,
    LoaiDonHang NVARCHAR(50) DEFAULT N'Giao hàng',
    TongTien DECIMAL(12,2) DEFAULT 0.00,
    TienGiamGia DECIMAL(12,2) DEFAULT 0.00,
    TamTinh INT DEFAULT 0,
    PhiGiao INT DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Hoàn tất',
    NgayTao DATETIME2 DEFAULT GETDATE(),
    HoTenNguoiNhan NVARCHAR(100) NOT NULL,
    SdtNguoiNhan NVARCHAR(15) NOT NULL,
    DiaChiGiao NVARCHAR(255) NULL,
    GhiChu NVARCHAR(255) NULL,
    CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);
GO

-- 9. Bảng Chi Tiết Hóa Đơn (Order Details)
IF OBJECT_ID('ChiTietHoaDon', 'U') IS NOT NULL DROP TABLE ChiTietHoaDon;
CREATE TABLE ChiTietHoaDon (
    MaCTHD INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT NOT NULL,
    MaSP INT NOT NULL,
    MaBienThe INT NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    GiaBan DECIMAL(12,2) NOT NULL,
    ThanhTien DECIMAL(12,2) NOT NULL,
    TienTopping INT DEFAULT 0,
    CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD) ON DELETE CASCADE,
    CONSTRAINT FK_CTHD_SanPham FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP),
    CONSTRAINT FK_CTHD_BienThe FOREIGN KEY (MaBienThe) REFERENCES BienTheSanPham(MaBienThe)
);
GO

-- 10. Bảng Chi Tiết Hóa Đơn Topping (Order Detail Toppings)
IF OBJECT_ID('ChiTietHoaDon_Topping', 'U') IS NOT NULL DROP TABLE ChiTietHoaDon_Topping;
CREATE TABLE ChiTietHoaDon_Topping (
    MaCTHD INT NOT NULL,
    MaSP INT NOT NULL, -- Topping product
    GiaBan INT NOT NULL,
    PRIMARY KEY (MaCTHD, MaSP),
    CONSTRAINT FK_CTHDTopping_CTHD FOREIGN KEY (MaCTHD) REFERENCES ChiTietHoaDon(MaCTHD) ON DELETE CASCADE,
    CONSTRAINT FK_CTHDTopping_SanPham FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 11. Bảng Lịch Sử Điểm (Point History)
IF OBJECT_ID('LichSuDiem', 'U') IS NOT NULL DROP TABLE LichSuDiem;
CREATE TABLE LichSuDiem (
    MaLSD INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT NULL,
    MaKH INT NOT NULL,
    DiemThayDoi INT NOT NULL,
    LyDo NVARCHAR(255) NULL,
    NgayThucHien DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_LSD_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);
GO

-- 12. Bảng Phản Hồi / Đánh Giá Sản Phẩm (Reviews & Feedbacks)
IF OBJECT_ID('PhanHoi', 'U') IS NOT NULL DROP TABLE PhanHoi;
CREATE TABLE PhanHoi (
    MaPhanHoi INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NOT NULL,
    MaSP INT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    DanhGia INT NULL CHECK (DanhGia BETWEEN 1 AND 5),
    NgayGui DATETIME2 DEFAULT GETDATE(),
    TrangThaiXL NVARCHAR(50) DEFAULT N'Chờ xử lý',
    CONSTRAINT FK_PhanHoi_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_PhanHoi_SanPham FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 13. Khảo sát CRM (Surveys)
IF OBJECT_ID('CauTraLoi', 'U') IS NOT NULL DROP TABLE CauTraLoi;
IF OBJECT_ID('CauHoiKhaoSat', 'U') IS NOT NULL DROP TABLE CauHoiKhaoSat;
IF OBJECT_ID('KhaoSat', 'U') IS NOT NULL DROP TABLE KhaoSat;

CREATE TABLE KhaoSat (
    MaKhaoSat INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan INT NOT NULL,
    TieuDe NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    NgayBatDau DATETIME2 NOT NULL,
    NgayKetThuc DATETIME2 NOT NULL,
    CONSTRAINT FK_KhaoSat_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);
GO

CREATE TABLE CauHoiKhaoSat (
    MaCauHoi INT IDENTITY(1,1) PRIMARY KEY,
    MaKhaoSat INT NOT NULL,
    NoiDungCauHoi NVARCHAR(255) NOT NULL,
    LoaiCauHoi NVARCHAR(50) DEFAULT 'TRAC_NGHIEM',
    CONSTRAINT FK_CauHoi_KhaoSat FOREIGN KEY (MaKhaoSat) REFERENCES KhaoSat(MaKhaoSat) ON DELETE CASCADE
);
GO

CREATE TABLE CauTraLoi (
    MaCauTraLoi INT IDENTITY(1,1) PRIMARY KEY,
    MaCauHoi INT NOT NULL,
    NoiDungTraLoi NVARCHAR(MAX) NULL,
    NgayTraLoi DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_CauTraLoi_CauHoi FOREIGN KEY (MaCauHoi) REFERENCES CauHoiKhaoSat(MaCauHoi) ON DELETE CASCADE
);
GO

-- -------------------------------------------------------------
-- SEED DATA (Dữ liệu mẫu)
-- -------------------------------------------------------------
SET IDENTITY_INSERT LoaiSanPham ON;
INSERT INTO LoaiSanPham (MaLoaiSP, TenLoaiSP, ThuTuHienThi) VALUES
(1, N'Cà Phê Espresso', 10),
(2, N'Cà Phê Phin Mê', 20),
(3, N'Cold Brew', 30),
(4, N'Matcha', 40),
(5, N'Trà Sữa', 50),
(6, N'Trà Trái Cây', 60),
(7, N'Fruit Boost', 70),
(8, N'Bánh Ngọt', 80),
(9, N'Topping', 90);
SET IDENTITY_INSERT LoaiSanPham OFF;
GO

SET IDENTITY_INSERT SanPham ON;
INSERT INTO SanPham (MaSP, MaLoaiSP, TenSP, TrangThai, HinhAnh) VALUES
(1, 2, N'Phin Mê Đen', 1, 'me-den.png'),
(2, 2, N'Phin Mê Sữa', 1, 'me-sua.png'),
(3, 2, N'Phin Mê Xỉu', 1, 'me-xiu.png'),
(4, 2, N'Phin Mê Dừa Non', 1, 'me-dua-non.png'),
(5, 1, N'Americano', 1, 'americano.png'),
(6, 1, N'Espresso Đen Đá', 1, 'espresso-den-da.png'),
(7, 1, N'Espresso Sữa Đá', 1, 'espresso-sua-da.png'),
(8, 1, N'Espresso Bạc Xỉu', 1, 'espresso-bac-xiu.png'),
(9, 1, N'Latte Nguyên Bản', 1, 'latte-nguyen-ban.png'),
(10, 1, N'Cà Phê Kem Mây Chanh Vàng', 1, 'cafe-kem-may-chanh-vang.png'),
(11, 3, N'Cold Brew Chanh', 1, 'cold-brew-chanh.png'),
(12, 3, N'Cold Brew Quýt', 1, 'cold-brew-quyt.png'),
(13, 3, N'Cold Brew Tằm', 1, 'cold-brew-tam.png'),
(14, 4, N'Matcha Latte', 1, 'matcha-latte.png'),
(15, 4, N'Matcha Dừa Xiêm', 1, 'matcha-dua-xiem.png'),
(16, 4, N'Matcha Bắp Bắp', 1, 'matcha-bap-bi.png'),
(17, 4, N'Matcha Tofu', 1, 'matcha-tofu.png'),
(18, 5, N'Trà Sữa Oolong Nướng', 1, 'tra-sua-oolong-nuong.png'),
(19, 5, N'Trà Sữa Oolong Ba Lá', 1, 'tra-sua-oolong-ba-la.png'),
(20, 5, N'Trà Sữa Chôm Chôm', 1, 'tra-sua-chom-chom.png'),
(21, 5, N'Trà Sữa Thanh Hương', 1, 'tra-sua-thanh-huong.png'),
(22, 5, N'Nhài Sữa Dẻ Cười', 1, 'nhai-sua-de-cuoi.png'),
(23, 6, N'Trà Đào Hồng Đài', 1, 'tra-dao-hong-dai.png'),
(24, 6, N'Trà Quýt', 1, 'tra-quyt.png'),
(25, 6, N'Cóc Cóc Đác Đác', 1, 'coc-coc-dac-dac.png'),
(26, 6, N'Lệ Chi Thanh Trà', 1, 'le-chi-thanh-tra.png'),
(27, 7, N'Socola', 1, 'so-co-la.png'),
(28, 7, N'Sữa Chua Tằm', 1, 'sua-chua-tam.png'),
(29, 8, N'Bánh Chuối', 1, 'banh-chuoi.png'),
(30, 8, N'Bánh Tráng Muối Tỏi', 1, 'banh-trang-muoi-toi.png'),
(31, 8, N'Bánh Tráng Tôm Hành', 1, 'banh-trang-tom-hanh.png'),
(32, 8, N'Bông Lan Trứng Muối', 1, 'bong-lan-trung-muoi.png'),
(33, 8, N'Bông Lan Trứng Muối Dầu Trứng', 1, 'bong-lan-trung-muoi-dau-trung.png'),
(34, 8, N'Crème Flan', 1, 'creme-plan.png'),
(35, 8, N'Bánh Flan', 1, 'plan.png'),
(36, 8, N'Bánh Phô Mai Chanh Dây', 1, 'pho-mai-chanh-day.png'),
(37, 8, N'Bánh Phô Mai Tan Chảy', 1, 'pho-mai-tan-chay.png'),
(38, 8, N'Bánh Phô Mai Tom & Jerry', 1, 'tom-and-jerry.png'),
(39, 9, N'Huyền Châu', 1, 'huyen-chau.png'),
(40, 9, N'Phô Mai Dẻo', 1, 'pho-mai-deo.png'),
(41, 9, N'Tofu', 1, 'tofu.png'),
(42, 9, N'Thạch Hồng Đài', 1, 'thach-hong-dai.png'),
(43, 9, N'Thốt Nốt', 1, 'thot-not.png'),
(44, 9, N'Trân Châu Dừa', 1, 'tran-chau-dua.png'),
(45, 9, N'Trân Châu Matcha', 1, 'tran-chau-matcha.png'),
(46, 9, N'Trân Châu Trắng', 1, 'tranchau-trang.png'),
(47, 7, N'Bơ Già Dừa Non', 1, 'bo-dua-non.png'),
(48, 7, N'Bơ Sáp Khoai Môn Macchiato', 1, 'bo-khoaimon.png');
SET IDENTITY_INSERT SanPham OFF;
GO

SET IDENTITY_INSERT BienTheSanPham ON;
INSERT INTO BienTheSanPham (MaBienThe, MaSP, KichCo, GiaBan, TrangThai) VALUES
(1, 1, 'M', 29000, 1),
(2, 1, 'L', 35000, 1),
(3, 2, 'M', 32000, 1),
(4, 2, 'L', 39000, 1),
(5, 3, 'M', 32000, 1),
(6, 3, 'L', 39000, 1),
(7, 4, 'M', 39000, 1),
(8, 4, 'L', 45000, 1),
(9, 5, 'M', 35000, 1),
(10, 5, 'L', 42000, 1),
(11, 6, 'M', 32000, 1),
(12, 6, 'L', 39000, 1),
(13, 7, 'M', 35000, 1),
(14, 7, 'L', 42000, 1),
(15, 8, 'M', 38000, 1),
(16, 8, 'L', 45000, 1),
(17, 9, 'M', 45000, 1),
(18, 9, 'L', 52000, 1),
(19, 10, 'M', 49000, 1),
(20, 10, 'L', 56000, 1),
(21, 11, 'M', 45000, 1),
(22, 11, 'L', 52000, 1),
(23, 12, 'M', 45000, 1),
(24, 12, 'L', 52000, 1),
(25, 13, 'M', 49000, 1),
(26, 13, 'L', 56000, 1),
(27, 14, 'M', 45000, 1),
(28, 14, 'L', 55000, 1),
(29, 15, 'M', 49000, 1),
(30, 15, 'L', 59000, 1),
(31, 16, 'M', 49000, 1),
(32, 16, 'L', 59000, 1),
(33, 17, 'M', 52000, 1),
(34, 17, 'L', 62000, 1),
(35, 18, 'M', 39000, 1),
(36, 18, 'L', 49000, 1),
(37, 19, 'M', 39000, 1),
(38, 19, 'L', 49000, 1),
(39, 20, 'M', 45000, 1),
(40, 20, 'L', 55000, 1),
(41, 21, 'M', 42000, 1),
(42, 21, 'L', 52000, 1),
(43, 22, 'M', 45000, 1),
(44, 22, 'L', 55000, 1),
(45, 23, 'M', 39000, 1),
(46, 23, 'L', 49000, 1),
(47, 24, 'M', 39000, 1),
(48, 24, 'L', 49000, 1),
(49, 25, 'M', 45000, 1),
(50, 25, 'L', 55000, 1),
(51, 26, 'M', 42000, 1),
(52, 26, 'L', 52000, 1),
(53, 27, 'M', 42000, 1),
(54, 27, 'L', 52000, 1),
(55, 28, 'M', 45000, 1),
(56, 28, 'L', 55000, 1),
(57, 29, N'Mặc định', 25000, 1),
(58, 30, N'Mặc định', 22000, 1),
(59, 31, N'Mặc định', 25000, 1),
(60, 32, N'Mặc định', 35000, 1),
(61, 33, N'Mặc định', 38000, 1),
(62, 34, N'Mặc định', 29000, 1),
(63, 35, N'Mặc định', 25000, 1),
(64, 36, N'Mặc định', 39000, 1),
(65, 37, N'Mặc định', 42000, 1),
(66, 38, N'Mặc định', 45000, 1),
(67, 39, N'Mặc định', 10000, 1),
(68, 40, N'Mặc định', 12000, 1),
(69, 41, N'Mặc định', 12000, 1),
(70, 42, N'Mặc định', 10000, 1),
(71, 43, N'Mặc định', 12000, 1),
(72, 44, N'Mặc định', 10000, 1),
(73, 45, N'Mặc định', 10000, 1),
(74, 46, N'Mặc định', 10000, 1),
(75, 47, 'M', 47000, 1),
(76, 47, 'L', 69000, 1),
(77, 48, 'M', 45000, 1);
SET IDENTITY_INSERT BienTheSanPham OFF;
GO

SET IDENTITY_INSERT VaiTro ON;
INSERT INTO VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
(1, N'Admin', N'Quản trị toàn quyền hệ thống'),
(2, N'NhanVien', N'Nhân viên bán hàng & pha chế');
SET IDENTITY_INSERT VaiTro OFF;
GO

SET IDENTITY_INSERT TaiKhoan ON;
-- Mật khẩu mặc định: admin123
INSERT INTO TaiKhoan (MaTaiKhoan, MaVaiTro, TenDangNhap, MatKhau, HoTen, TrangThai) VALUES
(1, 1, 'admin', 'admin123', N'Quản trị viên', 1);
SET IDENTITY_INSERT TaiKhoan OFF;
GO

SET IDENTITY_INSERT KhachHang ON;
-- Mật khẩu mặc định: 123456
INSERT INTO KhachHang (MaKH, TaiKhoan, MatKhau, HoTen, Sdt, Email, DiemTichLuy, HangTV, NgayTao) VALUES
(1, 'user01', '123456', N'Nguyễn Văn A', '0909090909', 'user01@gmail.com', 50, N'Bạc', GETDATE()),
(2, 'user02', '123456', N'Trần Thị B', '0912345678', 'user02@gmail.com', 10, N'Đồng', GETDATE());
SET IDENTITY_INSERT KhachHang OFF;
GO

SET IDENTITY_INSERT Voucher ON;
INSERT INTO Voucher (MaVoucher, MaCode, PhanLoaiGiam, GiaTriGiam, TenChuongTrinh, NgayBatDau, NgayKetThuc, DonToiThieu, TrangThai) VALUES
(1, 'COFFEE10', 'PERCENT', 10.00, N'Ưu đãi thành viên', DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 100000, 1),
(2, 'COFFEE20', 'PERCENT', 20.00, N'Khuyến mãi cuối tuần', DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 300000, 1),
(3, 'GIAM30K', 'AMOUNT', 30000.00, N'Giảm trực tiếp', DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 500000, 1);
SET IDENTITY_INSERT Voucher OFF;
GO

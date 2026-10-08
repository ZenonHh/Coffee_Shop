-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 08, 2026 at 04:02 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.1.25

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `coffee_shop_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `bienthesanpham`
--

CREATE TABLE `bienthesanpham` (
  `MaBienThe` int(11) NOT NULL,
  `MaSP` int(11) NOT NULL,
  `KichCo` varchar(10) NOT NULL,
  `GiaBan` decimal(12,2) NOT NULL,
  `TrangThai` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `bienthesanpham`
--

INSERT INTO `bienthesanpham` (`MaBienThe`, `MaSP`, `KichCo`, `GiaBan`, `TrangThai`) VALUES
(1, 1, 'M', 29000.00, 1),
(2, 1, 'L', 35000.00, 1),
(3, 2, 'M', 32000.00, 1),
(4, 2, 'L', 39000.00, 1),
(5, 3, 'M', 32000.00, 1),
(6, 3, 'L', 39000.00, 1),
(7, 4, 'M', 39000.00, 1),
(8, 4, 'L', 45000.00, 1),
(9, 5, 'M', 35000.00, 1),
(10, 5, 'L', 42000.00, 1),
(11, 6, 'M', 32000.00, 1),
(12, 6, 'L', 39000.00, 1),
(13, 7, 'M', 35000.00, 1),
(14, 7, 'L', 42000.00, 1),
(15, 8, 'M', 38000.00, 1),
(16, 8, 'L', 45000.00, 1),
(17, 9, 'M', 45000.00, 1),
(18, 9, 'L', 52000.00, 1),
(19, 10, 'M', 49000.00, 1),
(20, 10, 'L', 56000.00, 1),
(21, 11, 'M', 45000.00, 1),
(22, 11, 'L', 52000.00, 1),
(23, 12, 'M', 45000.00, 1),
(24, 12, 'L', 52000.00, 1),
(25, 13, 'M', 49000.00, 1),
(26, 13, 'L', 56000.00, 1),
(27, 14, 'M', 45000.00, 1),
(28, 14, 'L', 55000.00, 1),
(29, 15, 'M', 49000.00, 1),
(30, 15, 'L', 59000.00, 1),
(31, 16, 'M', 49000.00, 1),
(32, 16, 'L', 59000.00, 1),
(33, 17, 'M', 52000.00, 1),
(34, 17, 'L', 62000.00, 1),
(35, 18, 'M', 39000.00, 1),
(36, 18, 'L', 49000.00, 1),
(37, 19, 'M', 39000.00, 1),
(38, 19, 'L', 49000.00, 1),
(39, 20, 'M', 45000.00, 1),
(40, 20, 'L', 55000.00, 1),
(41, 21, 'M', 42000.00, 1),
(42, 21, 'L', 52000.00, 1),
(43, 22, 'M', 45000.00, 1),
(44, 22, 'L', 55000.00, 1),
(45, 23, 'M', 39000.00, 1),
(46, 23, 'L', 49000.00, 1),
(47, 24, 'M', 39000.00, 1),
(48, 24, 'L', 49000.00, 1),
(49, 25, 'M', 45000.00, 1),
(50, 25, 'L', 55000.00, 1),
(51, 26, 'M', 42000.00, 1),
(52, 26, 'L', 52000.00, 1),
(53, 27, 'M', 42000.00, 1),
(54, 27, 'L', 52000.00, 1),
(55, 28, 'M', 45000.00, 1),
(56, 28, 'L', 55000.00, 1),
(57, 29, 'Mặc định', 25000.00, 1),
(58, 30, 'Mặc định', 22000.00, 1),
(59, 31, 'Mặc định', 25000.00, 1),
(60, 32, 'Mặc định', 35000.00, 1),
(61, 33, 'Mặc định', 38000.00, 1),
(62, 34, 'Mặc định', 29000.00, 1),
(63, 35, 'Mặc định', 25000.00, 1),
(64, 36, 'Mặc định', 39000.00, 1),
(65, 37, 'Mặc định', 42000.00, 1),
(66, 38, 'Mặc định', 45000.00, 1),
(67, 39, 'Mặc định', 10000.00, 1),
(68, 40, 'Mặc định', 12000.00, 1),
(69, 41, 'Mặc định', 12000.00, 1),
(70, 42, 'Mặc định', 10000.00, 1),
(71, 43, 'Mặc định', 12000.00, 1),
(72, 44, 'Mặc định', 10000.00, 1),
(73, 45, 'Mặc định', 10000.00, 1),
(74, 46, 'Mặc định', 10000.00, 1),
(75, 47, 'M', 47000.00, 1),
(76, 47, 'L', 69000.00, 1),
(77, 48, 'M', 45000.00, 1),
(78, 1, 'L', 65000.00, 1);

-- --------------------------------------------------------

--
-- Table structure for table `calam`
--

CREATE TABLE `calam` (
  `MaCaLam` int(11) NOT NULL,
  `MaNV` int(11) NOT NULL,
  `GioMoCa` datetime DEFAULT current_timestamp(),
  `GioDongCa` datetime DEFAULT NULL,
  `TienDauCa` decimal(12,2) DEFAULT 0.00,
  `TienCuoiCa` decimal(12,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `cauhoikhaosat`
--

CREATE TABLE `cauhoikhaosat` (
  `MaCauHoi` int(11) NOT NULL,
  `MaKhaoSat` int(11) NOT NULL,
  `NoiDungCauHoi` varchar(255) NOT NULL,
  `LoaiCauHoi` varchar(50) DEFAULT 'TRAC_NGHIEM'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `cautraloi`
--

CREATE TABLE `cautraloi` (
  `MaCauTraLoi` int(11) NOT NULL,
  `MaLoiMoi` int(11) NOT NULL,
  `MaCauHoi` int(11) NOT NULL,
  `MaDanhMuc` int(11) DEFAULT NULL,
  `NoiDungTraLoi` text DEFAULT NULL,
  `NgayTraLoi` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `chitietgiohang`
--

CREATE TABLE `chitietgiohang` (
  `MaCTGH` int(11) NOT NULL,
  `MaGioHang` int(11) NOT NULL,
  `MaSP` int(11) NOT NULL,
  `SoLuong` int(11) DEFAULT 1,
  `TongTien` decimal(12,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `chitiethoadon`
--

CREATE TABLE `chitiethoadon` (
  `MaCTHD` int(11) NOT NULL,
  `MaHD` int(11) NOT NULL,
  `MaSP` int(11) NOT NULL,
  `SoLuong` int(11) NOT NULL,
  `GiaBan` decimal(12,2) NOT NULL,
  `ThanhTien` decimal(12,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `chitietkm`
--

CREATE TABLE `chitietkm` (
  `MaCTKM` int(11) NOT NULL,
  `TenCTKM` varchar(150) NOT NULL,
  `NgayBatDau` datetime NOT NULL,
  `NgayKetThuc` datetime NOT NULL,
  `MoTa` varchar(255) DEFAULT NULL,
  `TrangThai` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `chitietphieunhap`
--

CREATE TABLE `chitietphieunhap` (
  `MaCTPN` int(11) NOT NULL,
  `MaPN` int(11) NOT NULL,
  `MaNL` int(11) NOT NULL,
  `SoLuong` decimal(10,2) NOT NULL,
  `DonGia` decimal(12,2) NOT NULL,
  `ThanhTien` decimal(12,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `ctkm_theo_hoadon`
--

CREATE TABLE `ctkm_theo_hoadon` (
  `MaKMDon` int(11) NOT NULL,
  `MaCTKM` int(11) NOT NULL,
  `DonToiThieu` decimal(12,2) NOT NULL,
  `KieuGiam` varchar(10) DEFAULT 'AMOUNT',
  `GiaTriGiam` decimal(12,2) NOT NULL,
  `GiamToiDa` decimal(12,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `ctkm_theo_sp`
--

CREATE TABLE `ctkm_theo_sp` (
  `MaKMSP` int(11) NOT NULL,
  `MaCTKM` int(11) NOT NULL,
  `MaSP` int(11) NOT NULL,
  `KieuGiam` varchar(10) DEFAULT 'PERCENT',
  `GiaTriGiam` decimal(12,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `danhmucluachon`
--

CREATE TABLE `danhmucluachon` (
  `MaDanhMuc` int(11) NOT NULL,
  `MaCauHoi` int(11) NOT NULL,
  `NoiDungLuaChon` varchar(255) NOT NULL,
  `ThuTuHienThi` int(11) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `giohang`
--

CREATE TABLE `giohang` (
  `MaGioHang` int(11) NOT NULL,
  `MaKH` int(11) NOT NULL,
  `NgayTao` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `hoadon`
--

CREATE TABLE `hoadon` (
  `MaHD` int(11) NOT NULL,
  `MaCaLam` int(11) DEFAULT NULL,
  `MaKH` int(11) DEFAULT NULL,
  `MaNV` int(11) DEFAULT NULL,
  `MaCTKM` int(11) DEFAULT NULL,
  `LoaiDonHang` varchar(50) DEFAULT 'Tại quán',
  `TongTien` decimal(12,2) DEFAULT 0.00,
  `TienGiamGia` decimal(12,2) DEFAULT 0.00,
  `TrangThai` varchar(50) DEFAULT 'Đã thanh toán',
  `NgayTao` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `khachhang`
--

CREATE TABLE `khachhang` (
  `MaKH` int(11) NOT NULL,
  `TaiKhoan` varchar(50) DEFAULT NULL,
  `MatKhau` varchar(255) DEFAULT NULL,
  `HoTen` varchar(100) NOT NULL,
  `Sdt` varchar(20) NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `DiemTichLuy` int(11) DEFAULT 0,
  `HangTV` varchar(20) DEFAULT 'Đồng',
  `NgayTao` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `khachhang`
--

INSERT INTO `khachhang` (`MaKH`, `TaiKhoan`, `MatKhau`, `HoTen`, `Sdt`, `Email`, `DiemTichLuy`, `HangTV`, `NgayTao`) VALUES
(1, 'user01', '$2y$10$x0gqtT./APiYudtODvg/IewGSv2PxjNDxDWQVq8GuMcz7mGeL8Yxy', 'nguyen van a', '0909090909', '', 0, 'Đồng', '2026-10-06 14:44:33');

-- --------------------------------------------------------

--
-- Table structure for table `khaosat`
--

CREATE TABLE `khaosat` (
  `MaKhaoSat` int(11) NOT NULL,
  `MaTaiKhoan` int(11) NOT NULL,
  `TieuDe` varchar(255) NOT NULL,
  `MoTa` text DEFAULT NULL,
  `NgayBatDau` datetime NOT NULL,
  `NgayKetThuc` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `lichsudiem`
--

CREATE TABLE `lichsudiem` (
  `MaLSD` int(11) NOT NULL,
  `MaHD` int(11) DEFAULT NULL,
  `MaKH` int(11) NOT NULL,
  `DiemThayDoi` int(11) NOT NULL,
  `LyDo` varchar(255) DEFAULT NULL,
  `NgayThucHien` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `loaisanpham`
--

CREATE TABLE `loaisanpham` (
  `MaLoaiSP` int(11) NOT NULL,
  `TenLoaiSP` varchar(100) NOT NULL,
  `ThuTuHienThi` int(11) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `loaisanpham`
--

INSERT INTO `loaisanpham` (`MaLoaiSP`, `TenLoaiSP`, `ThuTuHienThi`) VALUES
(1, 'Cà Phê Espresso', 10),
(2, 'Cà Phê Phin Mê', 20),
(3, 'Cold Brew', 30),
(4, 'Matcha', 40),
(5, 'Trà Sữa', 50),
(6, 'Trà Trái Cây', 60),
(7, 'Fruit Boost', 70),
(8, 'Bánh Ngọt', 80),
(9, 'Topping', 90);

-- --------------------------------------------------------

--
-- Table structure for table `loimoikhaosat`
--

CREATE TABLE `loimoikhaosat` (
  `MaLoiMoi` int(11) NOT NULL,
  `MaKhaoSat` int(11) NOT NULL,
  `MaKH` int(11) NOT NULL,
  `ThoiGianGui` datetime DEFAULT current_timestamp(),
  `TrangThai` varchar(50) DEFAULT 'Chưa làm'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `nguyenlieu`
--

CREATE TABLE `nguyenlieu` (
  `MaNL` int(11) NOT NULL,
  `TenNL` varchar(100) NOT NULL,
  `DonViTinh` varchar(20) DEFAULT NULL,
  `GiaVon` decimal(12,2) DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `nhacungcap`
--

CREATE TABLE `nhacungcap` (
  `MaNCC` int(11) NOT NULL,
  `TenNCC` varchar(150) NOT NULL,
  `Sdt` varchar(20) DEFAULT NULL,
  `DiaChi` varchar(255) DEFAULT NULL,
  `TrangThai` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `nhanvien`
--

CREATE TABLE `nhanvien` (
  `MaNV` int(11) NOT NULL,
  `HoVaDem` varchar(50) NOT NULL,
  `Ten` varchar(20) NOT NULL,
  `Sdt` varchar(20) DEFAULT NULL,
  `ChucVu` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `nhatkyhoatdong`
--

CREATE TABLE `nhatkyhoatdong` (
  `MaNhatKy` int(11) NOT NULL,
  `MaTaiKhoan` int(11) NOT NULL,
  `HanhDong` varchar(255) NOT NULL,
  `ThoiGian` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `phanhoi`
--

CREATE TABLE `phanhoi` (
  `MaPhanHoi` int(11) NOT NULL,
  `MaKH` int(11) NOT NULL,
  `MaSP` int(11) DEFAULT NULL,
  `NoiDung` text NOT NULL,
  `DanhGia` int(11) DEFAULT NULL CHECK (`DanhGia` between 1 and 5),
  `NgayGui` datetime DEFAULT current_timestamp(),
  `TrangThaiXL` varchar(50) DEFAULT 'Chờ xử lý'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `phieunhap`
--

CREATE TABLE `phieunhap` (
  `MaPN` int(11) NOT NULL,
  `MaNCC` int(11) NOT NULL,
  `MaNV` int(11) NOT NULL,
  `NgayNhap` datetime DEFAULT current_timestamp(),
  `TongTien` decimal(12,2) DEFAULT 0.00,
  `GhiChu` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `sanpham`
--

CREATE TABLE `sanpham` (
  `MaSP` int(11) NOT NULL,
  `MaLoaiSP` int(11) NOT NULL,
  `TenSP` varchar(100) NOT NULL,
  `TrangThai` tinyint(1) DEFAULT 1,
  `HinhAnh` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `sanpham`
--

INSERT INTO `sanpham` (`MaSP`, `MaLoaiSP`, `TenSP`, `TrangThai`, `HinhAnh`) VALUES
(1, 2, 'Phin Mê Đen', 1, 'me-den.png'),
(2, 2, 'Phin Mê Sữa', 1, 'me-sua.png'),
(3, 2, 'Phin Mê Xỉu', 1, 'me-xiu.png'),
(4, 2, 'Phin Mê Dừa Non', 1, 'me-dua-non.png'),
(5, 1, 'Americano', 1, 'americano.png'),
(6, 1, 'Espresso Đen Đá', 1, 'espresso-den-da.png'),
(7, 1, 'Espresso Sữa Đá', 1, 'espresso-sua-da.png'),
(8, 1, 'Espresso Bạc Xỉu', 1, 'espresso-bac-xiu.png'),
(9, 1, 'Latte Nguyên Bản', 1, 'latte-nguyen-ban.png'),
(10, 1, 'Cà Phê Kem Mây Chanh Vàng', 1, 'cafe-kem-may-chanh-vang.png'),
(11, 3, 'Cold Brew Chanh', 1, 'cold-brew-chanh.png'),
(12, 3, 'Cold Brew Quýt', 1, 'cold-brew-quyt.png'),
(13, 3, 'Cold Brew Tằm', 1, 'cold-brew-tam.png'),
(14, 4, 'Matcha Latte', 1, 'matcha-latte.png'),
(15, 4, 'Matcha Dừa Xiêm', 1, 'matcha-dua-xiem.png'),
(16, 4, 'Matcha Bắp Bắp', 1, 'matcha-bap-bi.png'),
(17, 4, 'Matcha Tofu', 1, 'matcha-tofu.png'),
(18, 5, 'Trà Sữa Oolong Nướng', 1, 'tra-sua-oolong-nuong.png'),
(19, 5, 'Trà Sữa Oolong Ba Lá', 1, 'tra-sua-oolong-ba-la.png'),
(20, 5, 'Trà Sữa Chôm Chôm', 1, 'tra-sua-chom-chom.png'),
(21, 5, 'Trà Sữa Thanh Hương', 1, 'tra-sua-thanh-huong.png'),
(22, 5, 'Nhài Sữa Dẻ Cười', 1, 'nhai-sua-de-cuoi.png'),
(23, 6, 'Trà Đào Hồng Đài', 1, 'tra-dao-hong-dai.png'),
(24, 6, 'Trà Quýt', 1, 'tra-quyt.png'),
(25, 6, 'Cóc Cóc Đác Đác', 1, 'coc-coc-dac-dac.png'),
(26, 6, 'Lệ Chi Thanh Trà', 1, 'le-chi-thanh-tra.png'),
(27, 7, 'Socola', 1, 'so-co-la.png'),
(28, 7, 'Sữa Chua Tằm', 1, 'sua-chua-tam.png'),
(29, 8, 'Bánh Chuối', 1, 'banh-chuoi.png'),
(30, 8, 'Bánh Tráng Muối Tỏi', 1, 'banh-trang-muoi-toi.png'),
(31, 8, 'Bánh Tráng Tôm Hành', 1, 'banh-trang-tom-hanh.png'),
(32, 8, 'Bông Lan Trứng Muối', 1, 'bong-lan-trung-muoi.png'),
(33, 8, 'Bông Lan Trứng Muối Dầu Trứng', 1, 'bong-lan-trung-muoi-dau-trung.png'),
(34, 8, 'Crème Flan', 1, 'creme-plan.png'),
(35, 8, 'Bánh Flan', 1, 'plan.png'),
(36, 8, 'Bánh Phô Mai Chanh Dây', 1, 'pho-mai-chanh-day.png'),
(37, 8, 'Bánh Phô Mai Tan Chảy', 1, 'pho-mai-tan-chay.png'),
(38, 8, 'Bánh Phô Mai Tom & Jerry', 1, 'tom-and-jerry.png'),
(39, 9, 'Huyền Châu', 1, 'huyen-chau.png'),
(40, 9, 'Phô Mai Dẻo', 1, 'pho-mai-deo.png'),
(41, 9, 'Tofu', 1, 'tofu.png'),
(42, 9, 'Thạch Hồng Đài', 1, 'thach-hong-dai.png'),
(43, 9, 'Thốt Nốt', 1, 'thot-not.png'),
(44, 9, 'Trân Châu Dừa', 1, 'tran-chau-dua.png'),
(45, 9, 'Trân Châu Matcha', 1, 'tran-chau-matcha.png'),
(46, 9, 'Trân Châu Trắng', 1, 'tranchau-trang.png'),
(47, 7, 'Bơ Già Dừa Non', 1, 'bo-dua-non.png'),
(48, 7, ' Bơ Sáp Khoai Môn Macchiato', 1, 'bo-khoaimon.png');

-- --------------------------------------------------------

--
-- Table structure for table `taikhoan`
--

CREATE TABLE `taikhoan` (
  `MaTaiKhoan` int(11) NOT NULL,
  `MaVaiTro` int(11) NOT NULL,
  `TenDangNhap` varchar(50) NOT NULL,
  `MatKhau` varchar(255) NOT NULL,
  `HoTen` varchar(100) NOT NULL,
  `TrangThai` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `thanhtoan`
--

CREATE TABLE `thanhtoan` (
  `MaThanhToan` int(11) NOT NULL,
  `MaHD` int(11) NOT NULL,
  `PhuongThuc` varchar(50) DEFAULT 'Tiền mặt',
  `SoTien` decimal(12,2) NOT NULL,
  `MaGiaoDich` varchar(100) DEFAULT NULL,
  `ThoiGian` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `vaitro`
--

CREATE TABLE `vaitro` (
  `MaVaiTro` int(11) NOT NULL,
  `TenVaiTro` varchar(50) NOT NULL,
  `MoTa` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `voucher`
--

CREATE TABLE `voucher` (
  `MaVoucher` int(11) NOT NULL,
  `MaCode` varchar(50) NOT NULL,
  `PhanLoaiGiam` varchar(20) DEFAULT 'PERCENT',
  `GiaTriGiam` decimal(12,2) NOT NULL,
  `TrangThai` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `bienthesanpham`
--
ALTER TABLE `bienthesanpham`
  ADD PRIMARY KEY (`MaBienThe`),
  ADD KEY `fk_bienthe_sanpham` (`MaSP`);

--
-- Indexes for table `calam`
--
ALTER TABLE `calam`
  ADD PRIMARY KEY (`MaCaLam`),
  ADD KEY `fk_calam_nhanvien` (`MaNV`);

--
-- Indexes for table `cauhoikhaosat`
--
ALTER TABLE `cauhoikhaosat`
  ADD PRIMARY KEY (`MaCauHoi`),
  ADD KEY `fk_cauhoi_khaosat` (`MaKhaoSat`);

--
-- Indexes for table `cautraloi`
--
ALTER TABLE `cautraloi`
  ADD PRIMARY KEY (`MaCauTraLoi`),
  ADD KEY `fk_cautraloi_loimoi` (`MaLoiMoi`),
  ADD KEY `fk_cautraloi_cauhoi` (`MaCauHoi`),
  ADD KEY `fk_cautraloi_danhmuc` (`MaDanhMuc`);

--
-- Indexes for table `chitietgiohang`
--
ALTER TABLE `chitietgiohang`
  ADD PRIMARY KEY (`MaCTGH`),
  ADD KEY `fk_ctgh_giohang` (`MaGioHang`),
  ADD KEY `fk_ctgh_sanpham` (`MaSP`);

--
-- Indexes for table `chitiethoadon`
--
ALTER TABLE `chitiethoadon`
  ADD PRIMARY KEY (`MaCTHD`),
  ADD KEY `fk_cthd_hoadon` (`MaHD`),
  ADD KEY `fk_cthd_sanpham` (`MaSP`);

--
-- Indexes for table `chitietkm`
--
ALTER TABLE `chitietkm`
  ADD PRIMARY KEY (`MaCTKM`);

--
-- Indexes for table `chitietphieunhap`
--
ALTER TABLE `chitietphieunhap`
  ADD PRIMARY KEY (`MaCTPN`),
  ADD KEY `fk_ctpn_phieunhap` (`MaPN`),
  ADD KEY `fk_ctpn_nguyenlieu` (`MaNL`);

--
-- Indexes for table `ctkm_theo_hoadon`
--
ALTER TABLE `ctkm_theo_hoadon`
  ADD PRIMARY KEY (`MaKMDon`),
  ADD KEY `fk_ktkm_hoadon_ctkm` (`MaCTKM`);

--
-- Indexes for table `ctkm_theo_sp`
--
ALTER TABLE `ctkm_theo_sp`
  ADD PRIMARY KEY (`MaKMSP`),
  ADD KEY `fk_ctkm_sp_ctkm` (`MaCTKM`),
  ADD KEY `fk_ctkm_sp_sanpham` (`MaSP`);

--
-- Indexes for table `danhmucluachon`
--
ALTER TABLE `danhmucluachon`
  ADD PRIMARY KEY (`MaDanhMuc`),
  ADD KEY `fk_danhmuc_cauhoi` (`MaCauHoi`);

--
-- Indexes for table `giohang`
--
ALTER TABLE `giohang`
  ADD PRIMARY KEY (`MaGioHang`),
  ADD KEY `fk_giohang_khachhang` (`MaKH`);

--
-- Indexes for table `hoadon`
--
ALTER TABLE `hoadon`
  ADD PRIMARY KEY (`MaHD`),
  ADD KEY `fk_hoadon_calam` (`MaCaLam`),
  ADD KEY `fk_hoadon_khachhang` (`MaKH`),
  ADD KEY `fk_hoadon_nhanvien` (`MaNV`),
  ADD KEY `fk_hoadon_ctkm` (`MaCTKM`);

--
-- Indexes for table `khachhang`
--
ALTER TABLE `khachhang`
  ADD PRIMARY KEY (`MaKH`),
  ADD UNIQUE KEY `Sdt` (`Sdt`),
  ADD UNIQUE KEY `TaiKhoan` (`TaiKhoan`),
  ADD UNIQUE KEY `Email` (`Email`);

--
-- Indexes for table `khaosat`
--
ALTER TABLE `khaosat`
  ADD PRIMARY KEY (`MaKhaoSat`),
  ADD KEY `fk_khaosat_taikhoan` (`MaTaiKhoan`);

--
-- Indexes for table `lichsudiem`
--
ALTER TABLE `lichsudiem`
  ADD PRIMARY KEY (`MaLSD`),
  ADD KEY `fk_lsd_hoadon` (`MaHD`),
  ADD KEY `fk_lsd_khachhang` (`MaKH`);

--
-- Indexes for table `loaisanpham`
--
ALTER TABLE `loaisanpham`
  ADD PRIMARY KEY (`MaLoaiSP`);

--
-- Indexes for table `loimoikhaosat`
--
ALTER TABLE `loimoikhaosat`
  ADD PRIMARY KEY (`MaLoiMoi`),
  ADD KEY `fk_loimoi_khaosat` (`MaKhaoSat`),
  ADD KEY `fk_loimoi_khachhang` (`MaKH`);

--
-- Indexes for table `nguyenlieu`
--
ALTER TABLE `nguyenlieu`
  ADD PRIMARY KEY (`MaNL`);

--
-- Indexes for table `nhacungcap`
--
ALTER TABLE `nhacungcap`
  ADD PRIMARY KEY (`MaNCC`);

--
-- Indexes for table `nhanvien`
--
ALTER TABLE `nhanvien`
  ADD PRIMARY KEY (`MaNV`);

--
-- Indexes for table `nhatkyhoatdong`
--
ALTER TABLE `nhatkyhoatdong`
  ADD PRIMARY KEY (`MaNhatKy`),
  ADD KEY `fk_nhatky_taikhoan` (`MaTaiKhoan`);

--
-- Indexes for table `phanhoi`
--
ALTER TABLE `phanhoi`
  ADD PRIMARY KEY (`MaPhanHoi`),
  ADD KEY `fk_phanhoi_khachhang` (`MaKH`),
  ADD KEY `fk_phanhoi_sanpham` (`MaSP`);

--
-- Indexes for table `phieunhap`
--
ALTER TABLE `phieunhap`
  ADD PRIMARY KEY (`MaPN`),
  ADD KEY `fk_phieunhap_ncc` (`MaNCC`),
  ADD KEY `fk_phieunhap_nhanvien` (`MaNV`);

--
-- Indexes for table `sanpham`
--
ALTER TABLE `sanpham`
  ADD PRIMARY KEY (`MaSP`),
  ADD KEY `fk_sanpham_loaisp` (`MaLoaiSP`);

--
-- Indexes for table `taikhoan`
--
ALTER TABLE `taikhoan`
  ADD PRIMARY KEY (`MaTaiKhoan`),
  ADD UNIQUE KEY `TenDangNhap` (`TenDangNhap`),
  ADD KEY `fk_taikhoan_vaitro` (`MaVaiTro`);

--
-- Indexes for table `thanhtoan`
--
ALTER TABLE `thanhtoan`
  ADD PRIMARY KEY (`MaThanhToan`),
  ADD KEY `fk_thanhtoan_hoadon` (`MaHD`);

--
-- Indexes for table `vaitro`
--
ALTER TABLE `vaitro`
  ADD PRIMARY KEY (`MaVaiTro`);

--
-- Indexes for table `voucher`
--
ALTER TABLE `voucher`
  ADD PRIMARY KEY (`MaVoucher`),
  ADD UNIQUE KEY `MaCode` (`MaCode`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `bienthesanpham`
--
ALTER TABLE `bienthesanpham`
  MODIFY `MaBienThe` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=79;

--
-- AUTO_INCREMENT for table `calam`
--
ALTER TABLE `calam`
  MODIFY `MaCaLam` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `cauhoikhaosat`
--
ALTER TABLE `cauhoikhaosat`
  MODIFY `MaCauHoi` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `cautraloi`
--
ALTER TABLE `cautraloi`
  MODIFY `MaCauTraLoi` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `chitietgiohang`
--
ALTER TABLE `chitietgiohang`
  MODIFY `MaCTGH` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `chitiethoadon`
--
ALTER TABLE `chitiethoadon`
  MODIFY `MaCTHD` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `chitietkm`
--
ALTER TABLE `chitietkm`
  MODIFY `MaCTKM` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `chitietphieunhap`
--
ALTER TABLE `chitietphieunhap`
  MODIFY `MaCTPN` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `ctkm_theo_hoadon`
--
ALTER TABLE `ctkm_theo_hoadon`
  MODIFY `MaKMDon` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `ctkm_theo_sp`
--
ALTER TABLE `ctkm_theo_sp`
  MODIFY `MaKMSP` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `danhmucluachon`
--
ALTER TABLE `danhmucluachon`
  MODIFY `MaDanhMuc` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `giohang`
--
ALTER TABLE `giohang`
  MODIFY `MaGioHang` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `hoadon`
--
ALTER TABLE `hoadon`
  MODIFY `MaHD` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `khachhang`
--
ALTER TABLE `khachhang`
  MODIFY `MaKH` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `khaosat`
--
ALTER TABLE `khaosat`
  MODIFY `MaKhaoSat` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `lichsudiem`
--
ALTER TABLE `lichsudiem`
  MODIFY `MaLSD` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `loaisanpham`
--
ALTER TABLE `loaisanpham`
  MODIFY `MaLoaiSP` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT for table `loimoikhaosat`
--
ALTER TABLE `loimoikhaosat`
  MODIFY `MaLoiMoi` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `nguyenlieu`
--
ALTER TABLE `nguyenlieu`
  MODIFY `MaNL` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `nhacungcap`
--
ALTER TABLE `nhacungcap`
  MODIFY `MaNCC` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `nhanvien`
--
ALTER TABLE `nhanvien`
  MODIFY `MaNV` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `nhatkyhoatdong`
--
ALTER TABLE `nhatkyhoatdong`
  MODIFY `MaNhatKy` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `phanhoi`
--
ALTER TABLE `phanhoi`
  MODIFY `MaPhanHoi` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `phieunhap`
--
ALTER TABLE `phieunhap`
  MODIFY `MaPN` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `sanpham`
--
ALTER TABLE `sanpham`
  MODIFY `MaSP` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=49;

--
-- AUTO_INCREMENT for table `taikhoan`
--
ALTER TABLE `taikhoan`
  MODIFY `MaTaiKhoan` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `thanhtoan`
--
ALTER TABLE `thanhtoan`
  MODIFY `MaThanhToan` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `vaitro`
--
ALTER TABLE `vaitro`
  MODIFY `MaVaiTro` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `voucher`
--
ALTER TABLE `voucher`
  MODIFY `MaVoucher` int(11) NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `bienthesanpham`
--
ALTER TABLE `bienthesanpham`
  ADD CONSTRAINT `fk_bienthe_sanpham` FOREIGN KEY (`MaSP`) REFERENCES `sanpham` (`MaSP`) ON DELETE CASCADE;

--
-- Constraints for table `calam`
--
ALTER TABLE `calam`
  ADD CONSTRAINT `fk_calam_nhanvien` FOREIGN KEY (`MaNV`) REFERENCES `nhanvien` (`MaNV`);

--
-- Constraints for table `cauhoikhaosat`
--
ALTER TABLE `cauhoikhaosat`
  ADD CONSTRAINT `fk_cauhoi_khaosat` FOREIGN KEY (`MaKhaoSat`) REFERENCES `khaosat` (`MaKhaoSat`) ON DELETE CASCADE;

--
-- Constraints for table `cautraloi`
--
ALTER TABLE `cautraloi`
  ADD CONSTRAINT `fk_cautraloi_cauhoi` FOREIGN KEY (`MaCauHoi`) REFERENCES `cauhoikhaosat` (`MaCauHoi`),
  ADD CONSTRAINT `fk_cautraloi_danhmuc` FOREIGN KEY (`MaDanhMuc`) REFERENCES `danhmucluachon` (`MaDanhMuc`),
  ADD CONSTRAINT `fk_cautraloi_loimoi` FOREIGN KEY (`MaLoiMoi`) REFERENCES `loimoikhaosat` (`MaLoiMoi`) ON DELETE CASCADE;

--
-- Constraints for table `chitietgiohang`
--
ALTER TABLE `chitietgiohang`
  ADD CONSTRAINT `fk_ctgh_giohang` FOREIGN KEY (`MaGioHang`) REFERENCES `giohang` (`MaGioHang`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_ctgh_sanpham` FOREIGN KEY (`MaSP`) REFERENCES `sanpham` (`MaSP`);

--
-- Constraints for table `chitiethoadon`
--
ALTER TABLE `chitiethoadon`
  ADD CONSTRAINT `fk_cthd_hoadon` FOREIGN KEY (`MaHD`) REFERENCES `hoadon` (`MaHD`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_cthd_sanpham` FOREIGN KEY (`MaSP`) REFERENCES `sanpham` (`MaSP`);

--
-- Constraints for table `chitietphieunhap`
--
ALTER TABLE `chitietphieunhap`
  ADD CONSTRAINT `fk_ctpn_nguyenlieu` FOREIGN KEY (`MaNL`) REFERENCES `nguyenlieu` (`MaNL`),
  ADD CONSTRAINT `fk_ctpn_phieunhap` FOREIGN KEY (`MaPN`) REFERENCES `phieunhap` (`MaPN`) ON DELETE CASCADE;

--
-- Constraints for table `ctkm_theo_hoadon`
--
ALTER TABLE `ctkm_theo_hoadon`
  ADD CONSTRAINT `fk_ktkm_hoadon_ctkm` FOREIGN KEY (`MaCTKM`) REFERENCES `chitietkm` (`MaCTKM`) ON DELETE CASCADE;

--
-- Constraints for table `ctkm_theo_sp`
--
ALTER TABLE `ctkm_theo_sp`
  ADD CONSTRAINT `fk_ctkm_sp_ctkm` FOREIGN KEY (`MaCTKM`) REFERENCES `chitietkm` (`MaCTKM`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_ctkm_sp_sanpham` FOREIGN KEY (`MaSP`) REFERENCES `sanpham` (`MaSP`);

--
-- Constraints for table `danhmucluachon`
--
ALTER TABLE `danhmucluachon`
  ADD CONSTRAINT `fk_danhmuc_cauhoi` FOREIGN KEY (`MaCauHoi`) REFERENCES `cauhoikhaosat` (`MaCauHoi`) ON DELETE CASCADE;

--
-- Constraints for table `giohang`
--
ALTER TABLE `giohang`
  ADD CONSTRAINT `fk_giohang_khachhang` FOREIGN KEY (`MaKH`) REFERENCES `khachhang` (`MaKH`);

--
-- Constraints for table `hoadon`
--
ALTER TABLE `hoadon`
  ADD CONSTRAINT `fk_hoadon_calam` FOREIGN KEY (`MaCaLam`) REFERENCES `calam` (`MaCaLam`),
  ADD CONSTRAINT `fk_hoadon_ctkm` FOREIGN KEY (`MaCTKM`) REFERENCES `chitietkm` (`MaCTKM`),
  ADD CONSTRAINT `fk_hoadon_khachhang` FOREIGN KEY (`MaKH`) REFERENCES `khachhang` (`MaKH`),
  ADD CONSTRAINT `fk_hoadon_nhanvien` FOREIGN KEY (`MaNV`) REFERENCES `nhanvien` (`MaNV`);

--
-- Constraints for table `khaosat`
--
ALTER TABLE `khaosat`
  ADD CONSTRAINT `fk_khaosat_taikhoan` FOREIGN KEY (`MaTaiKhoan`) REFERENCES `taikhoan` (`MaTaiKhoan`);

--
-- Constraints for table `lichsudiem`
--
ALTER TABLE `lichsudiem`
  ADD CONSTRAINT `fk_lsd_hoadon` FOREIGN KEY (`MaHD`) REFERENCES `hoadon` (`MaHD`),
  ADD CONSTRAINT `fk_lsd_khachhang` FOREIGN KEY (`MaKH`) REFERENCES `khachhang` (`MaKH`);

--
-- Constraints for table `loimoikhaosat`
--
ALTER TABLE `loimoikhaosat`
  ADD CONSTRAINT `fk_loimoi_khachhang` FOREIGN KEY (`MaKH`) REFERENCES `khachhang` (`MaKH`),
  ADD CONSTRAINT `fk_loimoi_khaosat` FOREIGN KEY (`MaKhaoSat`) REFERENCES `khaosat` (`MaKhaoSat`);

--
-- Constraints for table `nhatkyhoatdong`
--
ALTER TABLE `nhatkyhoatdong`
  ADD CONSTRAINT `fk_nhatky_taikhoan` FOREIGN KEY (`MaTaiKhoan`) REFERENCES `taikhoan` (`MaTaiKhoan`);

--
-- Constraints for table `phanhoi`
--
ALTER TABLE `phanhoi`
  ADD CONSTRAINT `fk_phanhoi_khachhang` FOREIGN KEY (`MaKH`) REFERENCES `khachhang` (`MaKH`),
  ADD CONSTRAINT `fk_phanhoi_sanpham` FOREIGN KEY (`MaSP`) REFERENCES `sanpham` (`MaSP`);

--
-- Constraints for table `phieunhap`
--
ALTER TABLE `phieunhap`
  ADD CONSTRAINT `fk_phieunhap_ncc` FOREIGN KEY (`MaNCC`) REFERENCES `nhacungcap` (`MaNCC`),
  ADD CONSTRAINT `fk_phieunhap_nhanvien` FOREIGN KEY (`MaNV`) REFERENCES `nhanvien` (`MaNV`);

--
-- Constraints for table `sanpham`
--
ALTER TABLE `sanpham`
  ADD CONSTRAINT `fk_sanpham_loaisp` FOREIGN KEY (`MaLoaiSP`) REFERENCES `loaisanpham` (`MaLoaiSP`);

--
-- Constraints for table `taikhoan`
--
ALTER TABLE `taikhoan`
  ADD CONSTRAINT `fk_taikhoan_vaitro` FOREIGN KEY (`MaVaiTro`) REFERENCES `vaitro` (`MaVaiTro`);

--
-- Constraints for table `thanhtoan`
--
ALTER TABLE `thanhtoan`
  ADD CONSTRAINT `fk_thanhtoan_hoadon` FOREIGN KEY (`MaHD`) REFERENCES `hoadon` (`MaHD`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

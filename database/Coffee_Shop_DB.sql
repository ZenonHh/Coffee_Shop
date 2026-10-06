-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 06, 2026 at 09:26 AM
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
  `DiemTichLuy` int(11) DEFAULT 0,
  `HangTV` varchar(20) DEFAULT 'Đồng',
  `NgayTao` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

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
  ``
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
  `KichCo` varchar(10) DEFAULT 'M',
  `GiaBan` decimal(12,2) NOT NULL,
  `TrangThai` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

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
  ADD UNIQUE KEY `TaiKhoan` (`TaiKhoan`);

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
  MODIFY `MaKH` int(11) NOT NULL AUTO_INCREMENT;

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
  MODIFY `MaLoaiSP` int(11) NOT NULL AUTO_INCREMENT;

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
  MODIFY `MaSP` int(11) NOT NULL AUTO_INCREMENT;

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

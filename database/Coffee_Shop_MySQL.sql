CREATE DATABASE IF NOT EXISTS coffee_shop_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE coffee_shop_db;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE CaLam (
    MaCaLam INT NOT NULL AUTO_INCREMENT,
    MaNV INT NOT NULL,
    GioMoCa DATETIME NULL,
    GioDongCa DATETIME NULL,
    TienDauCa DECIMAL(12, 2) NULL,
    TienCuoiCa DECIMAL(12, 2) NULL,
    PRIMARY KEY (MaCaLam)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE CauHoiKhaoSat (
    MaCauHoi INT NOT NULL AUTO_INCREMENT,
    MaKhaoSat INT NOT NULL,
    NoiDungCauHoi VARCHAR(255) NOT NULL,
    LoaiCauHoi VARCHAR(50) NULL,
    PRIMARY KEY (MaCauHoi)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE CauTraLoi (
    MaCauTraLoi INT NOT NULL AUTO_INCREMENT,
    MaLoiMoi INT NOT NULL,
    MaCauHoi INT NOT NULL,
    MaDanhMuc INT NULL,
    NoiDungTraLoi LONGTEXT NULL,
    NgayTraLoi DATETIME NULL,
    PRIMARY KEY (MaCauTraLoi)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ChiTietGioHang (
    MaCTGH INT NOT NULL AUTO_INCREMENT,
    MaGioHang INT NOT NULL,
    MaSP INT NOT NULL,
    SoLuong INT NULL,
    TongTien DECIMAL(12, 2) NOT NULL,
    PRIMARY KEY (MaCTGH)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ChiTietHoaDon (
    MaCTHD INT NOT NULL AUTO_INCREMENT,
    MaHD INT NOT NULL,
    MaSP INT NOT NULL,
    SoLuong INT NOT NULL,
    GiaBan DECIMAL(12, 2) NOT NULL,
    ThanhTien DECIMAL(12, 2) NOT NULL,
    PRIMARY KEY (MaCTHD)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ChiTietKM (
    MaCTKM INT NOT NULL AUTO_INCREMENT,
    TenCTKM VARCHAR(150) NOT NULL,
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    MoTa VARCHAR(255) NULL,
    TrangThai TINYINT(1) NULL,
    PRIMARY KEY (MaCTKM)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ChiTietPhieuNhap (
    MaCTPN INT NOT NULL AUTO_INCREMENT,
    MaPN INT NOT NULL,
    MaNL INT NOT NULL,
    SoLuong DECIMAL(10, 2) NOT NULL,
    DonGia DECIMAL(12, 2) NOT NULL,
    ThanhTien DECIMAL(12, 2) NOT NULL,
    PRIMARY KEY (MaCTPN)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE CTKM_Theo_HoaDon (
    MaKMDon INT NOT NULL AUTO_INCREMENT,
    MaCTKM INT NOT NULL,
    DonToiThieu DECIMAL(12, 2) NOT NULL,
    KieuGiam VARCHAR(10) NULL,
    GiaTriGiam DECIMAL(12, 2) NOT NULL,
    GiamToiDa DECIMAL(12, 2) NULL,
    PRIMARY KEY (MaKMDon)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE CTKM_Theo_SP (
    MaKMSP INT NOT NULL AUTO_INCREMENT,
    MaCTKM INT NOT NULL,
    MaSP INT NOT NULL,
    KieuGiam VARCHAR(10) NULL,
    GiaTriGiam DECIMAL(12, 2) NOT NULL,
    PRIMARY KEY (MaKMSP)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE DanhMucLuaChon (
    MaDanhMuc INT NOT NULL AUTO_INCREMENT,
    MaCauHoi INT NOT NULL,
    NoiDungLuaChon VARCHAR(255) NOT NULL,
    ThuTuHienThi INT NULL,
    PRIMARY KEY (MaDanhMuc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE GioHang (
    MaGioHang INT NOT NULL AUTO_INCREMENT,
    MaKH INT NOT NULL,
    NgayTao DATETIME NULL,
    PRIMARY KEY (MaGioHang)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE HoaDon (
    MaHD INT NOT NULL AUTO_INCREMENT,
    MaCaLam INT NULL,
    MaKH INT NULL,
    MaNV INT NULL,
    MaCTKM INT NULL,
    LoaiDonHang VARCHAR(50) NULL,
    TongTien DECIMAL(12, 2) NULL,
    TienGiamGia DECIMAL(12, 2) NULL,
    TrangThai VARCHAR(50) NULL,
    NgayTao DATETIME NULL,
    TenNguoiNhan VARCHAR(100) NULL,
    SdtNguoiNhan VARCHAR(20) NULL,
    DiaChiGiaoHang VARCHAR(255) NULL,
    GhiChuGiaoHang VARCHAR(500) NULL,
    PRIMARY KEY (MaHD)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE KhachHang (
    MaKH INT NOT NULL AUTO_INCREMENT,
    TaiKhoan VARCHAR(50) NULL,
    MatKhau VARCHAR(255) NULL,
    HoTen VARCHAR(100) NOT NULL,
    Sdt VARCHAR(20) NOT NULL,
    DiemTichLuy INT NULL,
    HangTV VARCHAR(20) NULL,
    NgayTao DATETIME NULL,
    PRIMARY KEY (MaKH)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE KhaoSat (
    MaKhaoSat INT NOT NULL AUTO_INCREMENT,
    MaTaiKhoan INT NOT NULL,
    TieuDe VARCHAR(255) NOT NULL,
    MoTa LONGTEXT NULL,
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    PRIMARY KEY (MaKhaoSat)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE LichSuDiem (
    MaLSD INT NOT NULL AUTO_INCREMENT,
    MaHD INT NULL,
    MaKH INT NOT NULL,
    DiemThayDoi INT NOT NULL,
    LyDo VARCHAR(255) NULL,
    NgayThucHien DATETIME NULL,
    PRIMARY KEY (MaLSD)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE LoaiSanPham (
    MaLoaiSP INT NOT NULL AUTO_INCREMENT,
    TenLoaiSP VARCHAR(100) NOT NULL,
    ThuTuHienThi INT NULL,
    PRIMARY KEY (MaLoaiSP)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE LoiMoiKhaoSat (
    MaLoiMoi INT NOT NULL AUTO_INCREMENT,
    MaKhaoSat INT NOT NULL,
    MaKH INT NOT NULL,
    ThoiGianGui DATETIME NULL,
    TrangThai VARCHAR(50) NULL,
    PRIMARY KEY (MaLoiMoi)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE NguyenLieu (
    MaNL INT NOT NULL AUTO_INCREMENT,
    TenNL VARCHAR(100) NOT NULL,
    DonViTinh VARCHAR(20) NULL,
    GiaVon DECIMAL(12, 2) NULL,
    PRIMARY KEY (MaNL)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE NhaCungCap (
    MaNCC INT NOT NULL AUTO_INCREMENT,
    TenNCC VARCHAR(150) NOT NULL,
    Sdt VARCHAR(20) NULL,
    DiaChi VARCHAR(255) NULL,
    TrangThai TINYINT(1) NULL,
    PRIMARY KEY (MaNCC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE NhanVien (
    MaNV INT NOT NULL AUTO_INCREMENT,
    HoVaDem VARCHAR(50) NOT NULL,
    Ten VARCHAR(20) NOT NULL,
    Sdt VARCHAR(20) NULL,
    ChucVu VARCHAR(50) NULL,
    PRIMARY KEY (MaNV)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE NhatKyHoatDong (
    MaNhatKy INT NOT NULL AUTO_INCREMENT,
    MaTaiKhoan INT NOT NULL,
    HanhDong VARCHAR(255) NOT NULL,
    ThoiGian DATETIME NULL,
    PRIMARY KEY (MaNhatKy)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE PhanHoi (
    MaPhanHoi INT NOT NULL AUTO_INCREMENT,
    MaKH INT NOT NULL,
    MaSP INT NULL,
    NoiDung LONGTEXT NOT NULL,
    DanhGia INT NULL,
    NgayGui DATETIME NULL,
    TrangThaiXL VARCHAR(50) NULL,
    PRIMARY KEY (MaPhanHoi)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE PhieuNhap (
    MaPN INT NOT NULL AUTO_INCREMENT,
    MaNCC INT NOT NULL,
    MaNV INT NOT NULL,
    NgayNhap DATETIME NULL,
    TongTien DECIMAL(12, 2) NULL,
    GhiChu VARCHAR(255) NULL,
    PRIMARY KEY (MaPN)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE SanPham (
    MaSP INT NOT NULL AUTO_INCREMENT,
    MaLoaiSP INT NOT NULL,
    TenSP VARCHAR(100) NOT NULL,
    KichCo VARCHAR(10) NULL,
    GiaBan DECIMAL(12, 2) NOT NULL,
    TrangThai TINYINT(1) NULL,
    HinhAnh VARCHAR(255) NULL,
    PRIMARY KEY (MaSP)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT NOT NULL AUTO_INCREMENT,
    MaVaiTro INT NOT NULL,
    TenDangNhap VARCHAR(50) NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen VARCHAR(100) NOT NULL,
    TrangThai TINYINT(1) NULL,
    PRIMARY KEY (MaTaiKhoan),
    UNIQUE KEY uq_taikhoan_tendangnhap (TenDangNhap)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ThanhToan (
    MaThanhToan INT NOT NULL AUTO_INCREMENT,
    MaHD INT NOT NULL,
    PhuongThuc VARCHAR(50) NULL,
    SoTien DECIMAL(12, 2) NOT NULL,
    MaGiaoDich VARCHAR(100) NULL,
    ThoiGian DATETIME NULL,
    PRIMARY KEY (MaThanhToan)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE VaiTro (
    MaVaiTro INT NOT NULL AUTO_INCREMENT,
    TenVaiTro VARCHAR(50) NOT NULL,
    MoTa VARCHAR(255) NULL,
    PRIMARY KEY (MaVaiTro),
    UNIQUE KEY uq_vaitro_tenvaitro (TenVaiTro)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE Voucher (
    MaVoucher INT NOT NULL AUTO_INCREMENT,
    MaCode VARCHAR(50) NOT NULL,
    PhanLoaiGiam VARCHAR(20) NULL,
    GiaTriGiam DECIMAL(12, 2) NOT NULL,
    TrangThai TINYINT(1) NULL,
    PRIMARY KEY (MaVoucher),
    UNIQUE KEY uq_voucher_macode (MaCode)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE CaLam
    ADD CONSTRAINT fk_calam_nhanvien FOREIGN KEY (MaNV) REFERENCES NhanVien (MaNV);
ALTER TABLE CauHoiKhaoSat
    ADD CONSTRAINT fk_cauhoikhaosat_khaosat FOREIGN KEY (MaKhaoSat) REFERENCES KhaoSat (MaKhaoSat);
ALTER TABLE CauTraLoi
    ADD CONSTRAINT fk_cautraloi_cauhoi FOREIGN KEY (MaCauHoi) REFERENCES CauHoiKhaoSat (MaCauHoi),
    ADD CONSTRAINT fk_cautraloi_danhmuc FOREIGN KEY (MaDanhMuc) REFERENCES DanhMucLuaChon (MaDanhMuc),
    ADD CONSTRAINT fk_cautraloi_loimoi FOREIGN KEY (MaLoiMoi) REFERENCES LoiMoiKhaoSat (MaLoiMoi);
ALTER TABLE ChiTietGioHang
    ADD CONSTRAINT fk_chitietgiohang_giohang FOREIGN KEY (MaGioHang) REFERENCES GioHang (MaGioHang),
    ADD CONSTRAINT fk_chitietgiohang_sanpham FOREIGN KEY (MaSP) REFERENCES SanPham (MaSP);
ALTER TABLE ChiTietHoaDon
    ADD CONSTRAINT fk_chitiethoadon_hoadon FOREIGN KEY (MaHD) REFERENCES HoaDon (MaHD),
    ADD CONSTRAINT fk_chitiethoadon_sanpham FOREIGN KEY (MaSP) REFERENCES SanPham (MaSP);
ALTER TABLE ChiTietPhieuNhap
    ADD CONSTRAINT fk_chitietphieunhap_nguyenlieu FOREIGN KEY (MaNL) REFERENCES NguyenLieu (MaNL),
    ADD CONSTRAINT fk_chitietphieunhap_phieunhap FOREIGN KEY (MaPN) REFERENCES PhieuNhap (MaPN);
ALTER TABLE CTKM_Theo_HoaDon
    ADD CONSTRAINT fk_ctkm_hoadon_chitietkm FOREIGN KEY (MaCTKM) REFERENCES ChiTietKM (MaCTKM);
ALTER TABLE CTKM_Theo_SP
    ADD CONSTRAINT fk_ctkm_sanpham_chitietkm FOREIGN KEY (MaCTKM) REFERENCES ChiTietKM (MaCTKM),
    ADD CONSTRAINT fk_ctkm_sanpham_sanpham FOREIGN KEY (MaSP) REFERENCES SanPham (MaSP);
ALTER TABLE DanhMucLuaChon
    ADD CONSTRAINT fk_danhmucluachon_cauhoi FOREIGN KEY (MaCauHoi) REFERENCES CauHoiKhaoSat (MaCauHoi);
ALTER TABLE GioHang
    ADD CONSTRAINT fk_giohang_khachhang FOREIGN KEY (MaKH) REFERENCES KhachHang (MaKH);
ALTER TABLE HoaDon
    ADD CONSTRAINT fk_hoadon_calam FOREIGN KEY (MaCaLam) REFERENCES CaLam (MaCaLam),
    ADD CONSTRAINT fk_hoadon_chitietkm FOREIGN KEY (MaCTKM) REFERENCES ChiTietKM (MaCTKM),
    ADD CONSTRAINT fk_hoadon_khachhang FOREIGN KEY (MaKH) REFERENCES KhachHang (MaKH),
    ADD CONSTRAINT fk_hoadon_nhanvien FOREIGN KEY (MaNV) REFERENCES NhanVien (MaNV);
ALTER TABLE KhaoSat
    ADD CONSTRAINT fk_khaosat_taikhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan (MaTaiKhoan);
ALTER TABLE LichSuDiem
    ADD CONSTRAINT fk_lichsudiem_hoadon FOREIGN KEY (MaHD) REFERENCES HoaDon (MaHD),
    ADD CONSTRAINT fk_lichsudiem_khachhang FOREIGN KEY (MaKH) REFERENCES KhachHang (MaKH);
ALTER TABLE LoiMoiKhaoSat
    ADD CONSTRAINT fk_loimoikhaosat_khaosat FOREIGN KEY (MaKhaoSat) REFERENCES KhaoSat (MaKhaoSat),
    ADD CONSTRAINT fk_loimoikhaosat_khachhang FOREIGN KEY (MaKH) REFERENCES KhachHang (MaKH);
ALTER TABLE NhatKyHoatDong
    ADD CONSTRAINT fk_nhatkyhoatdong_taikhoan FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan (MaTaiKhoan);
ALTER TABLE PhanHoi
    ADD CONSTRAINT fk_phanhoi_khachhang FOREIGN KEY (MaKH) REFERENCES KhachHang (MaKH),
    ADD CONSTRAINT fk_phanhoi_sanpham FOREIGN KEY (MaSP) REFERENCES SanPham (MaSP);
ALTER TABLE PhieuNhap
    ADD CONSTRAINT fk_phieunhap_nhacungcap FOREIGN KEY (MaNCC) REFERENCES NhaCungCap (MaNCC),
    ADD CONSTRAINT fk_phieunhap_nhanvien FOREIGN KEY (MaNV) REFERENCES NhanVien (MaNV);
ALTER TABLE SanPham
    ADD CONSTRAINT fk_sanpham_loaisanpham FOREIGN KEY (MaLoaiSP) REFERENCES LoaiSanPham (MaLoaiSP);
ALTER TABLE TaiKhoan
    ADD CONSTRAINT fk_taikhoan_vaitro FOREIGN KEY (MaVaiTro) REFERENCES VaiTro (MaVaiTro);
ALTER TABLE ThanhToan
    ADD CONSTRAINT fk_thanhtoan_hoadon FOREIGN KEY (MaHD) REFERENCES HoaDon (MaHD);

SET FOREIGN_KEY_CHECKS = 1;

INSERT INTO LoaiSanPham (TenLoaiSP, ThuTuHienThi) VALUES
    ('Cà phê & Matcha', 1),
    ('Trà & Đồ uống', 2),
    ('Ăn nhẹ', 3),
    ('Tráng miệng', 4),
    ('Topping', 5);

INSERT INTO VaiTro (TenVaiTro, MoTa) VALUES
    ('Quản trị viên', 'Tài khoản quản trị mẫu'),
    ('Nhân viên', 'Tài khoản nhân viên'),
    ('Khách hàng', 'Tài khoản khách hàng');

-- Demo password for the sample account: CoffeeDemo123! (replace it before deployment).
INSERT INTO TaiKhoan (MaVaiTro, TenDangNhap, MatKhau, HoTen, TrangThai) VALUES
    (1, 'demo_admin', '$2y$10$replace_this_with_a_valid_password_hash', 'Quản trị viên mẫu', 1);

INSERT INTO SanPham (MaLoaiSP, TenSP, KichCo, GiaBan, TrangThai, HinhAnh) VALUES
    (1, 'Americano', 'M', 35000.00, 1, 'americano.png'),
    (1, 'Cà phê kem mây chanh vàng', 'M', 49000.00, 1, 'cafe-kem-may-chanh-vang.png'),
    (2, 'Cold brew chanh', 'M', 45000.00, 1, 'cold-brew-chanh.png'),
    (3, 'Bánh chuối', NULL, 30000.00, 1, 'banh-chuoi.png');

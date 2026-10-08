<?php

// ====== CẤU HÌNH THEO DỮ LIỆU CỦA BẠN (đổi số cho đúng MaLoaiSP trong bảng loaisanpham) ======
// Mã loại chứa các món topping (Trân châu, Thạch dừa...)
if (!defined('MA_LOAI_TOPPING')) define('MA_LOAI_TOPPING', 9);
// Các mã loại đồ uống được phép chọn thêm topping
if (!defined('MA_LOAI_CO_TOPPING')) define('MA_LOAI_CO_TOPPING', [1, 2, 3, 4, 5, 6, 7]);

class Product
{
    private $db;

    public function __construct()
    {
        $this->db = new Database();
    }

    // Lấy tất cả sản phẩm đang bán, kèm giá thấp nhất trong các biến thể còn hiển thị
    public function getAll()
    {
        return $this->db->select(
            "SELECT sp.MaSP, sp.TenSP, sp.HinhAnh, MIN(bt.GiaBan) AS GiaThapNhat
             FROM sanpham sp
             JOIN bienthesanpham bt ON sp.MaSP = bt.MaSP
             WHERE sp.TrangThai = 1 AND bt.TrangThai = 1
             GROUP BY sp.MaSP, sp.TenSP, sp.HinhAnh"
        );
    }

    // Lấy sản phẩm theo 1 loại cụ thể (MaLoaiSP)
    public function getByCategory($maLoaiSP)
    {
        $this->db->select_prepare(
            "SELECT sp.MaSP, sp.TenSP, sp.HinhAnh, MIN(bt.GiaBan) AS GiaThapNhat
             FROM sanpham sp
             JOIN bienthesanpham bt ON sp.MaSP = bt.MaSP
             WHERE sp.TrangThai = 1 AND bt.TrangThai = 1 AND sp.MaLoaiSP = ?
             GROUP BY sp.MaSP, sp.TenSP, sp.HinhAnh",
            'i',
            $maLoaiSP
        );
        return $this->db->fetchAll();
    }

    // Lấy chi tiết 1 sản phẩm (kèm tên loại và loại đó có cho thêm topping không)
    public function getById($maSP)
    {
        $this->db->select_prepare(
            "SELECT sp.*, lsp.TenLoaiSP
             FROM sanpham sp
             JOIN loaisanpham lsp ON sp.MaLoaiSP = lsp.MaLoaiSP
             WHERE sp.TrangThai = 1 AND sp.MaSP = ?",
            'i',
            $maSP
        );
        $rows = $this->db->fetchAll();
        return $rows ? $rows[0] : null;
    }

    // Lấy các size (biến thể) đang bán của 1 sản phẩm, giá thấp xếp trước
    public function getVariants($maSP)
    {
        $this->db->select_prepare(
            "SELECT MaBienThe, KichCo, GiaBan
             FROM bienthesanpham
             WHERE MaSP = ? AND TrangThai = 1
             ORDER BY GiaBan ASC",
            'i',
            $maSP
        );
        return $this->db->fetchAll();
    }
}

class LoaiSanPham
{
    private $db;

    public function __construct()
    {
        $this->db = new Database();
    }

    // Lấy toàn bộ danh mục, sắp theo đúng thứ tự muốn hiển thị (ThuTuHienThi)
    public function getAll()
    {
        return $this->db->select(
            "SELECT MaLoaiSP, TenLoaiSP, ThuTuHienThi
             FROM loaisanpham
             WHERE MaLoaiSP <> " . (int)MA_LOAI_TOPPING . "
             ORDER BY ThuTuHienThi ASC"
        );
    }
}

class Topping
{
    private $db;

    public function __construct()
    {
        $this->db = new Database();
    }

    // Topping là các sản phẩm thuộc loại MA_LOAI_TOPPING.
    // MaTopping = MaSP, TenTopping = TenSP, GiaTopping = giá thấp nhất của sản phẩm đó
    public function getAll()
    {
        return $this->db->select(
            "SELECT sp.MaSP AS MaTopping, sp.TenSP AS TenTopping, MIN(bt.GiaBan) AS GiaTopping
             FROM sanpham sp
             JOIN bienthesanpham bt ON sp.MaSP = bt.MaSP
             WHERE sp.MaLoaiSP = " . (int)MA_LOAI_TOPPING . " AND sp.TrangThai = 1 AND bt.TrangThai = 1
             GROUP BY sp.MaSP, sp.TenSP
             ORDER BY GiaTopping ASC, sp.TenSP ASC"
        );
    }
}
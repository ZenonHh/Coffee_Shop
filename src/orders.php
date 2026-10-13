<?php
// src/orders.php
// Tạo hóa đơn từ giỏ hàng, ghi 4 bảng trong 1 transaction:
//   hoadon -> chitiethoadon -> chitiethoadon_topping, và thanhtoan
// Cần include database/connect.php trước.

require_once __DIR__ . '/base-model.php';

class Order extends BaseModel
{
    // ===== Chỉnh 2 giá trị này cho đúng với dữ liệu của bạn =====
    // Giá trị cột hoadon.LoaiDonHang cho đơn "giao hàng" (website hiện chỉ có giao hàng)
    const LOAI_DON_GIAO_HANG = '1';
    // Giá trị cột TrangThai của hóa đơn mới / thanh toán mới (0 = chờ xử lý, chờ thanh toán)
    const TRANG_THAI_MOI = '0';

    public $loi = ''; // nội dung lỗi gần nhất (để debug)

    // Lấy tên + sđt khách để điền sẵn vào form thanh toán
    public function getThongTinKhach($maKH)
    {
        $this->db->select_prepare('SELECT HoTen, Sdt FROM khachhang WHERE MaKH = ?', 'i', (int)$maKH);
        return $this->db->fetch();
    }

    // Lấy 1 hóa đơn của đúng khách đó (kèm phương thức thanh toán) để hiện trang "đặt hàng thành công"
    public function getById($maHD, $maKH)
    {
        $this->db->select_prepare(
            'SELECT h.*, tt.PhuongThuc
             FROM hoadon h
             LEFT JOIN thanhtoan tt ON tt.MaHD = h.MaHD
             WHERE h.MaHD = ? AND h.MaKH = ?
             LIMIT 1',
            'ii',
            (int)$maHD,
            (int)$maKH
        );
        return $this->db->fetch();
    }
// Danh sách đơn của 1 khách, đơn mới nhất lên đầu
public function getByKhach($maKH)
{
    return $this->db->select(
        "SELECT h.MaHD, h.NgayTao, h.TongTien, h.TrangThai, tt.PhuongThuc
         FROM hoadon h
         LEFT JOIN thanhtoan tt ON tt.MaHD = h.MaHD
         WHERE h.MaKH = " . (int)$maKH . "
         ORDER BY h.MaHD DESC"
    );
}

// Các món của 1 đơn (kèm tên topping). Chỉ gọi sau khi getById() đã xác nhận đơn thuộc về khách.
public function getChiTiet($maHD)
{
    $maHD = (int)$maHD;
    $rows = $this->db->select(
        "SELECT ct.MaCTHD, ct.SoLuong, ct.GiaBan, ct.TienTopping, ct.ThanhTien, sp.TenSP, bt.KichCo
         FROM chitiethoadon ct
         JOIN sanpham sp ON sp.MaSP = ct.MaSP
         LEFT JOIN bienthesanpham bt ON bt.MaBienThe = ct.MaBienThe
         WHERE ct.MaHD = $maHD
         ORDER BY ct.MaCTHD"
    );
    if (!$rows) {
        return [];
    }

    $tpRows = $this->db->select(
        "SELECT ctt.MaCTHD, sp.TenSP
         FROM chitiethoadon_topping ctt
         JOIN chitiethoadon ct ON ct.MaCTHD = ctt.MaCTHD
         JOIN sanpham sp ON sp.MaSP = ctt.MaSP
         WHERE ct.MaHD = $maHD"
    );
    $tpTheoDong = [];
    foreach ($tpRows as $t) {
        $tpTheoDong[(int)$t['MaCTHD']][] = $t['TenSP'];
    }
    foreach ($rows as &$r) {
        $r['Topping'] = $tpTheoDong[(int)$r['MaCTHD']] ?? [];
    }
    return $rows;
}
    // Tạo hóa đơn. Trả về MaHD nếu thành công, false nếu lỗi (không lưu gì cả).
    // $nguoiNhan = ['HoTen','Sdt','DiaChi','GhiChu','PhuongThucTT']  (PhuongThucTT: 'COD' hoặc 'CK')
    // $lines, $tong lấy từ Cart::getLines() và Cart::summary()
    public function create($maKH, array $nguoiNhan, array $lines, array $tong)
    {
        $conn = $this->db->getConnection();

        try {
            $conn->begin_transaction();

            $ghiChu = $nguoiNhan['GhiChu'] !== '' ? $nguoiNhan['GhiChu'] : null;

            // MaNV (nhân viên xử lý) và MaCTKM (khuyến mãi) để trống, nhân viên/khuyến mãi gắn vào sau
            $this->chay(
                'INSERT INTO hoadon (MaKH, LoaiDonHang, TamTinh, PhiGiao, TienGiamGia, TongTien, TrangThai,
                                     HoTenNguoiNhan, SdtNguoiNhan, DiaChiGiao, GhiChu, NgayTao)
                 VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, NOW())',
                'isiiiisssss',
                (int)$maKH, self::LOAI_DON_GIAO_HANG,
                (int)$tong['TamTinh'], (int)$tong['PhiGiao'], (int)$tong['GiamGia'], (int)$tong['Tong'],
                self::TRANG_THAI_MOI,
                $nguoiNhan['HoTen'], $nguoiNhan['Sdt'], $nguoiNhan['DiaChi'], $ghiChu
            );
            $maHD = $this->idMoiNhat();

            foreach ($lines as $l) {
                // GiaBan = giá size tại thời điểm mua, TienTopping = tiền topping của 1 ly
                $this->chay(
                    'INSERT INTO chitiethoadon (MaHD, MaBienThe, MaSP, SoLuong, GiaBan, TienTopping, ThanhTien)
                     VALUES (?, ?, ?, ?, ?, ?, ?)',
                    'iiiiiii',
                    $maHD, $l['MaBienThe'], $l['MaSP'], $l['SoLuong'], $l['GiaSize'], $l['GiaTopping'], $l['ThanhTien']
                );
                $maCT = $this->idMoiNhat();

                foreach ($l['Topping'] as $tp) {
                    $this->chay(
                        'INSERT INTO chitiethoadon_topping (MaCTHD, MaSP, GiaBan) VALUES (?, ?, ?)',
                        'iii',
                        $maCT, $tp['MaTopping'], $tp['Gia']
                    );
                }
            }

            // Bản ghi thanh toán: COD / chuyển khoản đều ở trạng thái "chờ thanh toán", chưa có mã giao dịch
            $this->chay(
                'INSERT INTO thanhtoan (MaHD, PhuongThuc, SoTien, MaGiaoDich, ThoiGian, TrangThai)
                 VALUES (?, ?, ?, NULL, NOW(), ?)',
                'isis',
                $maHD, $nguoiNhan['PhuongThucTT'], (int)$tong['Tong'], self::TRANG_THAI_MOI
            );

            $conn->commit();
            return $maHD;
        } catch (Throwable $e) {
            $conn->rollback();
            $this->loi = $e->getMessage();
            return false;
        }
    }
}
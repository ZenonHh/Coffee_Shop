<?php
// src/cart.php
// Giỏ hàng lưu trong CSDL, mỗi khách (MaKH) có đúng 1 giỏ:
//   giohang -> chitietgiohang (1 dòng = 1 size + 1 bộ topping) -> chitietgiohang_topping
// Tên và giá luôn lấy lại từ bảng sanpham/bienthesanpham mỗi lần hiển thị,
// nên khách không thể tự sửa giá từ trình duyệt.
// Cần include database/connect.php trước khi dùng file này.

require_once __DIR__ . '/products.php';   // dùng hằng MA_LOAI_TOPPING, MA_LOAI_CO_TOPPING
require_once __DIR__ . '/base-model.php';

if (!function_exists('dinh_dang_tien')) {
    function dinh_dang_tien($so)
    {
        return number_format((float)$so, 0, ',', '.') . 'đ';
    }
}

class Cart extends BaseModel
{
    const PHI_GIAO  = 0;    // phí giao hàng cố định, sau này có thể tính theo khu vực
    const SL_TOI_DA = 100;  // số lượng tối đa mỗi dòng

    private $maKH;

    public function __construct($maKH)
    {
        parent::__construct();
        $this->maKH = (int)$maKH;
    }

    // ---------- Thêm / sửa / xóa ----------

    // Thêm 1 món vào giỏ. Trả về true nếu thành công, false nếu dữ liệu không hợp lệ.
    public function add($maBienThe, array $topping, $soLuong)
    {
        $maBienThe = (int)$maBienThe;
        $soLuong   = max(1, min(self::SL_TOI_DA, (int)$soLuong));

        // 1. Size phải tồn tại và đang bán
        $this->db->select_prepare(
            "SELECT sp.MaSP, sp.MaLoaiSP
             FROM bienthesanpham bt
             JOIN sanpham sp ON sp.MaSP = bt.MaSP
             WHERE bt.MaBienThe = ? AND bt.TrangThai = 1 AND sp.TrangThai = 1",
            'i',
            $maBienThe
        );
        $sp = $this->db->fetch();
        if (!$sp) {
            return false;
        }

        // 2. Chỉ giữ lại các topping hợp lệ
        $topping = $this->locTopping($topping, (int)$sp['MaLoaiSP']);

        // 3. KhoaDong = size + topping. Cùng khóa thì cộng dồn số lượng vào 1 dòng
        $khoa = $maBienThe . ':' . implode('-', $topping);

        $conn = $this->db->getConnection();
        try {
            $conn->begin_transaction();

            $maGH = $this->maGioHang(true);

            $this->db->select_prepare(
                'SELECT MaCTGH, SoLuong FROM chitietgiohang WHERE MaGioHang = ? AND KhoaDong = ?',
                'is',
                $maGH,
                $khoa
            );
            $dong = $this->db->fetch();

            if ($dong) {
                $moi = min(self::SL_TOI_DA, (int)$dong['SoLuong'] + $soLuong);
                $this->chay('UPDATE chitietgiohang SET SoLuong = ? WHERE MaCTGH = ?', 'ii', $moi, (int)$dong['MaCTGH']);
            } else {
                $this->chay(
                    'INSERT INTO chitietgiohang (MaGioHang, MaBienThe, MaSP, SoLuong, KhoaDong) VALUES (?, ?, ?, ?, ?)',
                    'iiiis',
                    $maGH, $maBienThe, (int)$sp['MaSP'], $soLuong, $khoa
                );
                $maCT = $this->idMoiNhat();
                foreach ($topping as $maTopping) {
                    $this->chay('INSERT INTO chitietgiohang_topping (MaCTGH, MaSP) VALUES (?, ?)', 'ii', $maCT, $maTopping);
                }
            }

            $conn->commit();
            return true;
        } catch (Throwable $e) {
            $conn->rollback();
            return false;
        }
    }

    // $slTheoKey = ['KhoaDong' => số lượng mới]. Số lượng <= 0 thì xóa dòng đó.
    public function update(array $slTheoKey)
    {
        $maGH = $this->maGioHang();
        if (!$maGH) {
            return;
        }
        foreach ($slTheoKey as $key => $sl) {
            $sl = (int)$sl;
            if ($sl <= 0) {
                $this->chay('DELETE FROM chitietgiohang WHERE MaGioHang = ? AND KhoaDong = ?', 'is', $maGH, (string)$key);
            } else {
                $this->chay(
                    'UPDATE chitietgiohang SET SoLuong = ? WHERE MaGioHang = ? AND KhoaDong = ?',
                    'iis',
                    min(self::SL_TOI_DA, $sl), $maGH, (string)$key
                );
            }
        }
    }

    public function remove($key)
    {
        $maGH = $this->maGioHang();
        if ($maGH) {
            $this->chay('DELETE FROM chitietgiohang WHERE MaGioHang = ? AND KhoaDong = ?', 'is', $maGH, (string)$key);
        }
    }

    public function clear()
    {
        $maGH = $this->maGioHang();
        if ($maGH) {
            $this->chay('DELETE FROM chitietgiohang WHERE MaGioHang = ?', 'i', $maGH);
        }
    }

    // Tổng số ly/món trong giỏ
    public function count()
    {
        $this->db->select_prepare(
            'SELECT COALESCE(SUM(ct.SoLuong), 0) AS n
             FROM chitietgiohang ct
             JOIN giohang g ON g.MaGioHang = ct.MaGioHang
             WHERE g.MaKH = ?',
            'i',
            $this->maKH
        );
        $r = $this->db->fetch();
        return (int)($r['n'] ?? 0);
    }

    // ---------- Đọc giỏ hàng (kèm tên, giá thật từ CSDL) ----------

    public function getLines()
    {
        $maGH = $this->maGioHang();
        if (!$maGH) {
            return [];
        }

        $rows = $this->db->select(
            "SELECT ct.MaCTGH, ct.KhoaDong, ct.SoLuong,
                    bt.MaBienThe, bt.KichCo, bt.GiaBan, bt.TrangThai AS TrangThaiBienThe,
                    sp.MaSP, sp.TenSP, sp.HinhAnh, sp.TrangThai AS TrangThaiSP
             FROM chitietgiohang ct
             JOIN bienthesanpham bt ON bt.MaBienThe = ct.MaBienThe
             JOIN sanpham sp ON sp.MaSP = bt.MaSP
             WHERE ct.MaGioHang = " . (int)$maGH . "
             ORDER BY ct.MaCTGH"
        );

        // Món đã bị ẩn/ngừng bán sau khi khách thêm vào giỏ -> xóa khỏi giỏ
        $conHang = [];
        foreach ($rows as $r) {
            if ((int)$r['TrangThaiBienThe'] !== 1 || (int)$r['TrangThaiSP'] !== 1) {
                $this->chay('DELETE FROM chitietgiohang WHERE MaCTGH = ?', 'i', (int)$r['MaCTGH']);
            } else {
                $conHang[] = $r;
            }
        }
        if (!$conHang) {
            return [];
        }

        // Lấy topping của tất cả các dòng bằng 1 câu truy vấn
        $ids = implode(',', array_map(function ($r) { return (int)$r['MaCTGH']; }, $conHang));
        $tpRows = $this->db->select(
            "SELECT ctt.MaCTGH, sp.MaSP AS MaTopping, sp.TenSP AS TenTopping, MIN(bt.GiaBan) AS Gia
             FROM chitietgiohang_topping ctt
             JOIN sanpham sp ON sp.MaSP = ctt.MaSP
             JOIN bienthesanpham bt ON bt.MaSP = sp.MaSP
             WHERE ctt.MaCTGH IN ($ids) AND sp.TrangThai = 1 AND bt.TrangThai = 1
             GROUP BY ctt.MaCTGH, sp.MaSP, sp.TenSP
             ORDER BY ctt.MaCTGH, sp.MaSP"
        );
        $tpTheoDong = [];
        foreach ($tpRows as $t) {
            $tpTheoDong[(int)$t['MaCTGH']][] = [
                'MaTopping'  => (int)$t['MaTopping'],
                'TenTopping' => $t['TenTopping'],
                'Gia'        => (int)round((float)$t['Gia']),
            ];
        }

        $lines = [];
        foreach ($conHang as $r) {
            $toppings   = $tpTheoDong[(int)$r['MaCTGH']] ?? [];
            $giaTopping = array_sum(array_column($toppings, 'Gia'));
            $giaSize    = (int)round((float)$r['GiaBan']);
            $soLuong    = (int)$r['SoLuong'];
            $donGia     = $giaSize + $giaTopping;

            $lines[] = [
                'Key'        => (string)$r['KhoaDong'],
                'MaBienThe'  => (int)$r['MaBienThe'],
                'MaSP'       => (int)$r['MaSP'],
                'TenSP'      => $r['TenSP'],
                'KichCo'     => $r['KichCo'],
                'HinhAnh'    => $r['HinhAnh'],
                'GiaSize'    => $giaSize,
                'Topping'    => $toppings,
                'GiaTopping' => $giaTopping,
                'DonGia'     => $donGia,
                'SoLuong'    => $soLuong,
                'ThanhTien'  => $donGia * $soLuong,
            ];
        }
        return $lines;
    }

    // Tạm tính / phí giao / giảm giá / tổng cộng
    public function summary(array $lines)
    {
        $tamTinh = 0;
        foreach ($lines as $l) {
            $tamTinh += $l['ThanhTien'];
        }
        $phiGiao = $tamTinh > 0 ? self::PHI_GIAO : 0;
        $giamGia = 0; // chỗ này sau này gắn voucher / khuyến mãi

        return [
            'TamTinh' => $tamTinh,
            'PhiGiao' => $phiGiao,
            'GiamGia' => $giamGia,
            'Tong'    => $tamTinh + $phiGiao - $giamGia,
        ];
    }

    // ---------- Nội bộ ----------

    // Lấy MaGioHang của khách. $taoNeuChuaCo = true thì tạo giỏ mới nếu khách chưa có.
    private function maGioHang($taoNeuChuaCo = false)
    {
        $this->db->select_prepare('SELECT MaGioHang FROM giohang WHERE MaKH = ?', 'i', $this->maKH);
        $row = $this->db->fetch();
        if ($row) {
            return (int)$row['MaGioHang'];
        }
        if (!$taoNeuChuaCo) {
            return null;
        }
        $this->chay('INSERT INTO giohang (MaKH, NgayTao) VALUES (?, NOW())', 'i', $this->maKH);
        return $this->idMoiNhat();
    }

    // Chỉ giữ topping thật sự tồn tại + loại đồ uống này được phép thêm topping
    private function locTopping(array $ds, $maLoaiSP)
    {
        if (!in_array((int)$maLoaiSP, MA_LOAI_CO_TOPPING, true)) {
            return [];
        }

        $ids = array_values(array_unique(array_filter(array_map('intval', $ds), function ($v) {
            return $v > 0;
        })));
        if (!$ids) {
            return [];
        }

        $in   = implode(',', $ids);
        $rows = $this->db->select(
            "SELECT DISTINCT sp.MaSP
             FROM sanpham sp
             JOIN bienthesanpham bt ON bt.MaSP = sp.MaSP
             WHERE sp.MaLoaiSP = " . (int)MA_LOAI_TOPPING . "
               AND sp.TrangThai = 1 AND bt.TrangThai = 1
               AND sp.MaSP IN ($in)"
        );

        $hopLe = array_map(function ($r) {
            return (int)$r['MaSP'];
        }, $rows);
        sort($hopLe);
        return $hopLe;
    }
}
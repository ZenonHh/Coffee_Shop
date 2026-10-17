<?php

require_once __DIR__ . '/../database/connect.php';
require_once __DIR__ . '/../src/Base-model.php';

class AdminModel extends BaseModel
{
    public function __construct()
    {
        mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);
        parent::__construct();
    }

    private function query(string $sql, string $types = '', array $params = []): mysqli_stmt
    {
        $statement = $this->db->getConnection()->prepare($sql);
        if ($types !== '') {
            $arguments = [$types];
            foreach ($params as &$param) {
                $arguments[] = &$param;
            }
            call_user_func_array([$statement, 'bind_param'], $arguments);
        }
        $statement->execute();
        return $statement;
    }

    private function rows(string $sql, string $types = '', array $params = []): array
    {
        return $this->query($sql, $types, $params)->get_result()->fetch_all(MYSQLI_ASSOC);
    }

    private function row(string $sql): array
    {
        return $this->query($sql)->get_result()->fetch_assoc() ?: [];
    }

    private function value(string $sql): mixed
    {
        $row = $this->query($sql)->get_result()->fetch_row();
        return $row[0] ?? null;
    }

  public function authenticateAdmin(string $username): ?array
    {
        $sql = "SELECT tk.MaTaiKhoan, tk.MaVaiTro, tk.TenDangNhap, tk.MatKhau, tk.HoTen, vt.TenVaiTro
                FROM taikhoan tk
                INNER JOIN vaitro vt ON tk.MaVaiTro = vt.MaVaiTro
                WHERE tk.TenDangNhap = ? 
                  AND tk.TrangThai = 1 
                  AND (LOWER(vt.TenVaiTro) = 'admin' OR LOWER(vt.TenVaiTro) = 'manager' OR LOWER(vt.TenVaiTro) = 'quản lý')
                LIMIT 1";
        
        $conn = $this->db->getConnection();
        $stmt = $conn->prepare($sql);
        if (!$stmt) {
            return null;
        }
        $stmt->bind_param('s', $username);
        $stmt->execute();
        $result = $stmt->get_result()->fetch_assoc();
        $stmt->close();
        return $result ?: null;
    }

    public function findActiveAdmin(int $accountId): ?array
    {
        $sql = "SELECT tk.MaTaiKhoan, tk.MaVaiTro, tk.TenDangNhap, tk.HoTen, vt.TenVaiTro
                FROM taikhoan tk
                INNER JOIN vaitro vt ON tk.MaVaiTro = vt.MaVaiTro
                WHERE tk.MaTaiKhoan = ? 
                  AND tk.TrangThai = 1 
                  AND (LOWER(vt.TenVaiTro) = 'admin' OR LOWER(vt.TenVaiTro) = 'manager' OR LOWER(vt.TenVaiTro) = 'quản lý')
                LIMIT 1";
                
        $conn = $this->db->getConnection();
        $stmt = $conn->prepare($sql);
        if (!$stmt) {
            return null;
        }
        $stmt->bind_param('i', $accountId);
        $stmt->execute();
        $result = $stmt->get_result()->fetch_assoc();
        $stmt->close();
        return $result ?: null;
    }

    public function createProduct(int $categoryId, string $name, string $size, float $price, string $imageName): void
    {
        $conn = $this->db->getConnection();
        $conn->begin_transaction();
        try {
            $this->chay(
                'INSERT INTO SanPham (MaLoaiSP, TenSP, TrangThai, HinhAnh) VALUES (?, ?, 1, ?)',
                'iss',
                $categoryId,
                $name,
                $imageName
            );
            $productId = $this->idMoiNhat();
            $this->chay(
                'INSERT INTO BienTheSanPham (MaSP, KichCo, GiaBan, TrangThai) VALUES (?, ?, ?, 1)',
                'isd',
                $productId,
                $size,
                $price
            );
            $conn->commit();
        } catch (Throwable $exception) {
            $conn->rollback();
            throw $exception;
        }
    }

    public function categoryExists(int $categoryId): bool
    {
        $this->db->select_prepare(
            'SELECT MaLoaiSP FROM LoaiSanPham WHERE MaLoaiSP = ?',
            'i',
            $categoryId
        );
        return $this->db->fetch() !== null;
    }

    public function hideProduct(int $productId): void
    {
        $conn = $this->db->getConnection();
        $conn->begin_transaction();
        try {
            $this->chay('UPDATE SanPham SET TrangThai = 0 WHERE MaSP = ?', 'i', $productId);
            if ($this->db->getConnection()->affected_rows === 0) {
                $this->db->select_prepare('SELECT MaSP FROM SanPham WHERE MaSP = ?', 'i', $productId);
                if (!$this->db->fetch()) {
                    throw new InvalidArgumentException('Không tìm thấy sản phẩm.');
                }
            }
            $this->chay('UPDATE BienTheSanPham SET TrangThai = 0 WHERE MaSP = ?', 'i', $productId);
            $conn->commit();
        } catch (Throwable $exception) {
            $conn->rollback();
            throw $exception;
        }
    }

    public function advanceOrder(int $orderId, string $currentStatus): string
    {
        $nextStatus = match ($currentStatus) {
            '0', 'Chờ xác nhận' => 'Đang giao',
            'Đang giao' => 'Hoàn tất',
            default => null,
        };
        if ($nextStatus === null) {
            throw new InvalidArgumentException('Trạng thái đơn hàng không hợp lệ.');
        }
        if ($this->chay(
            'UPDATE HoaDon SET TrangThai = ? WHERE MaHD = ? AND TrangThai = ?',
            'sis',
            $nextStatus,
            $orderId,
            $currentStatus
        ) !== 1) {
            throw new InvalidArgumentException('Đơn hàng đã được cập nhật hoặc không còn ở trạng thái này.');
        }
        return $nextStatus;
    }

    public function loadPage(string $page): array
    {
        $data = [
            'products' => [],
            'orders' => [],
            'categories' => [],
            'customers' => [],
            'surveys' => [],
            'feedback' => [],
            'dashboard' => [
                'revenue_today' => 0.0,
                'orders_today' => 0,
                'new_customers' => 0,
                'average_order' => 0.0,
                'revenue_week' => 0.0,
                'revenue_previous_week' => 0.0,
                'products_sold' => 0,
                'revenue_days' => [],
                'category_sales' => [],
                'customer_count' => 0,
                'loyal_customer_count' => 0,
                'survey_open' => 0,
                'survey_invitations' => 0,
                'survey_responses' => 0,
                'average_rating' => 0.0,
                'rating_counts' => [],
                'feedback_count' => 0,
            ],
        ];

        if (in_array($page, ['products', 'dashboard'], true)) {
            $data['categories'] = $this->rows(
                'SELECT MaLoaiSP, TenLoaiSP FROM LoaiSanPham ORDER BY ThuTuHienThi, TenLoaiSP'
            );
            $data['products'] = $this->rows(
                'SELECT sp.MaSP, sp.TenSP, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP,
                        MIN(bs.GiaBan) AS GiaBan,
                        GROUP_CONCAT(DISTINCT bs.KichCo ORDER BY bs.GiaBan SEPARATOR ", ") AS KichCo
                 FROM SanPham AS sp
                 INNER JOIN LoaiSanPham AS ls ON ls.MaLoaiSP = sp.MaLoaiSP
                 LEFT JOIN BienTheSanPham AS bs ON bs.MaSP = sp.MaSP
                 GROUP BY sp.MaSP, sp.TenSP, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP
                 ORDER BY sp.MaSP DESC'
            );
        }
        if (in_array($page, ['orders', 'dashboard'], true)) {
            $data['orders'] = $this->rows(
                'SELECT hd.MaHD, hd.HoTenNguoiNhan AS TenNguoiNhan, hd.SdtNguoiNhan, hd.NgayTao, hd.TongTien, hd.TrangThai,
                        COALESCE(SUM(ct.SoLuong), 0) AS SoSanPham
                 FROM HoaDon AS hd
                 LEFT JOIN ChiTietHoaDon AS ct ON ct.MaHD = hd.MaHD
                 GROUP BY hd.MaHD, hd.HoTenNguoiNhan, hd.SdtNguoiNhan, hd.NgayTao, hd.TongTien, hd.TrangThai
                 ORDER BY hd.MaHD DESC'
            );
        }
        if (in_array($page, ['customers', 'dashboard'], true)) {
            $data['customers'] = $this->rows(
                "SELECT kh.MaKH, kh.HoTen, kh.Sdt, kh.HangTV, kh.NgayTao,
                        COUNT(DISTINCT hd.MaHD) AS SoDonHang,
                        COALESCE(SUM(CASE WHEN hd.TrangThai IS NULL OR
                            (hd.TrangThai NOT LIKE '%hủy%' AND hd.TrangThai NOT LIKE '%huy%')
                            THEN hd.TongTien ELSE 0 END), 0) AS TongChiTieu
                 FROM KhachHang AS kh
                 LEFT JOIN HoaDon AS hd ON hd.MaKH = kh.MaKH
                 GROUP BY kh.MaKH, kh.HoTen, kh.Sdt, kh.HangTV, kh.NgayTao
                 ORDER BY kh.MaKH DESC"
            );
            $data['dashboard']['customer_count'] = count($data['customers']);
            $data['dashboard']['loyal_customer_count'] = count(array_filter(
                $data['customers'],
                static fn (array $customer): bool => trim((string) ($customer['HangTV'] ?? '')) !== ''
            ));
            $data['dashboard']['new_customers'] = (int) $this->value(
                'SELECT COUNT(*) FROM KhachHang
                 WHERE NgayTao >= CURDATE() - INTERVAL 29 DAY
                   AND NgayTao < CURDATE() + INTERVAL 1 DAY'
            );
        }
        if (in_array($page, ['surveys', 'dashboard'], true)) {
            $data['surveys'] = $this->rows(
                "SELECT ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc,
                        COUNT(DISTINCT ch.MaCauHoi) AS SoCauHoi,
                        COUNT(DISTINCT lm.MaLoiMoi) AS SoLuotMoi,
                        COUNT(DISTINCT CASE WHEN ct.MaLoiMoi IS NOT NULL THEN lm.MaLoiMoi END) AS SoLuotPhanHoi,
                        CASE WHEN ks.NgayBatDau > NOW() THEN 'Sắp diễn ra'
                             WHEN ks.NgayKetThuc < NOW() THEN 'Đã kết thúc'
                             ELSE 'Đang diễn ra' END AS TrangThaiTinh
                 FROM KhaoSat AS ks
                 LEFT JOIN CauHoiKhaoSat AS ch ON ch.MaKhaoSat = ks.MaKhaoSat
                 LEFT JOIN LoiMoiKhaoSat AS lm ON lm.MaKhaoSat = ks.MaKhaoSat
                 LEFT JOIN CauTraLoi AS ct ON ct.MaLoiMoi = lm.MaLoiMoi
                 GROUP BY ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc
                 ORDER BY ks.MaKhaoSat DESC"
            );
            $data['feedback'] = $this->rows(
                'SELECT ph.MaPhanHoi, ph.NoiDung, ph.DanhGia, ph.NgayGui, ph.TrangThaiXL, kh.HoTen, sp.TenSP
                 FROM PhanHoi AS ph
                 LEFT JOIN KhachHang AS kh ON kh.MaKH = ph.MaKH
                 LEFT JOIN SanPham AS sp ON sp.MaSP = ph.MaSP
                 ORDER BY ph.NgayGui DESC, ph.MaPhanHoi DESC
                 LIMIT 12'
            );
            $dashboard = &$data['dashboard'];
            $dashboard['feedback_count'] = (int) $this->value('SELECT COUNT(*) FROM PhanHoi');
            $dashboard['survey_open'] = count(array_filter(
                $data['surveys'],
                static fn (array $survey): bool => $survey['TrangThaiTinh'] === 'Đang diễn ra'
            ));
            $dashboard['survey_invitations'] = array_sum(array_column($data['surveys'], 'SoLuotMoi'));
            $dashboard['survey_responses'] = array_sum(array_column($data['surveys'], 'SoLuotPhanHoi'));
            foreach ($this->rows(
                'SELECT DanhGia, COUNT(*) AS SoLuot FROM PhanHoi
                 WHERE DanhGia BETWEEN 1 AND 5 GROUP BY DanhGia'
            ) as $rating) {
                $dashboard['rating_counts'][(int) $rating['DanhGia']] = (int) $rating['SoLuot'];
            }
            $ratingTotal = array_sum($dashboard['rating_counts']);
            if ($ratingTotal > 0) {
                foreach ($dashboard['rating_counts'] as $rating => $count) {
                    $dashboard['average_rating'] += $rating * $count / $ratingTotal;
                }
            }
            unset($dashboard);
        }
        if ($page === 'dashboard') {
            $metrics = $this->row(
                "SELECT
                    COALESCE(SUM(CASE WHEN DATE(NgayTao) = CURDATE()
                        AND (TrangThai IS NULL OR TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')
                        THEN TongTien ELSE 0 END), 0) AS DoanhThuHomNay,
                    SUM(CASE WHEN DATE(NgayTao) = CURDATE() THEN 1 ELSE 0 END) AS DonHomNay,
                    COALESCE(AVG(CASE WHEN DATE(NgayTao) = CURDATE()
                        AND (TrangThai IS NULL OR TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')
                        THEN TongTien END), 0) AS GiaTriDonTrungBinh
                 FROM HoaDon"
            );
            $dashboard = &$data['dashboard'];
            $dashboard['revenue_today'] = (float) ($metrics['DoanhThuHomNay'] ?? 0);
            $dashboard['orders_today'] = (int) ($metrics['DonHomNay'] ?? 0);
            $dashboard['average_order'] = (float) ($metrics['GiaTriDonTrungBinh'] ?? 0);
            $sales = $this->row(
                "SELECT
                    COALESCE(SUM(CASE WHEN NgayTao >= CURDATE() - INTERVAL 6 DAY
                        AND NgayTao < CURDATE() + INTERVAL 1 DAY
                        AND (TrangThai IS NULL OR TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')
                        THEN TongTien ELSE 0 END), 0) AS TuanNay,
                    COALESCE(SUM(CASE WHEN NgayTao >= CURDATE() - INTERVAL 13 DAY
                        AND NgayTao < CURDATE() - INTERVAL 6 DAY
                        AND (TrangThai IS NULL OR TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')
                        THEN TongTien ELSE 0 END), 0) AS TuanTruoc
                 FROM HoaDon"
            );
            $dashboard['revenue_week'] = (float) ($sales['TuanNay'] ?? 0);
            $dashboard['revenue_previous_week'] = (float) ($sales['TuanTruoc'] ?? 0);
            $revenueByDay = [];
            foreach ($this->rows(
                "SELECT DATE(NgayTao) AS Ngay, SUM(TongTien) AS DoanhThu
                 FROM HoaDon
                 WHERE NgayTao >= CURDATE() - INTERVAL 6 DAY
                   AND NgayTao < CURDATE() + INTERVAL 1 DAY
                   AND (TrangThai IS NULL OR TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')
                 GROUP BY DATE(NgayTao)"
            ) as $day) {
                $revenueByDay[$day['Ngay']] = (float) $day['DoanhThu'];
            }
            $today = new DateTimeImmutable('today');
            for ($offset = 6; $offset >= 0; $offset--) {
                $date = $today->modify('-' . $offset . ' days');
                $dashboard['revenue_days'][] = [
                    'label' => ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'][(int) $date->format('N') - 1],
                    'revenue' => $revenueByDay[$date->format('Y-m-d')] ?? 0.0,
                ];
            }
            $dashboard['category_sales'] = $this->rows(
                "SELECT ls.TenLoaiSP,
                        COALESCE(SUM(CASE WHEN hd.MaHD IS NOT NULL THEN ct.SoLuong ELSE 0 END), 0) AS SoLuongBan
                 FROM LoaiSanPham AS ls
                 LEFT JOIN SanPham AS sp ON sp.MaLoaiSP = ls.MaLoaiSP
                 LEFT JOIN ChiTietHoaDon AS ct ON ct.MaSP = sp.MaSP
                 LEFT JOIN HoaDon AS hd ON hd.MaHD = ct.MaHD
                   AND (hd.TrangThai IS NULL OR hd.TrangThai NOT LIKE '%hủy%' AND hd.TrangThai NOT LIKE '%huy%')
                 GROUP BY ls.MaLoaiSP, ls.TenLoaiSP
                 ORDER BY SoLuongBan DESC, ls.TenLoaiSP"
            );
            $dashboard['products_sold'] = array_sum(array_map(
                static fn (array $category): int => (int) $category['SoLuongBan'],
                $dashboard['category_sales']
            ));
            unset($dashboard);
        }
        return $data;
    }
}

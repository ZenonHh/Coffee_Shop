<?php
$adminConfig = is_file(__DIR__ . '/../config.local.php')
    ? require __DIR__ . '/../config.local.php'
    : [];
if (!is_array($adminConfig)) {
    $adminConfig = [];
}
if (session_status() !== PHP_SESSION_ACTIVE) {
    session_set_cookie_params([
        'httponly' => true,
        'secure' => !empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off',
        'samesite' => 'Lax',
    ]);
    session_start();
}
if (!isset($_SESSION['admin_csrf'])) {
    $_SESSION['admin_csrf'] = bin2hex(random_bytes(32));
}
function admin_redirect(string $location): never
{
    header('Location: ' . $location, true, 303);
    exit;
}

$adminMessage = $_SESSION['admin_message'] ?? '';
$adminMessageType = $_SESSION['admin_message_type'] ?? 'success';
unset($_SESSION['admin_message'], $_SESSION['admin_message_type']);
$adminError = '';
$isAdminAuthenticated = ($_SESSION['admin_authenticated'] ?? false) === true;

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $postedToken = $_POST['csrf_token'] ?? '';
    if (!is_string($postedToken) || !hash_equals($_SESSION['admin_csrf'], $postedToken)) {
        $adminError = 'Phiên làm việc không hợp lệ. Vui lòng tải lại trang.';
    } else {
        $action = $_POST['action'] ?? '';
        if ($action === 'login') {
            $adminUsername = $adminConfig['admin_username'] ?? getenv('COFFEE_ADMIN_USERNAME') ?: '';
            $adminPasswordHash = $adminConfig['admin_password_hash'] ?? getenv('COFFEE_ADMIN_PASSWORD_HASH') ?: '';
            $enteredUsername = is_string($_POST['username'] ?? null) ? $_POST['username'] : '';
            $enteredPassword = is_string($_POST['password'] ?? null) ? $_POST['password'] : '';
            if ($adminPasswordHash === '') {
                $adminError = 'Chưa cấu hình mật khẩu quản trị. Hãy tạo password hash và đặt trong config.local.php.';
            } elseif (
                is_string($adminUsername)
                && is_string($adminPasswordHash)
                && hash_equals($adminUsername, $enteredUsername)
                && password_verify($enteredPassword, $adminPasswordHash)
            ) {
                session_regenerate_id(true);
                $_SESSION['admin_authenticated'] = true;
                $_SESSION['admin_csrf'] = bin2hex(random_bytes(32));
                admin_redirect('index.php');
            } else {
                $adminError = 'Tên đăng nhập hoặc mật khẩu không chính xác.';
            }
        } elseif ($action === 'logout') {
            unset($_SESSION['admin_authenticated']);
            session_regenerate_id(true);
            $_SESSION['admin_csrf'] = bin2hex(random_bytes(32));
            admin_redirect('index.php');
        } elseif (!$isAdminAuthenticated) {
            http_response_code(403);
            $adminError = 'Vui lòng đăng nhập với tài khoản quản trị.';
        } else {
            try {
                require_once __DIR__ . '/../connect.php';
                if ($action === 'create_product') {
                    $name = is_string($_POST['name'] ?? null) ? trim($_POST['name']) : '';
                    $categoryId = filter_var($_POST['category_id'] ?? null, FILTER_VALIDATE_INT);
                    $price = filter_var($_POST['price'] ?? null, FILTER_VALIDATE_FLOAT);
                    $size = is_string($_POST['size'] ?? null) ? trim($_POST['size']) : '';
                    $image = $_FILES['image'] ?? null;

                    if ($name === '' || mb_strlen($name, 'UTF-8') > 100 || !$categoryId || $price === false || $price < 0 || strlen($size) > 10) {
                        throw new InvalidArgumentException('Vui lòng nhập tên, danh mục và giá bán hợp lệ; kích cỡ tối đa 10 ký tự.');
                    }
                    if (!is_array($image) || ($image['error'] ?? UPLOAD_ERR_NO_FILE) !== UPLOAD_ERR_OK) {
                        throw new InvalidArgumentException('Vui lòng chọn ảnh sản phẩm.');
                    }
                    if (!is_string($image['tmp_name'] ?? null) || !is_int($image['size'] ?? null) || $image['size'] < 1 || $image['size'] > 5 * 1024 * 1024 || !is_uploaded_file($image['tmp_name'])) {
                        throw new InvalidArgumentException('Ảnh phải có dung lượng tối đa 5 MB.');
                    }

                    $imageInfo = new finfo(FILEINFO_MIME_TYPE);
                    $mimeType = $imageInfo->file($image['tmp_name']);
                    $imageExtensions = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/webp' => 'webp'];
                    if (!isset($imageExtensions[$mimeType]) || @getimagesize($image['tmp_name']) === false) {
                        throw new InvalidArgumentException('Ảnh chỉ được dùng định dạng JPG, PNG hoặc WEBP hợp lệ.');
                    }

                    $categoryCheck = $conn->prepare('SELECT 1 FROM LoaiSanPham WHERE MaLoaiSP = :categoryId');
                    $categoryCheck->execute(['categoryId' => $categoryId]);
                    if (!$categoryCheck->fetchColumn()) {
                        throw new InvalidArgumentException('Danh mục sản phẩm không hợp lệ.');
                    }

                    $imageName = bin2hex(random_bytes(16)) . '.' . $imageExtensions[$mimeType];
                    $imagePath = __DIR__ . '/../images/' . $imageName;
                    if (!move_uploaded_file($image['tmp_name'], $imagePath)) {
                        throw new RuntimeException('Không thể lưu ảnh sản phẩm vào thư mục images.');
                    }
                    try {
                        $insertProduct = $conn->prepare(
                            'INSERT INTO SanPham (MaLoaiSP, TenSP, KichCo, GiaBan, TrangThai, HinhAnh)
                             VALUES (:categoryId, :name, :size, :price, 1, :image)'
                        );
                        $insertProduct->execute([
                            'categoryId' => $categoryId,
                            'name' => $name,
                            'size' => $size !== '' ? $size : null,
                            'price' => $price,
                            'image' => $imageName,
                        ]);
                    } catch (Throwable $exception) {
                        if (is_file($imagePath)) {
                            unlink($imagePath);
                        }
                        throw $exception;
                    }
                    $_SESSION['admin_message'] = 'Đã thêm sản phẩm.';
                } elseif ($action === 'delete_product') {
                    $productId = filter_var($_POST['product_id'] ?? null, FILTER_VALIDATE_INT);
                    if (!$productId || $productId < 1) {
                        throw new InvalidArgumentException('Mã sản phẩm không hợp lệ.');
                    }
                    $findProduct = $conn->prepare('SELECT HinhAnh FROM SanPham WHERE MaSP = :productId');
                    $findProduct->execute(['productId' => $productId]);
                    $productImage = $findProduct->fetchColumn();
                    if ($productImage === false) {
                        throw new InvalidArgumentException('Không tìm thấy sản phẩm.');
                    }
                    $usageCheck = $conn->prepare('SELECT COUNT(*) FROM ChiTietHoaDon WHERE MaSP = :productId');
                    $usageCheck->execute(['productId' => $productId]);
                    if ((int) $usageCheck->fetchColumn() > 0) {
                        $deleteProduct = $conn->prepare('UPDATE SanPham SET TrangThai = 0 WHERE MaSP = :productId');
                        $deleteProduct->execute(['productId' => $productId]);
                        $_SESSION['admin_message'] = 'Sản phẩm đã có trong hóa đơn nên được ẩn khỏi menu để giữ lịch sử đơn hàng.';
                    } else {
                        $deleteProduct = $conn->prepare('DELETE FROM SanPham WHERE MaSP = :productId');
                        $deleteProduct->execute(['productId' => $productId]);
                        $imageReferenceCheck = $conn->prepare('SELECT COUNT(*) FROM SanPham WHERE HinhAnh = :imageName');
                        $imageReferenceCheck->execute(['imageName' => $productImage]);
                        if ((int) $imageReferenceCheck->fetchColumn() === 0 && is_string($productImage) && basename($productImage) === $productImage) {
                            $imagePath = __DIR__ . '/../images/' . $productImage;
                            if (is_file($imagePath)) {
                                unlink($imagePath);
                            }
                        }
                        $_SESSION['admin_message'] = 'Đã xóa sản phẩm.';
                    }
                } elseif ($action === 'advance_order') {
                    $orderId = filter_var($_POST['order_id'] ?? null, FILTER_VALIDATE_INT);
                    $currentStatus = $_POST['current_status'] ?? '';
                    $nextStatus = match ($currentStatus) {
                        'Chờ xác nhận' => 'Đang giao',
                        'Đang giao' => 'Hoàn tất',
                        default => null,
                    };
                    if (!$orderId || $orderId < 1 || $nextStatus === null) {
                        throw new InvalidArgumentException('Trạng thái đơn hàng không hợp lệ.');
                    }
                    $updateOrder = $conn->prepare(
                        'UPDATE HoaDon SET TrangThai = :nextStatus
                         WHERE MaHD = :orderId AND TrangThai = :currentStatus'
                    );
                    $updateOrder->execute([
                        'nextStatus' => $nextStatus,
                        'orderId' => $orderId,
                        'currentStatus' => $currentStatus,
                    ]);
                    if ($updateOrder->rowCount() !== 1) {
                        throw new InvalidArgumentException('Đơn hàng đã được cập nhật hoặc không còn ở trạng thái này.');
                    }
                    $_SESSION['admin_message'] = 'Đã chuyển đơn hàng sang trạng thái ' . $nextStatus . '.';
                }
                $_SESSION['admin_message_type'] = 'success';
                admin_redirect('index.php?page=' . rawurlencode((string) ($_GET['page'] ?? 'dashboard')));
            } catch (InvalidArgumentException $exception) {
                $adminError = $exception->getMessage();
            } catch (Throwable $exception) {
                error_log('Admin operation failed: ' . $exception->getMessage());
                $adminError = 'Không thể lưu thay đổi do lỗi hệ thống. Hãy kiểm tra cấu hình kết nối và nhật ký PHP.';
            }
        }
    }
}
$pages = [
    'dashboard' => ['title' => 'Tổng quan', 'eyebrow' => 'TỔNG QUA'],
    'products' => ['title' => 'Sản phẩm', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
    'orders' => ['title' => 'Đơn hàng', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
    'customers' => ['title' => 'Khách hàng', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
    'surveys' => ['title' => 'Khảo sát ý kiến', 'eyebrow' => 'THẤU HIỂU KHÁCH HÀNG'],
];

$page = $_GET['page'] ?? 'dashboard';
if (!is_string($page) || !isset($pages[$page])) {
    $page = 'dashboard';
}
$title = $pages[$page]['title'];

$products = [];
$orders = [];
$categories = [];
$customers = [];
$surveys = [];
$feedback = [];
$dashboard = [
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
];
if ($isAdminAuthenticated) {
    try {
        require_once __DIR__ . '/../connect.php';
        if (in_array($page, ['products', 'dashboard'], true)) {
            $categoryQuery = $conn->prepare('SELECT MaLoaiSP, TenLoaiSP FROM LoaiSanPham ORDER BY ThuTuHienThi, TenLoaiSP');
            $categoryQuery->execute();
            $categories = $categoryQuery->fetchAll();

            $productQuery = $conn->prepare(
                'SELECT sp.MaSP, sp.TenSP, sp.KichCo, sp.GiaBan, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP
                 FROM SanPham AS sp
                 INNER JOIN LoaiSanPham AS ls ON ls.MaLoaiSP = sp.MaLoaiSP
                 ORDER BY sp.MaSP DESC'
            );
            $productQuery->execute();
            $products = $productQuery->fetchAll();
        }
        if (in_array($page, ['orders', 'dashboard'], true)) {
            $orderQuery = $conn->prepare(
                'SELECT hd.MaHD, hd.TenNguoiNhan, hd.SdtNguoiNhan, hd.NgayTao, hd.TongTien, hd.TrangThai,
                        COALESCE(SUM(ct.SoLuong), 0) AS SoSanPham
                 FROM HoaDon AS hd
                 LEFT JOIN ChiTietHoaDon AS ct ON ct.MaHD = hd.MaHD
                 GROUP BY hd.MaHD, hd.TenNguoiNhan, hd.SdtNguoiNhan, hd.NgayTao, hd.TongTien, hd.TrangThai
                 ORDER BY hd.MaHD DESC'
            );
            $orderQuery->execute();
            $orders = $orderQuery->fetchAll();
        }
        if (in_array($page, ['customers', 'dashboard'], true)) {
            $customerQuery = $conn->prepare(
                "SELECT kh.MaKH, kh.HoTen, kh.Sdt, kh.HangTV, kh.NgayTao,
                        COUNT(DISTINCT hd.MaHD) AS SoDonHang,
                        COALESCE(SUM(CASE WHEN hd.TrangThai IS NULL OR (hd.TrangThai NOT LIKE '%hủy%' AND hd.TrangThai NOT LIKE '%huy%')
                                          THEN hd.TongTien ELSE 0 END), 0) AS TongChiTieu
                 FROM KhachHang AS kh
                 LEFT JOIN HoaDon AS hd ON hd.MaKH = kh.MaKH
                 GROUP BY kh.MaKH, kh.HoTen, kh.Sdt, kh.HangTV, kh.NgayTao
                 ORDER BY kh.MaKH DESC"
            );
            $customerQuery->execute();
            $customers = $customerQuery->fetchAll();
            $dashboard['customer_count'] = count($customers);
            $dashboard['loyal_customer_count'] = count(array_filter(
                $customers,
                static fn (array $customer): bool => trim((string) ($customer['HangTV'] ?? '')) !== ''
            ));
            $newCustomerQuery = $conn->prepare(
                'SELECT COUNT(*) FROM KhachHang
                 WHERE NgayTao >= CURDATE() - INTERVAL 29 DAY
                   AND NgayTao < CURDATE() + INTERVAL 1 DAY'
            );
            $newCustomerQuery->execute();
            $dashboard['new_customers'] = (int) $newCustomerQuery->fetchColumn();
        }
        if (in_array($page, ['surveys', 'dashboard'], true)) {
            $surveyQuery = $conn->prepare(
                "SELECT ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc,
                        COUNT(DISTINCT ch.MaCauHoi) AS SoCauHoi,
                        COUNT(DISTINCT lm.MaLoiMoi) AS SoLuotMoi,
                        COUNT(DISTINCT CASE WHEN ct.MaLoiMoi IS NOT NULL THEN lm.MaLoiMoi END) AS SoLuotPhanHoi,
                        CASE WHEN ks.NgayBatDau IS NULL OR ks.NgayKetThuc IS NULL THEN 'Chưa thiết lập'
                             WHEN ks.NgayBatDau > NOW() THEN 'Sắp diễn ra'
                             WHEN ks.NgayKetThuc < NOW() THEN 'Đã kết thúc'
                             ELSE 'Đang diễn ra' END AS TrangThaiTinh
                 FROM KhaoSat AS ks
                 LEFT JOIN CauHoiKhaoSat AS ch ON ch.MaKhaoSat = ks.MaKhaoSat
                 LEFT JOIN LoiMoiKhaoSat AS lm ON lm.MaKhaoSat = ks.MaKhaoSat
                 LEFT JOIN CauTraLoi AS ct ON ct.MaLoiMoi = lm.MaLoiMoi
                 GROUP BY ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc
                 ORDER BY ks.MaKhaoSat DESC"
            );
            $surveyQuery->execute();
            $surveys = $surveyQuery->fetchAll();

            $feedbackQuery = $conn->prepare(
                'SELECT ph.MaPhanHoi, ph.NoiDung, ph.DanhGia, ph.NgayGui, kh.HoTen, sp.TenSP
                 FROM PhanHoi AS ph
                 LEFT JOIN KhachHang AS kh ON kh.MaKH = ph.MaKH
                 LEFT JOIN SanPham AS sp ON sp.MaSP = ph.MaSP
                 ORDER BY ph.NgayGui DESC, ph.MaPhanHoi DESC
                 LIMIT 12'
            );
            $feedbackQuery->execute();
            $feedback = $feedbackQuery->fetchAll();
            $feedbackCountQuery = $conn->prepare('SELECT COUNT(*) FROM PhanHoi');
            $feedbackCountQuery->execute();
            $dashboard['feedback_count'] = (int) $feedbackCountQuery->fetchColumn();
            $dashboard['survey_open'] = count(array_filter(
                $surveys,
                static fn (array $survey): bool => $survey['TrangThaiTinh'] === 'Đang diễn ra'
            ));
            $dashboard['survey_invitations'] = array_sum(array_map(
                static fn (array $survey): int => (int) $survey['SoLuotMoi'],
                $surveys
            ));
            $dashboard['survey_responses'] = array_sum(array_map(
                static fn (array $survey): int => (int) $survey['SoLuotPhanHoi'],
                $surveys
            ));

            $ratingQuery = $conn->prepare(
                'SELECT DanhGia, COUNT(*) AS SoLuot
                 FROM PhanHoi
                 WHERE DanhGia BETWEEN 1 AND 5
                 GROUP BY DanhGia'
            );
            $ratingQuery->execute();
            foreach ($ratingQuery->fetchAll() as $ratingRow) {
                $dashboard['rating_counts'][(int) $ratingRow['DanhGia']] = (int) $ratingRow['SoLuot'];
            }
            $ratingTotal = array_sum($dashboard['rating_counts']);
            if ($ratingTotal > 0) {
                $dashboard['average_rating'] = array_sum(array_map(
                    static fn (int $rating, int $count): int => $rating * $count,
                    array_keys($dashboard['rating_counts']),
                    array_values($dashboard['rating_counts'])
                )) / $ratingTotal;
            }
        }
        if ($page === 'dashboard') {
            $metricQuery = $conn->prepare(
                "SELECT
                    COALESCE(SUM(CASE WHEN DATE(NgayTao) = CURDATE()
                        AND (TrangThai IS NULL OR (TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')) THEN TongTien ELSE 0 END), 0) AS DoanhThuHomNay,
                    SUM(CASE WHEN DATE(NgayTao) = CURDATE() THEN 1 ELSE 0 END) AS DonHomNay,
                    COALESCE(AVG(CASE WHEN DATE(NgayTao) = CURDATE()
                        AND (TrangThai IS NULL OR (TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%')) THEN TongTien END), 0) AS GiaTriDonTrungBinh
                 FROM HoaDon"
            );
            $metricQuery->execute();
            $metrics = $metricQuery->fetch() ?: [];
            $dashboard['revenue_today'] = (float) ($metrics['DoanhThuHomNay'] ?? 0);
            $dashboard['orders_today'] = (int) ($metrics['DonHomNay'] ?? 0);
            $dashboard['average_order'] = (float) ($metrics['GiaTriDonTrungBinh'] ?? 0);

            $salesQuery = $conn->prepare(
                "SELECT COALESCE(SUM(CASE WHEN NgayTao >= CURDATE() - INTERVAL 6 DAY
                                           AND NgayTao < CURDATE() + INTERVAL 1 DAY
                                           AND (TrangThai IS NULL OR (TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%'))
                                      THEN TongTien ELSE 0 END), 0) AS TuanNay,
                        COALESCE(SUM(CASE WHEN NgayTao >= CURDATE() - INTERVAL 13 DAY
                                           AND NgayTao < CURDATE() - INTERVAL 6 DAY
                                           AND (TrangThai IS NULL OR (TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%'))
                                      THEN TongTien ELSE 0 END), 0) AS TuanTruoc
                 FROM HoaDon"
            );
            $salesQuery->execute();
            $sales = $salesQuery->fetch() ?: [];
            $dashboard['revenue_week'] = (float) ($sales['TuanNay'] ?? 0);
            $dashboard['revenue_previous_week'] = (float) ($sales['TuanTruoc'] ?? 0);

            $dailyRevenueQuery = $conn->prepare(
                "SELECT DATE(NgayTao) AS Ngay, SUM(TongTien) AS DoanhThu
                 FROM HoaDon
                 WHERE NgayTao >= CURDATE() - INTERVAL 6 DAY
                   AND NgayTao < CURDATE() + INTERVAL 1 DAY
                   AND (TrangThai IS NULL OR (TrangThai NOT LIKE '%hủy%' AND TrangThai NOT LIKE '%huy%'))
                 GROUP BY DATE(NgayTao)"
            );
            $dailyRevenueQuery->execute();
            $revenueByDay = [];
            foreach ($dailyRevenueQuery->fetchAll() as $dayRow) {
                $revenueByDay[(new DateTimeImmutable((string) $dayRow['Ngay']))->format('Y-m-d')] = (float) $dayRow['DoanhThu'];
            }
            $today = new DateTimeImmutable('today');
            for ($offset = 6; $offset >= 0; $offset--) {
                $date = $today->modify('-' . $offset . ' days');
                $dashboard['revenue_days'][] = [
                    'label' => ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'C'][(int) $date->format('') - 1],
                    'revenue' => $revenueByDay[$date->format('Y-m-d')] ?? 0.0,
                ];
            }

            $categorySalesQuery = $conn->prepare(
                "SELECT ls.TenLoaiSP, COALESCE(SUM(CASE WHEN hd.MaHD IS NOT NULL THEN ct.SoLuong ELSE 0 END), 0) AS SoLuongBan
                 FROM LoaiSanPham AS ls
                 LEFT JOIN SanPham AS sp ON sp.MaLoaiSP = ls.MaLoaiSP
                 LEFT JOIN ChiTietHoaDon AS ct ON ct.MaSP = sp.MaSP
                 LEFT JOIN HoaDon AS hd ON hd.MaHD = ct.MaHD
                   AND (hd.TrangThai IS NULL OR (hd.TrangThai NOT LIKE '%hủy%' AND hd.TrangThai NOT LIKE '%huy%'))
                 GROUP BY ls.MaLoaiSP, ls.TenLoaiSP
                 ORDER BY SoLuongBan DESC, ls.TenLoaiSP"
            );
            $categorySalesQuery->execute();
            $dashboard['category_sales'] = $categorySalesQuery->fetchAll();
            $dashboard['products_sold'] = array_sum(array_map(
                static fn (array $category): int => (int) $category['SoLuongBan'],
                $dashboard['category_sales']
            ));

        }
    } catch (Throwable $exception) {
        error_log('Admin data load failed: ' . $exception->getMessage());
        $adminError = 'Không thể tải dữ liệu quản trị. Hãy kiểm tra kết nối MySQL và cấu trúc cơ sở dữ liệu.';
    }
}

function money(int|float|string $amount): string
{
    return number_format((float) $amount, 0, ',', '.') . 'đ';
}

function status_class(string $status): string
{
    if (in_array($status, ['Hoàn thành', 'Hoàn tất', 'Đang bán', 'Thân thiết', 'Đang diễn ra', 'Đang mở'], true)) {
        return 'success';
    }
    if (in_array($status, ['Đang giao', 'Đang chuẩn bị', 'Thành viên'], true)) {
        return 'info';
    }
    if (in_array($status, ['Chờ xác nhận', 'Sắp hết', 'Sắp diễn ra'], true)) {
        return 'warning';
    }
    if (in_array($status, ['Đã hủy', 'Hết hàng', 'Đã kết thúc'], true)) {
        return 'muted';
    }
    return 'muted';
}

function nav_link(string $key, string $label, string $icon, string $activePage): void
{
    $active = $key === $activePage ? ' active' : '';
    echo '<a class="side-link' . $active . '" href="?page=' . htmlspecialchars($key, ENT_QUOTES, 'UTF-8') . '">'
        . '<span class="side-icon" aria-hidden="true">' . $icon . '</span>'
        . '<span>' . htmlspecialchars($label, ENT_QUOTES, 'UTF-8') . '</span></a>';
}
?>
<?php if (!$isAdminAuthenticated): ?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Đăng nhập | Coffee Blend Admin</title>
    <link rel="stylesheet" href="../css/open-iconic-bootstrap.min.css">
    <link rel="stylesheet" href="admin.css?v=2">
</head>
<body>
<main class="admin-shell"><div class="main-content">
    <section class="panel" style="max-width:460px;margin:10vh auto;padding:32px">
        <div class="eyebrow">COFFEE BLEND · ADMIN</div>
        <h1>Đăng nhập quản trị</h1>
        <p>Đăng nhập để quản lý sản phẩm và đơn hàng.</p>
        <?php if ($adminError !== ''): ?><div class="alert alert-danger" role="alert"><?= htmlspecialchars($adminError, ENT_QUOTES, 'UTF-8') ?></div><?php endif; ?>
        <form method="post" action="index.php" class="dialog-form">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($_SESSION['admin_csrf'], ENT_QUOTES, 'UTF-8') ?>">
            <input type="hidden" name="action" value="login">
            <label>Tên đăng nhập<input name="username" type="text" autocomplete="username" required></label>
            <label>Mật khẩu<input name="password" type="password" autocomplete="current-password" required></label>
            <button class="button button-primary" type="submit">Đăng nhập</button>
        </form>
        <p class="dialog-note"><?= empty($adminConfig['admin_password_hash']) ? 'Cần cấu hình admin_password_hash trong config.local.php trước khi đăng nhập.' : 'Tài khoản quản trị được cấu hình cục bộ; mật khẩu không được hiển thị tại đây.' ?></p>
    </section>
</div></main>
</body>
</html>
<?php exit; endif; ?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="Khu vực quản trị Coffee Blend">
    <title><?= htmlspecialchars($title, ENT_QUOTES, 'UTF-8') ?> | Coffee Blend Admin</title>
    <link rel="stylesheet" href="../css/open-iconic-bootstrap.min.css">
    <link rel="stylesheet" href="admin.css?v=2">
</head>
<body>
<div class="admin-shell">
    <aside class="sidebar" id="sidebar">
        <a class="brand" href="index.php" aria-label="Coffee Blend Admin">
            <span class="brand-mark">C</span>
            <span class="brand-copy">Coffee<small>Blend · ADMIN</small></span>
        </a>

        <div class="side-caption">MENU CHÍNH</div>
        <nav class="side-nav" aria-label="Điều hướng quản trị">
            <?php
            nav_link('dashboard', 'Tổng quan', '◫', $page);
            nav_link('products', 'Sản phẩm', '▤', $page);
            nav_link('orders', 'Đơn hàng', '▣', $page);
            nav_link('customers', 'Khách hàng', '♙', $page);
            nav_link('surveys', 'Khảo sát ý kiến', '▧', $page);
            ?>
        </nav>

        <div class="side-caption side-caption-spaced">HỆ THỐNG</div>
        <form method="post" action="index.php">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($_SESSION['admin_csrf'], ENT_QUOTES, 'UTF-8') ?>">
            <input type="hidden" name="action" value="logout">
            <button class="side-link" type="submit"><span class="side-icon" aria-hidden="true">↪</span><span>Đăng xuất</span></button>
        </form>

        <div class="sidebar-bottom">
            <div class="help-card">
                <span class="help-mark">?</span>
                <strong>Cần hỗ trợ?</strong>
                <span>Liên hệ quản trị viên hệ thống.</span>
            </div>
            <a class="store-link" href="../index.php"><span aria-hidden="true">↗</span> Xem website cửa hàng</a>
            <div class="admin-profile">
                <div class="avatar avatar-admin">AD</div>
                <div class="profile-copy"><strong>Quản trị viên</strong><span>admin@coffeeblend.vn</span></div>
                <span class="profile-dots" aria-hidden="true">···</span>
            </div>
        </div>
    </aside>

    <main class="main-area">
        <?php if ($adminError !== ''): ?><div class="alert alert-danger" role="alert"><?= htmlspecialchars($adminError, ENT_QUOTES, 'UTF-8') ?></div><?php endif; ?>
        <?php if ($adminMessage !== ''): ?><div class="alert alert-<?= htmlspecialchars($adminMessageType, ENT_QUOTES, 'UTF-8') ?>" role="status"><?= htmlspecialchars($adminMessage, ENT_QUOTES, 'UTF-8') ?></div><?php endif; ?>
        <header class="topbar">
            <button class="icon-button menu-toggle" id="menuToggle" type="button" aria-label="Mở menu" aria-expanded="false">☰</button>
            <div class="breadcrumb"><span>Admin</span><span class="crumb-separator">/</span><strong><?= htmlspecialchars($title, ENT_QUOTES, 'UTF-8') ?></strong></div>
            <div class="topbar-actions">
                <label class="global-search">
                    <span aria-hidden="true">⌕</span>
                    <input type="search" id="tableSearch" placeholder="Tìm kiếm..." aria-label="Tìm kiếm trong bảng">
                    <kbd>⌘ K</kbd>
                </label>
                <button class="icon-button notification-button" type="button" aria-label="Thông báo">♧<i></i></button>
                <div class="topbar-divider"></div>
                <div class="topbar-user"><div class="avatar avatar-admin">AD</div><span>Admin</span><span class="chevron">⌄</span></div>
            </div>
        </header>

        <div class="page-content">
            <?php if ($page === 'dashboard'): ?>
                <?php
                $revenueChartMaximum = max(1.0, ...array_map(
                    static fn (array $day): float => $day['revenue'],
                    $dashboard['revenue_days']
                ));
                $revenueAxisMaximum = $revenueChartMaximum > 0
                    ? max(1000000, (int) (ceil($revenueChartMaximum / 4 / 1000000) * 4000000))
                    : 0;
                $revenueChange = $dashboard['revenue_previous_week'] > 0
                    ? ($dashboard['revenue_week'] - $dashboard['revenue_previous_week']) / $dashboard['revenue_previous_week'] * 100
                    : null;
                $categoryMaximum = max(1, ...array_map(
                    static fn (array $category): int => (int) $category['SoLuongBan'],
                    $dashboard['category_sales']
                ));
                ?>
                <section class="welcome-row">
                    <div>
                        <div class="eyebrow">TỔNG QUAN</div>
                        <h1>Tổng quan kinh doanh <span aria-hidden="true">☀</span></h1>
                        <p class="page-subtitle">Đây là tình hình kinh doanh của cửa hàng hôm nay.</p>
                    </div>
                    <div class="button button-outline"><span aria-hidden="true">▦</span> <?= (new DateTimeImmutable('today'))->format('d/m/Y') ?></div>
                </section>

                <section class="stats-grid" aria-label="Chỉ số kinh doanh">
                    <article class="stat-card">
                        <div class="stat-top"><span>Doanh thu hôm nay</span><span class="stat-icon revenue-icon">₫</span></div>
                        <div class="stat-value"><?= money($dashboard['revenue_today']) ?></div>
                        <div class="stat-foot"><span>Doanh thu hóa đơn trong ngày</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Đơn hàng</span><span class="stat-icon order-icon">▣</span></div>
                        <div class="stat-value"><?= $dashboard['orders_today'] ?></div>
                        <div class="stat-foot"><span>Đơn được tạo hôm nay</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Khách hàng mới</span><span class="stat-icon customer-icon">♙</span></div>
                        <div class="stat-value"><?= $dashboard['new_customers'] ?></div>
                        <div class="stat-foot"><span>Khách đăng ký trong 30 ngày gần nhất</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Giá trị đơn trung bình</span><span class="stat-icon average-icon">⌁</span></div>
                        <div class="stat-value"><?= money($dashboard['average_order']) ?></div>
                        <div class="stat-foot"><span>Giá trị trung bình đơn hôm nay</span></div>
                    </article>
                </section>

                <section class="dashboard-grid">
                    <article class="panel revenue-panel">
                        <div class="panel-heading">
                            <div><h2>Doanh thu</h2><p>Biến động doanh thu trong tuần này</p></div>
                            <button class="select-button" type="button">Tuần này <span aria-hidden="true">⌄</span></button>
                        </div>
                        <div class="chart-summary"><strong><?= money($dashboard['revenue_week']) ?></strong>
                            <?php if ($revenueChange !== null): ?><span class="<?= $revenueChange >= 0 ? 'trend-up' : 'trend-down' ?>"><?= $revenueChange >= 0 ? '↗' : '↘' ?> <?= number_format(abs($revenueChange), 1, ',', '.') ?>%</span><small>so với 7 ngày trước đó</small><?php else: ?><small>Chưa có dữ liệu 7 ngày trước đó để so sánh.</small><?php endif; ?>
                        </div>
                        <div class="chart" role="img" aria-label="Biểu đồ doanh thu trong tuần: thứ Hai đến Chủ nhật">
                            <div class="chart-y-axis"><?php for ($tick = 4; $tick >= 0; $tick--): ?><span><?= number_format($revenueAxisMaximum * $tick / 4 / 1000000, 1, ',', '.') ?>tr</span><?php endfor; ?></div>
                            <div class="chart-plot">
                                <div class="chart-grid-lines"><i></i><i></i><i></i><i></i><i></i></div>
                                <div class="chart-bars">
                                    <?php foreach ($dashboard['revenue_days'] as $day): $height = $day['revenue'] > 0 ? max(2, (int) round($day['revenue'] / $revenueAxisMaximum * 100)) : 0; ?>
                                        <div class="chart-column"><span class="bar-value"><?= number_format($day['revenue'] / 1000000, 1, ',', '.') ?>tr</span><div class="bar" style="height: <?= $height ?>%"></div><span class="bar-label"><?= $day['label'] ?></span></div>
                                    <?php endforeach; ?>
                                </div>
                            </div>
                        </div>
                    </article>

                    <article class="panel category-panel">
                        <div class="panel-heading"><div><h2>Số món theo danh mục</h2><p>Tổng số lượng trong chi tiết hóa đơn</p></div><a class="text-link" href="?page=products">Chi tiết <span aria-hidden="true">→</span></a></div>
                        <div class="category-total"><strong><?= $dashboard['products_sold'] ?></strong><span>món đã bán</span></div>
                        <div class="category-list">
                            <?php foreach ($dashboard['category_sales'] as $categorySale): $soldCount = (int) $categorySale['SoLuongBan']; $share = $dashboard['products_sold'] > 0 ? (int) round($soldCount / $dashboard['products_sold'] * 100) : 0; $barWidth = (int) round($soldCount / $categoryMaximum * 100); ?>
                            <div class="category-row"><span class="category-dot dot-other"></span><span><?= htmlspecialchars($categorySale['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></span><strong><?= $soldCount ?></strong><div class="mini-track"><i style="width: <?= $barWidth ?>%"></i></div><small><?= $share ?>%</small></div>
                            <?php endforeach; ?>
                        </div>
                        <?php if ($dashboard['products_sold'] === 0): ?><div class="category-note">Chưa có sản phẩm nào trong chi tiết hóa đơn.</div><?php endif; ?>
                    </article>
                </section>

                <section class="panel table-panel">
                    <div class="panel-heading table-heading">
                        <div><h2>Đơn hàng gần đây</h2><p>Theo dõi các đơn hàng mới nhất của cửa hàng.</p></div>
                        <a class="button button-outline button-small" href="?page=orders">Xem tất cả <span aria-hidden="true">→</span></a>
                    </div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>MÃ ĐƠN</th><th>KHÁCH HÀNG</th><th>THỜI GIAN</th><th>SẢN PHẨM</th><th>TỔNG TIỀN</th><th>TRẠNG THÁI</th><th></th></tr></thead>
                            <tbody>
                            <?php foreach (array_slice($orders, 0, 4) as $order): ?>
                                <?php $orderStatus = (string) ($order['TrangThai'] ?? 'Chờ xác nhận'); ?>
                                <tr data-search-row>
                                    <td class="order-id">#<?= (int) $order['MaHD'] ?></td>
                                    <td><?= htmlspecialchars($order['TenNguoiNhan'] ?? 'Khách hàng', ENT_QUOTES, 'UTF-8') ?></td>
                                    <td class="muted-cell"><?= htmlspecialchars((string) ($order['NgayTao'] ?? ''), ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><?= (int) $order['SoSanPham'] ?> món</td><td class="money-cell"><?= money($order['TongTien'] ?? 0) ?></td>
                                    <td><span class="status status-<?= status_class($orderStatus) ?>"><i></i><?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td></td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($orders === []): ?><tr><td colspan="7" class="muted-cell">Chưa có đơn hàng trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                </section>

            <?php elseif ($page === 'products'): ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Sản phẩm</h1><p class="page-subtitle">Quản lý danh mục và tình trạng sản phẩm của cửa hàng.</p></div>
                    <button class="button button-primary" type="button" id="openProductDialog"><span aria-hidden="true">＋</span> Thêm sản phẩm</button>
                </section>
                <section class="stats-grid compact-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng sản phẩm</span><span class="stat-icon revenue-icon">▤</span></div><div class="stat-value"><?= count($products) ?></div><div class="stat-foot"><span>Sản phẩm trong cơ sở dữ liệu</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đang kinh doanh</span><span class="stat-icon customer-icon">✓</span></div><div class="stat-value"><?= count(array_filter($products, static fn (array $product): bool => (bool) ($product['TrangThai'] ?? 1))) ?></div><div class="stat-foot"><span>Sản phẩm đang hiển thị</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Danh mục</span><span class="stat-icon average-icon">▧</span></div><div class="stat-value"><?= count($categories) ?></div><div class="stat-foot"><span>Danh mục hiện có</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đang ẩn</span><span class="stat-icon order-icon">×</span></div><div class="stat-value"><?= count(array_filter($products, static fn (array $product): bool => !(bool) ($product['TrangThai'] ?? 1))) ?></div><div class="stat-foot"><span>Không hiển thị trên cửa hàng</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách sản phẩm</h2><p>Dữ liệu lấy trực tiếp từ cơ sở dữ liệu.</p></div><div class="table-tools"><select class="filter-select" aria-label="Lọc danh mục"><option>Tất cả danh mục</option><?php foreach ($categories as $category): ?><option><?= htmlspecialchars($category['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></option><?php endforeach; ?></select><button class="button button-outline button-small" type="button" id="productSearchFocus">⌕ <span class="hide-mobile">Tìm sản phẩm</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>SẢN PHẨM</th><th>DANH MỤC</th><th>GIÁ BÁN</th><th>KÍCH CỠ</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($products as $product): ?>
                                <?php $productStatus = (bool) ($product['TrangThai'] ?? 1) ? 'Đang bán' : 'Ngừng bán'; ?>
                                <tr data-search-row>
                                    <td><div class="product-cell"><img src="../images/<?= rawurlencode((string) ($product['HinhAnh'] ?? 'menu-1.jpg')) ?>" alt="<?= htmlspecialchars($product['TenSP'], ENT_QUOTES, 'UTF-8') ?>"><div><strong><?= htmlspecialchars($product['TenSP'], ENT_QUOTES, 'UTF-8') ?></strong><small>Mã món: <?= (int) $product['MaSP'] ?></small></div></div></td>
                                    <td><?= htmlspecialchars($product['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></td><td class="money-cell"><?= money($product['GiaBan']) ?></td>
                                    <td><?= htmlspecialchars($product['KichCo'] ?? '—', ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><span class="status status-<?= status_class($productStatus) ?>"><i></i><?= $productStatus ?></span></td>
                                    <td><form method="post" action="index.php?page=products" onsubmit="return confirm('Xóa hoặc ẩn sản phẩm này?')"><input type="hidden" name="csrf_token" value="<?= htmlspecialchars($_SESSION['admin_csrf'], ENT_QUOTES, 'UTF-8') ?>"><input type="hidden" name="action" value="delete_product"><input type="hidden" name="product_id" value="<?= (int) $product['MaSP'] ?>"><button class="row-more" type="submit" aria-label="Xóa <?= htmlspecialchars($product['TenSP'], ENT_QUOTES, 'UTF-8') ?>">×</button></form></td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($products === []): ?><tr><td colspan="6" class="muted-cell">Chưa có sản phẩm. Hãy thêm món mới để hiển thị trên cửa hàng.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($products) ?> sản phẩm</span></div>
                </section>

            <?php elseif ($page === 'orders'): ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Đơn hàng</h1><p class="page-subtitle">Theo dõi và cập nhật tình trạng các đơn hàng.</p></div>
                    <button class="button button-outline" type="button" id="exportButton"><span aria-hidden="true">↓</span> Xuất báo cáo</button>
                </section>
                <section class="stats-grid compact-stats three-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng đơn hàng</span><span class="stat-icon revenue-icon">▣</span></div><div class="stat-value"><?= count($orders) ?></div><div class="stat-foot"><span>Đơn trong cơ sở dữ liệu</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Chờ xác nhận</span><span class="stat-icon average-icon">◷</span></div><div class="stat-value"><?= count(array_filter($orders, static fn (array $order): bool => $order['TrangThai'] === 'Chờ xác nhận')) ?></div><div class="stat-foot"><span>Cần xử lý sớm</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đã hoàn tất</span><span class="stat-icon customer-icon">✓</span></div><div class="stat-value"><?= count(array_filter($orders, static fn (array $order): bool => in_array($order['TrangThai'], ['Hoàn tất', 'Hoàn thành'], true))) ?></div><div class="stat-foot"><span>Đơn đã xử lý</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Tất cả đơn hàng</h2><p>Dữ liệu lấy trực tiếp từ cơ sở dữ liệu.</p></div><div class="table-tools"><select class="filter-select" aria-label="Lọc trạng thái"><option>Tất cả trạng thái</option><option>Chờ xác nhận</option><option>Đang giao</option><option>Hoàn tất</option><option>Hoàn thành</option><option>Đã hủy</option></select><button class="button button-outline button-small" type="button" id="orderSearchFocus">⌕ <span class="hide-mobile">Tìm đơn hàng</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>MÃ ĐƠN</th><th>KHÁCH HÀNG</th><th>THỜI GIAN</th><th>SẢN PHẨM</th><th>TỔNG TIỀN</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($orders as $order): ?>
                                <?php $orderStatus = (string) ($order['TrangThai'] ?? 'Chờ xác nhận'); ?>
                                <tr data-search-row data-status="<?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?>">
                                    <td class="order-id">#<?= (int) $order['MaHD'] ?></td>
                                    <td><div class="customer-info"><strong><?= htmlspecialchars($order['TenNguoiNhan'] ?? 'Khách hàng', ENT_QUOTES, 'UTF-8') ?></strong><small><?= htmlspecialchars($order['SdtNguoiNhan'] ?? '', ENT_QUOTES, 'UTF-8') ?></small></div></td>
                                    <td class="muted-cell"><?= htmlspecialchars((string) ($order['NgayTao'] ?? ''), ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><?= (int) $order['SoSanPham'] ?> món</td>
                                    <td class="money-cell"><?= money($order['TongTien'] ?? 0) ?></td>
                                    <td><span class="status status-<?= status_class($orderStatus) ?>"><i></i><?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td>
                                        <?php if (in_array($orderStatus, ['Chờ xác nhận', 'Đang giao'], true)): ?>
                                        <form method="post" action="index.php?page=orders">
                                            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($_SESSION['admin_csrf'], ENT_QUOTES, 'UTF-8') ?>">
                                            <input type="hidden" name="action" value="advance_order">
                                            <input type="hidden" name="order_id" value="<?= (int) $order['MaHD'] ?>">
                                            <input type="hidden" name="current_status" value="<?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?>">
                                            <button class="button button-outline button-small" type="submit"><?= $orderStatus === 'Chờ xác nhận' ? 'Bắt đầu giao' : 'Hoàn tất' ?></button>
                                        </form>
                                        <?php endif; ?>
                                    </td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($orders === []): ?><tr><td colspan="7" class="muted-cell">Chưa có đơn hàng trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($orders) ?> đơn hàng</span></div>
                </section>

            <?php elseif ($page === 'surveys'): ?>
                <section class="welcome-row page-title-row survey-welcome">
                    <div><div class="eyebrow">THẤU HIỂU KHÁCH HÀNG</div><h1>Khảo sát ý kiến</h1><p class="page-subtitle">Lắng nghe khách hàng để cải thiện trải nghiệm tại Coffee Blend.</p></div>
                    <button class="button button-primary" type="button" id="openSurveyDialog"><span aria-hidden="true">＋</span> Tạo khảo sát</button>
                </section>
                <section class="survey-callout">
                    <span class="survey-callout-icon" aria-hidden="true">✦</span>
                    <div><strong>Mỗi ý kiến đều giúp cửa hàng tốt hơn</strong><p>Theo dõi phản hồi, mức độ hài lòng và những góp ý cần ưu tiên xử lý.</p></div>
                    <span class="callout-decoration" aria-hidden="true">☕</span>
                </section>
                <section class="stats-grid compact-stats survey-stats" aria-label="Chỉ số khảo sát">
                    <article class="stat-card"><div class="stat-top"><span>Khảo sát đang mở</span><span class="stat-icon revenue-icon">▧</span></div><div class="stat-value"><?= $dashboard['survey_open'] ?></div><div class="stat-foot"><span>Trong <?= count($surveys) ?> khảo sát trong CSDL</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Tổng lượt mời</span><span class="stat-icon order-icon">➤</span></div><div class="stat-value"><?= $dashboard['survey_invitations'] ?></div><div class="stat-foot"><span>Tổng lượt mời đã ghi nhận</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Phản hồi khảo sát</span><span class="stat-icon customer-icon">▤</span></div><div class="stat-value"><?= $dashboard['survey_responses'] ?></div><div class="stat-foot"><span>Lượt mời có ít nhất một câu trả lời</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đánh giá sản phẩm</span><span class="stat-icon average-icon">★</span></div><div class="stat-value"><?= number_format($dashboard['average_rating'], 1, ',', '.') ?> <small>/ 5</small></div><div class="stat-foot"><span><?= array_sum($dashboard['rating_counts']) ?> lượt đánh giá trong PhanHoi</span></div></article>
                </section>
                <section class="survey-insights">
                    <article class="panel satisfaction-panel">
                        <div class="panel-heading"><div><h2>Đánh giá sản phẩm</h2><p>Phân bố điểm trong bảng PhanHoi</p></div><span class="survey-period"><?= array_sum($dashboard['rating_counts']) ?> lượt</span></div>
                        <div class="satisfaction-content">
                            <div class="rating-score"><strong><?= number_format($dashboard['average_rating'], 1, ',', '.') ?></strong><span class="rating-stars" aria-label="Điểm trung bình <?= number_format($dashboard['average_rating'], 1, ',', '.') ?> trên 5"><?= str_repeat('★', (int) round($dashboard['average_rating'])) ?><?= str_repeat('☆', 5 - (int) round($dashboard['average_rating'])) ?></span><small>trên 5 điểm</small></div>
                            <div class="rating-bars" aria-label="Phân bố số sao">
                                <?php for ($rating = 5; $rating >= 1; $rating--): $ratingCount = $dashboard['rating_counts'][$rating] ?? 0; $ratingPercent = array_sum($dashboard['rating_counts']) > 0 ? (int) round($ratingCount / array_sum($dashboard['rating_counts']) * 100) : 0; ?>
                                    <div class="rating-row"><span><?= $rating ?> sao</span><div class="rating-track"><i style="width: <?= $ratingPercent ?>%"></i></div><strong><?= $ratingCount ?></strong></div>
                                <?php endfor; ?>
                            </div>
                        </div>
                        <?php if (array_sum($dashboard['rating_counts']) === 0): ?><div class="insight-note">Chưa có đánh giá nào trong bảng PhanHoi.</div><?php endif; ?>
                    </article>
                    <article class="panel survey-focus-panel">
                        <div class="panel-heading"><div><h2>Dữ liệu khảo sát</h2><p>Thống kê đọc từ các bảng khảo sát hiện có</p></div></div>
                        <div class="focus-list">
                            <div class="focus-row"><span class="focus-number">01</span><div><strong>Khảo sát</strong><small>Bản ghi trong KhaoSat</small></div><b><?= count($surveys) ?></b></div>
                            <div class="focus-row"><span class="focus-number">02</span><div><strong>Câu hỏi</strong><small>Bản ghi trong CauHoiKhaoSat</small></div><b><?= array_sum(array_map(static fn (array $survey): int => (int) $survey['SoCauHoi'], $surveys)) ?></b></div>
                            <div class="focus-row"><span class="focus-number">03</span><div><strong>Ý kiến sản phẩm</strong><small>Bản ghi trong PhanHoi</small></div><b><?= $dashboard['feedback_count'] ?></b></div>
                        </div>
                        <div class="focus-foot">Chủ đề góp ý chưa được phân loại trong các bảng hiện tại.</div>
                    </article>
                </section>
                <section class="panel table-panel survey-list-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách khảo sát</h2><p>Theo dõi thời gian, lượt mời và tiến độ phản hồi.</p></div><div class="table-tools"><select class="filter-select" id="surveyStatusFilter" aria-label="Lọc trạng thái khảo sát"><option value="">Tất cả trạng thái</option><option value="Đang diễn ra">Đang diễn ra</option><option value="Sắp diễn ra">Sắp diễn ra</option><option value="Đã kết thúc">Đã kết thúc</option><option value="Chưa thiết lập">Chưa thiết lập</option></select><button class="button button-outline button-small" type="button" id="surveySearchFocus">⌕ <span class="hide-mobile">Tìm khảo sát</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table survey-table">
                            <thead><tr><th>KHẢO SÁT</th><th>LOẠI CÂU HỎI</th><th>THỜI GIAN</th><th>LƯỢT MỜI</th><th>PHẢN HỒI</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($surveys as $survey): ?>
                                <?php $responseRate = (int) $survey['SoLuotMoi'] > 0 ? (int) round((int) $survey['SoLuotPhanHoi'] / (int) $survey['SoLuotMoi'] * 100) : 0; $surveyStatus = (string) $survey['TrangThaiTinh']; ?>
                                <tr data-search-row data-status="<?= htmlspecialchars($surveyStatus, ENT_QUOTES, 'UTF-8') ?>">
                                    <td><div class="survey-name-cell"><span class="survey-row-icon" aria-hidden="true">▧</span><div><strong><?= htmlspecialchars($survey['TieuDe'], ENT_QUOTES, 'UTF-8') ?></strong><small>#<?= (int) $survey['MaKhaoSat'] ?> · <?= (int) $survey['SoCauHoi'] ?> câu hỏi</small></div></div></td>
                                    <td><?= (int) $survey['SoCauHoi'] ?> câu hỏi</td>
                                    <td class="survey-dates"><?= !empty($survey['NgayBatDau']) ? date('d/m/Y', strtotime((string) $survey['NgayBatDau'])) : '—' ?><small>đến <?= !empty($survey['NgayKetThuc']) ? date('d/m/Y', strtotime((string) $survey['NgayKetThuc'])) : '—' ?></small></td>
                                    <td><?= (int) $survey['SoLuotMoi'] ?></td>
                                    <td><div class="response-cell"><strong><?= (int) $survey['SoLuotPhanHoi'] ?></strong><span><?= $responseRate ?>%</span><div class="response-track"><i style="width: <?= $responseRate ?>%"></i></div></div></td>
                                    <td><span class="status status-<?= status_class($surveyStatus) ?>"><i></i><?= htmlspecialchars($surveyStatus, ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td></td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($surveys === []): ?><tr><td colspan="7" class="muted-cell">Chưa có khảo sát trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($surveys) ?> khảo sát</span><a class="text-link" href="#recentFeedback">Xem phản hồi mới <span aria-hidden="true">↓</span></a></div>
                </section>
                <section class="feedback-section" id="recentFeedback">
                    <div class="feedback-heading"><div><div class="eyebrow">TIẾNG NÓI KHÁCH HÀNG</div><h2>Đánh giá sản phẩm mới nhất</h2><p class="page-subtitle">Ý kiến và điểm đánh giá được lưu trong bảng PhanHoi.</p></div><button class="button button-outline button-small" type="button" id="feedbackSearchFocus">⌕ Tìm phản hồi</button></div>
                    <div class="feedback-grid">
                        <?php foreach ($feedback as $response): ?>
                            <?php $feedbackRating = max(0, min(5, (int) $response['DanhGia'])); ?>
                            <article class="panel feedback-card" data-search-row>
                                <div class="feedback-card-top"><div class="customer-cell"><span class="avatar avatar-lavender"><?= htmlspecialchars(mb_substr((string) ($response['HoTen'] ?? 'K'), 0, 1, 'UTF-8'), ENT_QUOTES, 'UTF-8') ?></span><div class="customer-info"><strong><?= htmlspecialchars($response['HoTen'] ?? 'Khách hàng', ENT_QUOTES, 'UTF-8') ?></strong><small><?= !empty($response['NgayGui']) ? date('d/m/Y H:i', strtotime((string) $response['NgayGui'])) : '—' ?></small></div></div><span class="feedback-stars" aria-label="<?= $feedbackRating ?> trên 5 sao"><?= str_repeat('★', $feedbackRating) ?><span><?= str_repeat('★', 5 - $feedbackRating) ?></span></span></div>
                                <div class="feedback-question"><?= htmlspecialchars($response['TenSP'] ?? 'Góp ý chung', ENT_QUOTES, 'UTF-8') ?> · Mã phản hồi <?= (int) $response['MaPhanHoi'] ?></div>
                                <p class="feedback-answer">“<?= htmlspecialchars($response['NoiDung'] ?? '', ENT_QUOTES, 'UTF-8') ?>”</p>
                                <div class="feedback-card-foot"><span>Trạng thái: <?= htmlspecialchars($response['TrangThaiXL'] ?? 'Chưa cập nhật', ENT_QUOTES, 'UTF-8') ?></span></div>
                            </article>
                        <?php endforeach; ?>
                        <?php if ($feedback === []): ?><p class="muted-cell">Chưa có ý kiến khách hàng trong bảng PhanHoi.</p><?php endif; ?>
                    </div>
                </section>

            <?php else: ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Khách hàng</h1><p class="page-subtitle">Thông tin và hoạt động mua hàng của khách hàng.</p></div>
                    <button class="button button-outline" type="button" id="customerExportButton"><span aria-hidden="true">↓</span> Xuất danh sách</button>
                </section>
                <section class="stats-grid compact-stats three-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng khách hàng</span><span class="stat-icon customer-icon">♙</span></div><div class="stat-value"><?= $dashboard['customer_count'] ?></div><div class="stat-foot"><span>Bản ghi trong bảng KhachHang</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Khách hàng mới</span><span class="stat-icon revenue-icon">＋</span></div><div class="stat-value"><?= $dashboard['new_customers'] ?></div><div class="stat-foot"><span>Đăng ký trong 30 ngày gần nhất</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Có hạng thành viên</span><span class="stat-icon average-icon">✦</span></div><div class="stat-value"><?= $dashboard['loyal_customer_count'] ?></div><div class="stat-foot"><span>HạngTV có giá trị trong CSDL</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách khách hàng</h2><p>Dữ liệu lấy từ bảng KhachHang và HoaDon.</p></div><button class="button button-outline button-small" type="button" id="customerSearchFocus">⌕ <span class="hide-mobile">Tìm khách hàng</span></button></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>KHÁCH HÀNG</th><th>SỐ ĐIỆN THOẠI</th><th>ĐƠN HÀNG</th><th>TỔNG CHI TIÊU</th><th>HẠNG THÀNH VIÊN</th></tr></thead>
                            <tbody>
                            <?php foreach ($customers as $customer): ?>
                                <?php $customerTier = trim((string) ($customer['HangTV'] ?? '')) !== '' ? (string) $customer['HangTV'] : 'Chưa xếp hạng'; ?>
                                <tr data-search-row><td><div class="customer-cell"><span class="avatar avatar-lavender"><?= htmlspecialchars(mb_substr($customer['HoTen'], 0, 1, 'UTF-8'), ENT_QUOTES, 'UTF-8') ?></span><div class="customer-info"><strong><?= htmlspecialchars($customer['HoTen'], ENT_QUOTES, 'UTF-8') ?></strong><small>Mã khách hàng: <?= (int) $customer['MaKH'] ?></small></div></div></td><td><?= htmlspecialchars($customer['Sdt'], ENT_QUOTES, 'UTF-8') ?></td><td><?= (int) $customer['SoDonHang'] ?></td><td class="money-cell"><?= money($customer['TongChiTieu']) ?></td><td><span class="status status-<?= status_class($customerTier) ?>"><i></i><?= htmlspecialchars($customerTier, ENT_QUOTES, 'UTF-8') ?></span></td></tr>
                            <?php endforeach; ?>
                            <?php if ($customers === []): ?><tr><td colspan="5" class="muted-cell">Chưa có khách hàng trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($customers) ?> khách hàng</span></div>
                </section>
            <?php endif; ?>
            <footer class="page-footer">© 2026 Coffee Blend Admin <span>Số liệu quản trị được tổng hợp từ cơ sở dữ liệu coffee_shop_db</span></footer>
        </div>
    </main>
</div>

<?php if ($page === 'products'): ?>
<dialog class="product-dialog" id="productDialog">
    <form method="post" action="index.php?page=products" enctype="multipart/form-data" class="dialog-form">
        <div class="dialog-heading"><div><div class="eyebrow">SẢN PHẨM MỚI</div><h2>Thêm sản phẩm</h2></div><button class="icon-button dialog-close" type="button" aria-label="Đóng">×</button></div>
        <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($_SESSION['admin_csrf'], ENT_QUOTES, 'UTF-8') ?>">
        <input type="hidden" name="action" value="create_product">
        <label>Tên sản phẩm<input name="name" type="text" maxlength="100" placeholder="Ví dụ: Cà phê sữa đá" required></label>
        <div class="dialog-fields">
            <label>Danh mục<select name="category_id" required><option value="">Chọn danh mục</option><?php foreach ($categories as $category): ?><option value="<?= (int) $category['MaLoaiSP'] ?>"><?= htmlspecialchars($category['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></option><?php endforeach; ?></select></label>
            <label>Giá bán (đ)<input name="price" type="number" min="0" step="0.01" placeholder="0" required></label>
        </div>
        <label>Kích cỡ (không bắt buộc)<input name="size" type="text" maxlength="10" placeholder="M, L hoặc mặc định"></label>
        <label>Ảnh sản phẩm (JPG, PNG, WEBP · tối đa 5 MB)<input name="image" type="file" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" required></label>
        <div class="dialog-actions"><button class="button button-outline dialog-cancel" type="button">Hủy</button><button class="button button-primary" type="submit">Lưu sản phẩm</button></div>
    </form>
</dialog>
<?php endif; ?>
<?php if ($page === 'surveys'): ?>
<dialog class="survey-dialog product-dialog" id="surveyDialog">
    <form method="dialog" class="dialog-form">
        <div class="dialog-heading"><div><div class="eyebrow">KHẢO SÁT MỚI</div><h2>Tạo khảo sát khách hàng</h2></div><button class="icon-button dialog-close" value="cancel" aria-label="Đóng">×</button></div>
        <p class="dialog-note">Tạo khảo sát để thu thập ý kiến và cải thiện trải nghiệm khách hàng.</p>
        <label>Tiêu đề khảo sát<input type="text" maxlength="255" placeholder="Ví dụ: Đánh giá trải nghiệm tháng 10"></label>
        <label>Mô tả<textarea rows="3" placeholder="Chia sẻ mục đích khảo sát với khách hàng"></textarea></label>
        <div class="dialog-fields">
            <label>Ngày bắt đầu<input type="date"></label>
            <label>Ngày kết thúc<input type="date"></label>
        </div>
        <div class="survey-question-preview"><strong>Câu hỏi khảo sát</strong>Phần quản lý câu hỏi sẽ được bổ sung cùng chức năng lưu khảo sát.</div>
        <div class="dialog-actions"><button class="button button-outline" value="cancel">Hủy</button><button class="button button-primary" id="saveSurveyPreview" value="cancel">Tạo khảo sát</button></div>
    </form>
</dialog>
<?php endif; ?>
<div class="toast" id="adminToast" role="status" aria-live="polite"></div>
<div class="sidebar-backdrop" id="sidebarBackdrop"></div>
<script src="admin.js?v=2"></script>
</body>
</html>

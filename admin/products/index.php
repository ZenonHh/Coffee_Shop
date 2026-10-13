<?php
require_once __DIR__ . '/../includes/auth.php';
admin_require_login();

$stmt = $conn->query(
    'SELECT sp.MaSP, sp.TenSP, sp.TrangThai, lo.TenLoaiSP
     FROM sanpham AS sp
     LEFT JOIN loaisanpham AS lo ON lo.MaLoaiSP = sp.MaLoaiSP
     ORDER BY sp.MaSP DESC'
);
$products = $stmt->fetchAll(PDO::FETCH_ASSOC);
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <title>Quản lý sản phẩm</title>
    <style>
        body { margin: 0; font-family: Arial, sans-serif; background: #f4f6f8; }
        .layout { display: flex; min-height: 100vh; }
        .sidebar { width: 220px; background: #1d1d1d; color: white; padding: 18px 12px; }
        .brand { font-size: 22px; font-weight: 700; margin-bottom: 18px; padding: 10px 12px; }
        .nav a { display: block; padding: 12px 14px; color: #e3e3e3; text-decoration: none; border-radius: 8px; margin-bottom: 6px; }
        .nav a:hover, .nav a.active { background: #2d6a4f; }
        .content { flex: 1; padding: 30px; }
        .panel { background: white; border-radius: 14px; padding: 22px; box-shadow: 0 8px 18px rgba(0,0,0,.04); }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 12px 10px; border-bottom: 1px solid #edf0f3; text-align: left; }
    </style>
</head>
<body>
<div class="layout">
    <aside class="sidebar">
        <div class="brand">Coffee Admin</div>
        <nav class="nav">
            <a href="../dashboard/index.php">Dashboard</a>
            <a class="active" href="index.php">Sản phẩm</a>
            <a href="../orders/index.php">Đơn hàng</a>
            <a href="../surveys/index.php">Khảo sát</a>
            <a href="../staff/index.php">Nhân viên</a>
            <a href="../auth/logout.php">Đăng xuất</a>
        </nav>
    </aside>
    <main class="content">
        <div class="panel">
            <h2>Danh sách sản phẩm</h2>
            <table>
                <thead>
                    <tr>
                        <th>Mã</th>
                        <th>Tên sản phẩm</th>
                        <th>Loại</th>
                        <th>Trạng thái</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($products as $product): ?>
                        <tr>
                            <td><?= (int) $product['MaSP'] ?></td>
                            <td><?= htmlspecialchars((string) $product['TenSP'], ENT_QUOTES, 'UTF-8') ?></td>
                            <td><?= htmlspecialchars((string) ($product['TenLoaiSP'] ?? 'Chưa phân loại'), ENT_QUOTES, 'UTF-8') ?></td>
                            <td><?= (int) ($product['TrangThai'] ?? 0) === 1 ? 'Hiển thị' : 'Ẩn' ?></td>
                        </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
    </main>
</div>
</body>
</html>

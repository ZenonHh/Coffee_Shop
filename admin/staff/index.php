<?php
require_once __DIR__ . '/../includes/auth.php';
admin_require_login();

$stmt = $conn->query(
    'SELECT t.MaTaiKhoan, t.HoTen, t.TenDangNhap, v.TenVaiTro, t.TrangThai
     FROM taikhoan AS t
     LEFT JOIN vaitro AS v ON v.MaVaiTro = t.MaVaiTro
     ORDER BY t.MaTaiKhoan DESC'
);
$staffMembers = $stmt->fetchAll(PDO::FETCH_ASSOC);
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <title>Quản lý nhân viên</title>
    <style>
        body { margin: 0; font-family: Arial, sans-serif; background: #f4f6f8; }
        .layout { display: flex; min-height: 100vh; }
        .sidebar { width: 220px; background: #1d1d1d; color: white; padding: 18px 12px; }
        .brand { font-size: 22px; font-weight: 700; margin-bottom: 18px; padding: 10px 12px; }
        .nav a { display: block; padding: 12px 14px; color: #e3e3e3; text-decoration: none; border-radius: 8px; margin-bottom: 6px; }
        .nav a:hover, .nav a.active { background: #2d6a4f; }
        .content { flex: 1; padding: 30px; }
        .panel { background: white; border-radius: 14px; padding: 22px; box-shadow: 0 8px 18px rgba(0,0,0,.04); }
        .header-actions { display: flex; justify-content: space-between; align-items: center; margin-bottom: 18px; }
        .btn { display: inline-block; background: #1d7a5f; color: white; text-decoration: none; padding: 10px 15px; border-radius: 10px; }
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
            <a href="../products/index.php">Sản phẩm</a>
            <a href="../orders/index.php">Đơn hàng</a>
            <a href="../surveys/index.php">Khảo sát</a>
            <a class="active" href="index.php">Nhân viên</a>
            <a href="../auth/logout.php">Đăng xuất</a>
        </nav>
    </aside>
    <main class="content">
        <div class="panel">
            <div class="header-actions">
                <h2>Danh sách nhân viên</h2>
                <a class="btn" href="../auth/register.php">Thêm tài khoản</a>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Mã</th>
                        <th>Họ tên</th>
                        <th>Tên đăng nhập</th>
                        <th>Vai trò</th>
                        <th>Trạng thái</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($staffMembers as $member): ?>
                        <tr>
                            <td><?= (int) $member['MaTaiKhoan'] ?></td>
                            <td><?= htmlspecialchars((string) ($member['HoTen'] ?? ''), ENT_QUOTES, 'UTF-8') ?></td>
                            <td><?= htmlspecialchars((string) ($member['TenDangNhap'] ?? ''), ENT_QUOTES, 'UTF-8') ?></td>
                            <td><?= htmlspecialchars((string) ($member['TenVaiTro'] ?? 'Không xác định'), ENT_QUOTES, 'UTF-8') ?></td>
                            <td><?= (int) ($member['TrangThai'] ?? 0) === 1 ? 'Hoạt động' : 'Khóa' ?></td>
                        </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
    </main>
</div>
</body>
</html>

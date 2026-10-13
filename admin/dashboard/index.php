<?php
require_once __DIR__ . '/../includes/auth.php';
admin_require_login();

$user = admin_user();
$staffCount = 0;
$productCount = 0;
$orderCount = 0;

try {
    $staffCount = (int) $conn->query('SELECT COUNT(*) FROM taikhoan')->fetchColumn();
    $productCount = (int) $conn->query('SELECT COUNT(*) FROM sanpham')->fetchColumn();
    $orderCount = (int) $conn->query('SELECT COUNT(*) FROM hoadon')->fetchColumn();
} catch (Throwable $e) {
    $staffCount = 0;
    $productCount = 0;
    $orderCount = 0;
}
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Dashboard quản trị</title>
    <style>
        body { margin: 0; font-family: Arial, sans-serif; background: #f4f6f8; color: #1b1b1b; }
        .layout { display: flex; min-height: 100vh; }
        .sidebar { width: 220px; background: #1d1d1d; color: white; padding: 18px 12px; }
        .brand { font-size: 22px; font-weight: 700; margin-bottom: 18px; padding: 10px 12px; }
        .nav a { display: block; padding: 12px 14px; color: #e3e3e3; text-decoration: none; border-radius: 8px; margin-bottom: 6px; }
        .nav a:hover, .nav a.active { background: #2d6a4f; }
        .content { flex: 1; padding: 30px; }
        .topbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 26px; }
        .cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 18px; }
        .card { background: white; border-radius: 14px; padding: 22px; box-shadow: 0 8px 18px rgba(0,0,0,.04); }
        .card h3 { margin: 0 0 8px; font-size: 14px; color: #5c6270; }
        .card strong { font-size: 28px; }
        .panel { margin-top: 26px; background: white; border-radius: 14px; padding: 22px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 12px 10px; border-bottom: 1px solid #edf0f3; text-align: left; }
        .logout { color: white; text-decoration: none; display: inline-block; margin-top: 20px; }
    </style>
</head>
<body>
    <div class="layout">
        <aside class="sidebar">
            <div class="brand">Coffee Admin</div>
            <nav class="nav">
                <a class="active" href="index.php">Dashboard</a>
                <a href="../products/index.php">Sản phẩm</a>
                <a href="../orders/index.php">Đơn hàng</a>
                <a href="../surveys/index.php">Khảo sát</a>
                <a href="../staff/index.php">Nhân viên</a>
                <a href="../auth/logout.php">Đăng xuất</a>
            </nav>
        </aside>

        <main class="content">
            <div class="topbar">
                <div>
                    <h2>Chào mừng, <?= htmlspecialchars((string) ($user['full_name'] ?? $user['username'] ?? 'Admin'), ENT_QUOTES, 'UTF-8') ?></h2>
                    <small>Vai trò: <?= htmlspecialchars((string) ($user['role'] ?? 'Quản trị'), ENT_QUOTES, 'UTF-8') ?></small>
                </div>
            </div>

            <section class="cards">
                <div class="card">
                    <h3>Nhân viên</h3>
                    <strong><?= $staffCount ?></strong>
                </div>
                <div class="card">
                    <h3>Sản phẩm</h3>
                    <strong><?= $productCount ?></strong>
                </div>
                <div class="card">
                    <h3>Đơn hàng</h3>
                    <strong><?= $orderCount ?></strong>
                </div>
            </section>

            <div class="panel">
                <h3>Giới thiệu</h3>
                <p>Phần admin đã được chia theo từng chức năng: dashboard, sản phẩm, đơn hàng và nhân viên. Bạn có thể đăng ký tài khoản nhân viên và quản lý quyền truy cập theo từng nhóm.</p>
            </div>
        </main>
    </div>
</body>
</html>

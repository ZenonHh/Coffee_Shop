<?php
require_once __DIR__ . '/../includes/auth.php';

$errors = [];
$successMessage = $_SESSION['flash_success'] ?? '';
unset($_SESSION['flash_success']);

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $username = trim((string) ($_POST['username'] ?? ''));
    $password = (string) ($_POST['password'] ?? '');

    if ($username === '' || $password === '') {
        $errors[] = 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.';
    } else {
        $stmt = $conn->prepare(
            'SELECT t.MaTaiKhoan, t.TenDangNhap, t.MatKhau AS MatKhauHash, t.HoTen, t.TrangThai, v.TenVaiTro
             FROM taikhoan AS t
             INNER JOIN vaitro AS v ON v.MaVaiTro = t.MaVaiTro
             WHERE t.TenDangNhap = :username AND t.TrangThai = 1'
        );
        $stmt->execute(['username' => $username]);
        $staff = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($staff && password_verify($password, (string) $staff['MatKhauHash'])) {
            session_regenerate_id(true);
            $_SESSION['admin_user'] = [
                'id' => (int) $staff['MaTaiKhoan'],
                'username' => (string) $staff['TenDangNhap'],
                'full_name' => (string) $staff['HoTen'],
                'role' => (string) $staff['TenVaiTro'],
            ];
            admin_redirect('dashboard/index.php');
        }

        $errors[] = 'Tên đăng nhập hoặc mật khẩu không đúng.';
    }
}
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng nhập quản trị</title>
    <style>
        body { font-family: Arial, sans-serif; background: #f3f5f7; margin: 0; display: grid; place-items: center; min-height: 100vh; }
        .login-box { background: #fff; width: min(440px, calc(100% - 32px)); border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,.08); padding: 30px; }
        h1 { margin: 0 0 20px; text-align: center; color: #2a2a2a; }
        .alert { margin-bottom: 16px; padding: 12px 14px; border-radius: 8px; font-size: 14px; }
        .alert.error { background: #fff1f1; color: #a42828; border: 1px solid #f3c5c5; }
        .alert.success { background: #eafaf1; color: #1c7a4c; border: 1px solid #bde5c8; }
        label { display: block; font-weight: 600; margin-bottom: 8px; color: #2f2f2f; }
        input { width: 100%; box-sizing: border-box; padding: 12px 14px; border: 1px solid #d4d7db; border-radius: 10px; margin-bottom: 16px; font-size: 14px; }
        button { width: 100%; background: #1d7a5f; border: none; color: white; padding: 12px 14px; border-radius: 10px; font-size: 16px; cursor: pointer; }
        .links { margin-top: 18px; text-align: center; font-size: 14px; }
        a { color: #1d7a5f; text-decoration: none; }
    </style>
</head>
<body>
    <div class="login-box">
        <h1>Quản trị Coffee Shop</h1>

        <?php if ($errors): foreach ($errors as $error): ?>
            <div class="alert error"><?= htmlspecialchars($error, ENT_QUOTES, 'UTF-8') ?></div>
        <?php endforeach; endif; ?>

        <?php if ($successMessage !== ''): ?>
            <div class="alert success"><?= htmlspecialchars($successMessage, ENT_QUOTES, 'UTF-8') ?></div>
        <?php endif; ?>

        <form method="POST" action="login.php">
            <div>
                <label for="username">Tên đăng nhập</label>
                <input id="username" name="username" type="text" placeholder="nhap ten dang nhap" value="" required>
            </div>
            <div>
                <label for="password">Mật khẩu</label>
                <input id="password" name="password" type="password" placeholder="Nhập mật khẩu" required>
            </div>
            <button type="submit">Đăng nhập</button>
        </form>

        <div class="links">
            <a href="register.php">Đăng ký tài khoản nhân viên</a>
        </div>
    </div>
</body>
</html>

<?php
require_once __DIR__ . '/../includes/auth.php';

if (!empty($_SESSION['admin_user'])) {
    admin_redirect('dashboard/index.php');
}

$errors = [];
$successMessage = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $fullName = trim((string) ($_POST['full_name'] ?? ''));
    $phone = trim((string) ($_POST['phone'] ?? ''));
    $roleName = trim((string) ($_POST['role_name'] ?? 'staff'));
    $username = trim((string) ($_POST['username'] ?? ''));
    $password = (string) ($_POST['password'] ?? '');
    $confirmPassword = (string) ($_POST['confirm_password'] ?? '');

    if ($fullName === '' || $username === '' || $phone === '') {
        $errors[] = 'Vui lòng nhập đầy đủ thông tin cá nhân và tài khoản.';
    }
    if (strlen($username) < 4 || strlen($username) > 30) {
        $errors[] = 'Tên đăng nhập phải từ 4 đến 30 ký tự.';
    }
    if (strlen($password) < 6) {
        $errors[] = 'Mật khẩu phải có ít nhất 6 ký tự.';
    }
    if ($password !== $confirmPassword) {
        $errors[] = 'Mật khẩu xác nhận không khớp.';
    }

    if (!$errors) {
        try {
            $roleStmt = $conn->prepare('SELECT MaVaiTro FROM vaitro WHERE LOWER(TenVaiTro) = LOWER(:roleName) LIMIT 1');
            $roleStmt->execute(['roleName' => $roleName]);
            $role = $roleStmt->fetch(PDO::FETCH_ASSOC);

            if (!$role) {
                $insertRole = $conn->prepare(
                    'INSERT INTO vaitro (TenVaiTro, MoTa) VALUES (:roleName, :description)'
                );
                $insertRole->execute([
                    'roleName' => ucfirst(strtolower($roleName)),
                    'description' => 'Tài khoản quản trị mới',
                ]);
                $roleId = (int) $conn->lastInsertId();
            } else {
                $roleId = (int) $role['MaVaiTro'];
            }

            $existingUser = $conn->prepare('SELECT MaTaiKhoan FROM taikhoan WHERE TenDangNhap = :username LIMIT 1');
            $existingUser->execute(['username' => $username]);
            if ($existingUser->fetch()) {
                $errors[] = 'Tên đăng nhập đã tồn tại.';
            } else {
                $conn->beginTransaction();
                $staffStmt = $conn->prepare(
                    'INSERT INTO nhanvien (HoVaDem, Ten, Sdt, ChucVu) VALUES (:hoVaDem, :ten, :sdt, :chucVu)'
                );
                $staffStmt->execute([
                    'hoVaDem' => $fullName,
                    'ten' => '',
                    'sdt' => $phone,
                    'chucVu' => ucfirst(strtolower($roleName)),
                ]);
                $staffId = (int) $conn->lastInsertId();

                $accountStmt = $conn->prepare(
                    'INSERT INTO taikhoan (MaVaiTro, TenDangNhap, MatKhau, HoTen, TrangThai) VALUES (:roleId, :username, :passwordHash, :hoTen, 1)'
                );
                $accountStmt->execute([
                    'roleId' => $roleId,
                    'username' => $username,
                    'passwordHash' => password_hash($password, PASSWORD_DEFAULT),
                    'hoTen' => $fullName,
                ]);
                $conn->commit();

                $_SESSION['flash_success'] = 'Đăng ký tài khoản nhân viên thành công. Bạn có thể đăng nhập ngay.';
                admin_redirect('login.php');
            }
        } catch (Throwable $e) {
            if ($conn->inTransaction()) {
                $conn->rollBack();
            }
            $errors[] = 'Không thể tạo tài khoản: ' . $e->getMessage();
        }
    }
}
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng ký tài khoản nhân viên</title>
    <style>
        body { font-family: Arial, sans-serif; background: #f3f5f7; margin: 0; display: grid; place-items: center; min-height: 100vh; }
        .box { background: #fff; width: min(520px, calc(100% - 32px)); border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,.08); padding: 30px; }
        h1 { margin: 0 0 20px; text-align: center; }
        .alert { margin-bottom: 16px; padding: 12px 14px; border-radius: 8px; font-size: 14px; }
        .alert.error { background: #fff1f1; color: #a42828; border: 1px solid #f3c5c5; }
        .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
        label { display: block; margin-bottom: 8px; font-weight: 600; }
        input, select { width: 100%; box-sizing: border-box; padding: 12px 14px; border: 1px solid #d4d7db; border-radius: 10px; margin-bottom: 16px; font-size: 14px; }
        button { width: 100%; background: #1d7a5f; border: none; color: white; padding: 12px 14px; border-radius: 10px; font-size: 16px; cursor: pointer; }
        .link { margin-top: 16px; text-align: center; }
        a { color: #1d7a5f; text-decoration: none; }
        @media (max-width: 560px) { .grid { grid-template-columns: 1fr; } }
    </style>
</head>
<body>
    <div class="box">
        <h1>Đăng ký nhân viên</h1>

        <?php if ($errors): foreach ($errors as $error): ?>
            <div class="alert error"><?= htmlspecialchars($error, ENT_QUOTES, 'UTF-8') ?></div>
        <?php endforeach; endif; ?>

        <form method="POST" action="register.php">
            <div class="grid">
                <div>
                    <label for="full_name">Họ và tên</label>
                    <input id="full_name" name="full_name" type="text" placeholder="VD: Nguyễn Văn A" required>
                </div>
                <div>
                    <label for="phone">Số điện thoại</label>
                    <input id="phone" name="phone" type="text" placeholder="VD: 0909..." required>
                </div>
            </div>
            <div class="grid">
                <div>
                    <label for="username">Tên đăng nhập</label>
                    <input id="username" name="username" type="text" placeholder="ví dụ: admin01" required>
                </div>
                <div>
                    <label for="role_name">Vai trò</label>
                    <select id="role_name" name="role_name">
                        <option value="staff">Nhân viên</option>
                        <option value="admin">Quản trị</option>
                    </select>
                </div>
            </div>
            <div class="grid">
                <div>
                    <label for="password">Mật khẩu</label>
                    <input id="password" name="password" type="password" placeholder="Ít nhất 6 ký tự" required>
                </div>
                <div>
                    <label for="confirm_password">Xác nhận mật khẩu</label>
                    <input id="confirm_password" name="confirm_password" type="password" placeholder="Nhập lại mật khẩu" required>
                </div>
            </div>
            <button type="submit">Tạo tài khoản</button>
        </form>

        <div class="link">
            <a href="login.php">Quay lại đăng nhập</a>
        </div>
    </div>
</body>
</html>

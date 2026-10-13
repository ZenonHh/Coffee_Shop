<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require_once __DIR__ . '/../../connect.php';

function admin_url(string $path): string
{
    return '/Coffee_Shop/admin/' . ltrim($path, '/');
}

function admin_redirect(string $path): never
{
    header('Location: ' . admin_url($path), true, 303);
    exit;
}

function admin_require_login(): void
{
    if (($_SESSION['admin_authenticated'] ?? false) === true) {
        $accountId = filter_var($_SESSION['admin_account_id'] ?? null, FILTER_VALIDATE_INT);
        if ($accountId && $accountId > 0) {
            global $conn;
            $statement = $conn->prepare(
                "SELECT t.MaTaiKhoan, t.TenDangNhap, t.HoTen, v.TenVaiTro
                 FROM TaiKhoan AS t
                 INNER JOIN VaiTro AS v ON v.MaVaiTro = t.MaVaiTro
                 WHERE t.MaTaiKhoan = :accountId
                   AND t.TrangThai = 1
                   AND LOWER(TRIM(v.TenVaiTro)) IN ('admin', 'quản trị viên')
                 LIMIT 1"
            );
            $statement->execute(['accountId' => $accountId]);
            $account = $statement->fetch(PDO::FETCH_ASSOC);
            if (is_array($account)) {
                $_SESSION['admin_user'] = [
                    'id' => (int) $account['MaTaiKhoan'],
                    'username' => (string) $account['TenDangNhap'],
                    'full_name' => (string) $account['HoTen'],
                    'role' => (string) $account['TenVaiTro'],
                ];
                return;
            }
        }

        unset(
            $_SESSION['admin_user'],
            $_SESSION['admin_authenticated'],
            $_SESSION['admin_account_id'],
            $_SESSION['admin_role_id'],
            $_SESSION['admin_role_name'],
            $_SESSION['admin_username'],
            $_SESSION['admin_fullname']
        );
    }

    if (!empty($_SESSION['admin_user'])) {
        return;
    }

    admin_redirect('auth/login.php');
}

function admin_user(): ?array
{
    return $_SESSION['admin_user'] ?? null;
}

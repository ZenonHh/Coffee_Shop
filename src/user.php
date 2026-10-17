<?php

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require_once __DIR__ . '/../database/connect.php';

class User
{
    private $data;

    public function __construct()
    {
        $this->data = new Database();
    }

    public function dangky($taiKhoan, $hoTen, $email, $sdt, $matKhau)
    {
        // Kiểm tra tài khoản hoặc email đã tồn tại chưa
        $sqlCheck = 'SELECT MaKH FROM khachhang WHERE TaiKhoan = ? OR Email = ?';
        $this->data->select_prepare($sqlCheck, 'ss', $taiKhoan, $email);

        if ($this->data->numRows() > 0) {
            $_SESSION['error'] = 'Tên đăng nhập hoặc email đã tồn tại!';
            header('Location: register.php');
            exit();
        }

        $hash = password_hash($matKhau, PASSWORD_DEFAULT);
        $sqlInsert = 'INSERT INTO khachhang (TaiKhoan, MatKhau, HoTen, Email, Sdt) VALUES (?, ?, ?, ?, ?)';
        $ok = $this->data->command_prepare($sqlInsert, 'sssss', $taiKhoan, $hash, $hoTen, $email, $sdt)->execute();

        if ($ok) {
            $_SESSION['success'] = 'Đăng ký thành công! Vui lòng đăng nhập.';
            header('Location: login.php');
            exit();
        }

        $_SESSION['error'] = 'Đăng ký thất bại, vui lòng thử lại.';
        header('Location: register.php');
        exit();
    }

    public function dangnhap($taiKhoan, $matKhau)
    {
        $sql = 'SELECT MaKH, TaiKhoan, MatKhau, HoTen, DiemTichLuy, HangTV FROM khachhang WHERE TaiKhoan = ?';
        $this->data->select_prepare($sql, 's', $taiKhoan);
        $row = $this->data->fetch();

        if ($row && password_verify($matKhau, $row['MatKhau'])) {
            session_regenerate_id(true);
            $_SESSION['user_id'] = $row['MaKH'];
            $_SESSION['username'] = $row['TaiKhoan'];
            $_SESSION['fullname'] = $row['HoTen'];
            $_SESSION['diem'] = $row['DiemTichLuy'];
            $_SESSION['hang'] = $row['HangTV'];
            $redirectAfterLogin = $_SESSION['customer_redirect_after_login'] ?? null;
            unset($_SESSION['customer_redirect_after_login']);
            header('Location: ' . ($redirectAfterLogin === 'surveys.php' ? 'surveys.php' : 'index.php'));
            exit();
        }

        $_SESSION['error'] = 'Tên đăng nhập hoặc mật khẩu không đúng!';
        header('Location: login.php');
        exit();
    }

    public function getInfoById($maKH)
    {
        $sql = 'SELECT MaKH, TaiKhoan, HoTen, Email, Sdt, DiaChi FROM khachhang WHERE MaKH = ? LIMIT 1';
        $this->data->select_prepare($sql, 'i', $maKH);
        return $this->data->fetch();
    }

    public function updateProfileInfo($maKH, $hoTen, $sdt, $email)
    {
        $sql = 'UPDATE khachhang SET HoTen = ?, Sdt = ?, Email = ? WHERE MaKH = ?';
        try {
            $stmt = $this->data->command_prepare($sql, 'sssi', $hoTen, $sdt, $email, $maKH);
            return $stmt && $stmt->execute();
        } catch (Exception $e) {
            return false;
        }
    }

    public function updateAddress($maKH, $diaChi)
    {
        $sql = 'UPDATE khachhang SET DiaChi = ? WHERE MaKH = ?';
        try {
            $stmt = $this->data->command_prepare($sql, 'si', $diaChi, $maKH);
            return $stmt && $stmt->execute();
        } catch (Exception $e) {
            return false;
        }
    }

    public function updateProfileWithPassword($maKH, $hoTen, $sdt, $email, $matKhauCu = '', $matKhauMoi = '')
    {
        // 1. Cập nhật thông tin cơ bản trước
        $ok = $this->updateProfileInfo($maKH, $hoTen, $sdt, $email);
        if (!$ok) {
            return ['status' => false, 'error' => 'Không thể cập nhật thông tin cá nhân.'];
        }

        // 2. Nếu người dùng không nhập mật khẩu mới -> hoàn thành cập nhật thông tin
        if (empty($matKhauMoi)) {
            return ['status' => true, 'msg' => 'Đã lưu thông tin cá nhân thành công.'];
        }

        // 3. Nếu muốn đổi mật khẩu -> kiểm tra mật khẩu hiện tại
        $sql = 'SELECT MatKhau FROM khachhang WHERE MaKH = ? LIMIT 1';
        $this->data->select_prepare($sql, 'i', $maKH);
        $row = $this->data->fetch();

        if (!$row || !password_verify($matKhauCu, $row['MatKhau'])) {
            return ['status' => false, 'error' => 'Mật khẩu hiện tại không chính xác.'];
        }

        // 4. Mã hóa và cập nhật mật khẩu mới
        $hash = password_hash($matKhauMoi, PASSWORD_DEFAULT);
        $sqlPass = 'UPDATE khachhang SET MatKhau = ? WHERE MaKH = ?';
        try {
            $stmt = $this->data->command_prepare($sqlPass, 'si', $hash, $maKH);
            if ($stmt && $stmt->execute()) {
                return ['status' => true, 'msg' => 'Đã cập nhật thông tin và đổi mật khẩu thành công.'];
            }
        } catch (Exception $e) {
            return ['status' => false, 'error' => 'Lỗi CSDL khi cập nhật mật khẩu.'];
        }

        return ['status' => false, 'error' => 'Không thể cập nhật mật khẩu mới.'];
    }

    public function dangxuat()
    {
        unset($_SESSION['user_id'], $_SESSION['username'], $_SESSION['fullname'], $_SESSION['diem'], $_SESSION['hang']);
        session_regenerate_id(true);
        header('Location: index.php');
        exit();
    }
}

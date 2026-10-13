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
        $sqlCheck = 'SELECT MaKH FROM khachhang WHERE TaiKhoan = ? OR Email = ?';
        $this->data->select_prepare($sqlCheck, 'ss', $taiKhoan, $email);

        if ($this->data->numRows() > 0) {
            $_SESSION['error'] = 'Tên đăng nhập hoặc email đã tồn tại!';
            return false;
        }

        $hash = password_hash($matKhau, PASSWORD_DEFAULT);
        $sqlInsert = 'INSERT INTO khachhang (TaiKhoan, MatKhau, HoTen, Email, Sdt) VALUES (?, ?, ?, ?, ?)';
        
        try {
            $stmt = $this->data->command_prepare($sqlInsert, 'sssss', $taiKhoan, $hash, $hoTen, $email, $sdt);
            if ($stmt && $stmt->execute()) {
                return true;
            }
        } catch (Exception $e) {
            $_SESSION['error'] = 'Lỗi CSDL: ' . $e->getMessage();
        }

        return false;
    }

    public function dangnhap($taiKhoan, $matKhau)
    {
        $sql = 'SELECT MaKH, TaiKhoan, MatKhau, HoTen, Email, Sdt, DiaChi, DiemTichLuy, HangTV FROM khachhang WHERE TaiKhoan = ?';
        $this->data->select_prepare($sql, 's', $taiKhoan);
        $row = $this->data->fetch();

        if ($row && password_verify($matKhau, $row['MatKhau'])) {
            $_SESSION['user_id']   = $row['MaKH'];
            $_SESSION['username']  = $row['TaiKhoan'];
            $_SESSION['fullname']  = $row['HoTen'];
            $_SESSION['email']     = $row['Email'] ?? '';
            $_SESSION['sdt']       = $row['Sdt'] ?? '';
            $_SESSION['dia_chi']   = $row['DiaChi'] ?? '';
            $_SESSION['diem']      = $row['DiemTichLuy'] ?? 0;
            $_SESSION['hang']      = $row['HangTV'] ?? 'Thành viên';
            header('Location: index.php');
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

    public function dangxuat()
    {
        $_SESSION = [];
        session_destroy();
        header('Location: index.php');
        exit();
    }
}
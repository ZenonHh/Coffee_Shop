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
            header('Location: register.php');
            exit();
        }

        $hash = password_hash($matKhau, PASSWORD_DEFAULT);
        // Không cần truyền DiemTichLuy, HangTV — CSDL tự điền giá trị DEFAULT
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
        // Sửa lại đúng bảng khachhang, cùng bộ tên cột với dangky()
        $sql = 'SELECT MaKH, TaiKhoan, MatKhau, HoTen, DiemTichLuy, HangTV FROM khachhang WHERE TaiKhoan = ?';
        $this->data->select_prepare($sql, 's', $taiKhoan);
        $row = $this->data->fetch();

        if ($row && password_verify($matKhau, $row['MatKhau'])) {
            $_SESSION['user_id']   = $row['MaKH'];
            $_SESSION['username']  = $row['TaiKhoan'];
            $_SESSION['fullname']  = $row['HoTen'];
            $_SESSION['diem']      = $row['DiemTichLuy'];   // tiện hiển thị điểm ngay trên header
            $_SESSION['hang']      = $row['HangTV'];
            header('Location: index.php');
            exit();
        }

        $_SESSION['error'] = 'Tên đăng nhập hoặc mật khẩu không đúng!';
        header('Location: login.php');
        exit();
    }

    public function dangxuat()
    {
        $_SESSION = [];
        session_destroy();
        header('Location: index.php');
        exit();
    }
}
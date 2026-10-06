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
        $sqlCheck = 'SELECT ma_kh FROM khach_hang WHERE tai_khoan = ? OR email = ?';
        $this->data->select_prepare($sqlCheck, 'ss', $taiKhoan, $email);

        if ($this->data->numRows() > 0) {
            $_SESSION['error'] = 'Tên đăng nhập hoặc email đã tồn tại!';
            header('Location: register.php');
            exit();
        }

        $hash = password_hash($matKhau, PASSWORD_DEFAULT);
        $sqlInsert = 'INSERT INTO khach_hang (tai_khoan, mat_khau, ho_ten, email, sdt) VALUES (?, ?, ?, ?, ?)';
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
        $sql = 'SELECT ma_kh, tai_khoan, mat_khau, ho_ten FROM khach_hang WHERE tai_khoan = ?';
        $this->data->select_prepare($sql, 's', $taiKhoan);
        $row = $this->data->fetch();

        if ($row && password_verify($matKhau, $row['mat_khau'])) {
            $_SESSION['user_id'] = $row['ma_kh'];
            $_SESSION['username'] = $row['tai_khoan'];
            $_SESSION['fullname'] = $row['ho_ten'];
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

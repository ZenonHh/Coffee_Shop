<?php
// src/cart-count.php
// In ra số món trong giỏ của khách đang đăng nhập (dùng cho số nhỏ trên icon giỏ hàng ở header).
// Khách chưa đăng nhập -> in 0. Không include src/cart.php để tránh nạp trùng products.php.

if (!function_exists('so_luong_gio_hang')) {
    function so_luong_gio_hang()
    {
        if (!isset($_SESSION['user_id']) || !class_exists('Database')) {
            return 0;
        }
        $db = new Database();
        $db->select_prepare(
            'SELECT COALESCE(SUM(ct.SoLuong), 0) AS n
             FROM chitietgiohang ct
             JOIN giohang g ON g.MaGioHang = ct.MaGioHang
             WHERE g.MaKH = ?',
            'i',
            (int)$_SESSION['user_id']
        );
        $r = $db->fetch();
        return (int)($r['n'] ?? 0);
    }
}
echo so_luong_gio_hang();
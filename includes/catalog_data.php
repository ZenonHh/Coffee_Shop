<?php
require_once __DIR__ . '/app_helpers.php';

$categoryStatement = $conn->prepare(
    'SELECT DISTINCT ls.MaLoaiSP, ls.TenLoaiSP, ls.ThuTuHienThi
     FROM loaisanpham AS ls
     INNER JOIN sanpham AS sp ON sp.MaLoaiSP = ls.MaLoaiSP AND sp.TrangThai = 1
     INNER JOIN bienthesanpham AS bt ON bt.MaSP = sp.MaSP AND bt.TrangThai = 1
     WHERE ls.MaLoaiSP <> 9
     ORDER BY ls.ThuTuHienThi, ls.TenLoaiSP'
);
$categoryStatement->execute();
$catalogGroups = [];
foreach ($categoryStatement->fetchAll() as $category) {
    $categoryName = trim((string) $category['TenLoaiSP']);
    if ($categoryName === 'Fruit Boost') {
        $categoryName = 'Thức uống đặc biệt';
    }
    $catalogGroups[(int) $category['MaLoaiSP']] = [
        'name' => $categoryName,
        'products' => [],
    ];
}

$productStatement = $conn->prepare(
    'SELECT sp.MaSP, sp.MaLoaiSP, sp.TenSP, sp.HinhAnh,
            MIN(bt.GiaBan) AS GiaBan,
            SUBSTRING_INDEX(GROUP_CONCAT(bt.KichCo ORDER BY bt.GiaBan, bt.MaBienThe), CHAR(44), 1) AS KichCo
     FROM sanpham AS sp
     INNER JOIN bienthesanpham AS bt ON bt.MaSP = sp.MaSP AND bt.TrangThai = 1
     WHERE sp.TrangThai = 1 AND sp.MaLoaiSP <> 9
     GROUP BY sp.MaSP, sp.MaLoaiSP, sp.TenSP, sp.HinhAnh
     ORDER BY sp.MaLoaiSP, sp.TenSP'
);
$productStatement->execute();
foreach ($productStatement->fetchAll() as $product) {
    $categoryId = (int) $product['MaLoaiSP'];
    if (isset($catalogGroups[$categoryId])) {
        $catalogGroups[$categoryId]['products'][] = $product;
    }
}

<?php
require_once __DIR__ . '/app_helpers.php';

$categoryStatement = $conn->prepare(
    'SELECT DISTINCT ls.MaLoaiSP, ls.TenLoaiSP, ls.ThuTuHienThi
     FROM LoaiSanPham AS ls
     INNER JOIN SanPham AS sp ON sp.MaLoaiSP = ls.MaLoaiSP
     WHERE COALESCE(sp.TrangThai, 1) = 1
     ORDER BY ls.ThuTuHienThi, ls.TenLoaiSP'
);
$categoryStatement->execute();
$catalogGroups = [];
foreach ($categoryStatement->fetchAll() as $category) {
    $catalogGroups[(int) $category['MaLoaiSP']] = [
        'name' => $category['TenLoaiSP'],
        'products' => [],
    ];
}

$productStatement = $conn->prepare(
    'SELECT MaSP, MaLoaiSP, TenSP, KichCo, GiaBan, HinhAnh
     FROM SanPham
     WHERE COALESCE(TrangThai, 1) = 1
     ORDER BY MaLoaiSP, TenSP'
);
$productStatement->execute();

foreach ($productStatement->fetchAll() as $product) {
    $categoryId = (int) $product['MaLoaiSP'];
    if (isset($catalogGroups[$categoryId])) {
        $catalogGroups[$categoryId]['products'][] = $product;
    }
}

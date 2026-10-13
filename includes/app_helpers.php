<?php
declare(strict_types=1);

require_once __DIR__ . '/../connect.php';
require_once __DIR__ . '/session_helpers.php';

function app_escape(mixed $value): string
{
    if (!is_string($value) && !is_int($value) && !is_float($value) && $value !== null) {
        return '';
    }
    return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
}

function app_post_string(array $source, string $key): string
{
    return is_string($source[$key] ?? null) ? trim($source[$key]) : '';
}

function app_money(string|int|float|null $amount): string
{
    return number_format((float) $amount, 0, ',', '.') . 'đ';
}

function app_product_image(?string $fileName): string
{
    if ($fileName === null || $fileName === '' || basename($fileName) !== $fileName) {
        return 'images/menu-1.jpg';
    }
    if (!is_file(__DIR__ . '/../picture/' . $fileName) && !is_file(__DIR__ . '/../images/' . $fileName)) {
        return 'images/menu-1.jpg';
    }
    $directory = is_file(__DIR__ . '/../picture/' . $fileName) ? 'picture' : 'images';
    return $directory . '/' . rawurlencode($fileName);
}

function app_find_product(PDO $conn, int $productId, bool $activeOnly = true): ?array
{
    $statement = $conn->prepare(
        'SELECT sp.MaSP, sp.MaLoaiSP, sp.TenSP, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP,
                MIN(bt.GiaBan) AS GiaBan,
                SUBSTRING_INDEX(GROUP_CONCAT(bt.KichCo ORDER BY bt.GiaBan, bt.MaBienThe), CHAR(44), 1) AS KichCo
         FROM sanpham AS sp
         INNER JOIN loaisanpham AS ls ON ls.MaLoaiSP = sp.MaLoaiSP
         INNER JOIN bienthesanpham AS bt ON bt.MaSP = sp.MaSP AND bt.TrangThai = 1
         WHERE sp.MaSP = :productId AND (:includeInactive = 1 OR sp.TrangThai = 1)
         GROUP BY sp.MaSP, sp.MaLoaiSP, sp.TenSP, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP'
    );
    $statement->execute(['productId' => $productId, 'includeInactive' => $activeOnly ? 0 : 1]);
    $product = $statement->fetch();
    return $product === false ? null : $product;
}

function app_get_product_variants(PDO $conn, int $productId): array
{
    $statement = $conn->prepare(
        'SELECT MaBienThe, MaSP, KichCo, GiaBan
         FROM bienthesanpham
         WHERE MaSP = :productId AND TrangThai = 1
         ORDER BY GiaBan, MaBienThe'
    );
    $statement->execute(['productId' => $productId]);
    return $statement->fetchAll();
}

function app_fetch_products(PDO $conn, array $variantIds): array
{
    $variantIds = array_values(array_unique(array_filter(
        array_map('intval', $variantIds),
        static fn (int $id): bool => $id > 0
    )));
    if ($variantIds === []) {
        return [];
    }
    $placeholders = implode(', ', array_fill(0, count($variantIds), '?'));
    $statement = $conn->prepare(
        "SELECT bt.MaBienThe, bt.MaSP, bt.KichCo, bt.GiaBan, sp.MaLoaiSP, sp.TenSP,
                sp.HinhAnh, ls.TenLoaiSP
         FROM bienthesanpham AS bt
         INNER JOIN sanpham AS sp ON sp.MaSP = bt.MaSP
         INNER JOIN loaisanpham AS ls ON ls.MaLoaiSP = sp.MaLoaiSP
         WHERE bt.TrangThai = 1 AND sp.TrangThai = 1 AND bt.MaBienThe IN ($placeholders)"
    );
    $statement->execute($variantIds);
    $products = [];
    foreach ($statement->fetchAll() as $product) {
        $products[(int) $product['MaBienThe']] = $product;
    }
    return $products;
}

function app_csrf_token(): string
{
    app_start_session();
    if (!isset($_SESSION['csrf_token'])) {
        $_SESSION['csrf_token'] = bin2hex(random_bytes(32));
    }
    return $_SESSION['csrf_token'];
}

function app_verify_csrf(mixed $token): bool
{
    app_start_session();
    return is_string($token)
        && isset($_SESSION['csrf_token'])
        && hash_equals($_SESSION['csrf_token'], $token);
}

function app_redirect(string $location): never
{
    header('Location: ' . $location, true, 303);
    exit;
}

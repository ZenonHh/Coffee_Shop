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

function app_product_image(string|null $fileName): string
{
    if ($fileName === null || $fileName === '' || basename($fileName) !== $fileName) {
        return 'images/menu-1.jpg';
    }

    return 'images/' . rawurlencode($fileName);
}

function app_find_product(PDO $conn, int $productId, bool $activeOnly = true): ?array
{
    $sql = 'SELECT sp.MaSP, sp.MaLoaiSP, sp.TenSP, sp.KichCo, sp.GiaBan, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP
            FROM SanPham AS sp
            INNER JOIN LoaiSanPham AS ls ON ls.MaLoaiSP = sp.MaLoaiSP
            WHERE sp.MaSP = :productId';

    if ($activeOnly) {
        $sql .= ' AND COALESCE(sp.TrangThai, 1) = 1';
    }

    $statement = $conn->prepare($sql);
    $statement->execute(['productId' => $productId]);
    $product = $statement->fetch();

    return $product === false ? null : $product;
}

function app_fetch_products(PDO $conn, array $productIds): array
{
    $productIds = array_values(array_unique(array_filter(
        array_map('intval', $productIds),
        static fn (int $id): bool => $id > 0
    )));

    if ($productIds === []) {
        return [];
    }

    $placeholders = implode(', ', array_fill(0, count($productIds), '?'));
    $statement = $conn->prepare(
        "SELECT sp.MaSP, sp.MaLoaiSP, sp.TenSP, sp.KichCo, sp.GiaBan, sp.TrangThai, sp.HinhAnh, ls.TenLoaiSP
         FROM SanPham AS sp
         INNER JOIN LoaiSanPham AS ls ON ls.MaLoaiSP = sp.MaLoaiSP
         WHERE COALESCE(sp.TrangThai, 1) = 1 AND sp.MaSP IN ($placeholders)"
    );
    $statement->execute($productIds);

    $products = [];
    foreach ($statement->fetchAll() as $product) {
        $products[(int) $product['MaSP']] = $product;
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

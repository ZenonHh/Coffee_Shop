<?php
require_once __DIR__ . '/session_helpers.php';
app_start_session();
require_once __DIR__ . '/../connect.php';

function customer_portal_escape(mixed $value): string
{
    return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
}

function customer_portal_require_login(PDO $connection): array
{
    $customerId = filter_var($_SESSION['user_id'] ?? null, FILTER_VALIDATE_INT);
    if (!$customerId || $customerId < 1) {
        if (basename((string) ($_SERVER['SCRIPT_NAME'] ?? '')) === 'surveys.php') {
            $_SESSION['customer_redirect_after_login'] = 'surveys.php';
        }
        header('Location: login.php');
        exit();
    }

    $columnQuery = $connection->prepare(
        'SELECT COLUMN_NAME
         FROM INFORMATION_SCHEMA.COLUMNS
         WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = LOWER(:tableName)'
    );
    $columnQuery->execute(['tableName' => 'KhachHang']);
    $customerColumns = array_fill_keys(
        array_map('strtolower', $columnQuery->fetchAll(PDO::FETCH_COLUMN)),
        true
    );
    $requiredColumns = ['MaKH', 'TaiKhoan', 'HoTen', 'Sdt'];
    $missingColumns = array_filter(
        $requiredColumns,
        static fn (string $column): bool => !isset($customerColumns[strtolower($column)])
    );
    if ($missingColumns !== []) {
        throw new RuntimeException(
            'KhachHang is missing required columns: ' . implode(', ', $missingColumns)
        );
    }

    $optionalColumns = ['Email', 'NgaySinh', 'SoThich'];
    $selectColumns = $requiredColumns;
    foreach ($optionalColumns as $column) {
        $selectColumns[] = isset($customerColumns[strtolower($column)])
            ? $column
            : 'NULL AS ' . $column;
    }
    $statusCondition = isset($customerColumns['trangthai']) ? ' AND TrangThai = 1' : '';
    $customerQuery = $connection->prepare(
        'SELECT ' . implode(', ', $selectColumns) . '
         FROM KhachHang WHERE MaKH = :customerId' . $statusCondition
    );
    $customerQuery->execute(['customerId' => $customerId]);
    $customer = $customerQuery->fetch();
    if (!is_array($customer)) {
        unset($_SESSION['user_id'], $_SESSION['username'], $_SESSION['fullname'], $_SESSION['diem'], $_SESSION['hang']);
        $_SESSION['error'] = 'Tài khoản không còn hoạt động. Vui lòng liên hệ cửa hàng.';
        header('Location: login.php');
        exit();
    }
    if (!isset($_SESSION['customer_csrf']) || !is_string($_SESSION['customer_csrf'])) {
        $_SESSION['customer_csrf'] = bin2hex(random_bytes(32));
    }
    return $customer;
}

function customer_portal_validate_csrf(): void
{
    $postedToken = $_POST['csrf_token'] ?? null;
    if (!is_string($postedToken) || !hash_equals((string) ($_SESSION['customer_csrf'] ?? ''), $postedToken)) {
        throw new InvalidArgumentException('Phiên làm việc đã hết hạn. Vui lòng gửi lại biểu mẫu.');
    }
}

function customer_portal_redirect(string $page): never
{
    header('Location: ' . $page, true, 303);
    exit();
}

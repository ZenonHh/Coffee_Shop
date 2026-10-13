<?php
require_once __DIR__ . '/includes/customer_portal.php';
$customer = customer_portal_require_login($conn);

$productId = filter_var($_POST['product_id'] ?? null, FILTER_VALIDATE_INT);
$returnToInput = is_string($_POST['return_to'] ?? null) ? $_POST['return_to'] : '';
$returnTo = 'product-single.php?id=' . (is_int($productId) && $productId > 0 ? $productId : 0);
if (
    is_int($productId)
    && $productId > 0
    && $returnToInput === 'product-single.php?id=' . $productId
) {
    $returnTo = $returnToInput;
}

$redirectWithNotice = static function (string $message) use ($productId, $returnTo): never {
    if (is_int($productId) && $productId > 0) {
        $_SESSION['product_review_notice_product_id'] = $productId;
    }
    $_SESSION['product_review_notice'] = $message;
    header('Location: ' . $returnTo . '#product-reviews-' . (is_int($productId) && $productId > 0 ? $productId : ''), true, 303);
    exit;
};

try {
    customer_portal_validate_csrf();
    $rating = filter_var($_POST['rating'] ?? null, FILTER_VALIDATE_INT);
    $message = is_string($_POST['message'] ?? null) ? trim($_POST['message']) : '';
    if (!is_int($productId) || $productId < 1 || !is_int($rating) || $rating < 1 || $rating > 5) {
        throw new InvalidArgumentException('Vui lòng chọn sản phẩm và mức đánh giá từ 1 đến 5 sao.');
    }
    if (mb_strlen($message, 'UTF-8') > 1000) {
        throw new InvalidArgumentException('Nhận xét tối đa 1.000 ký tự.');
    }

    $productQuery = $conn->prepare(
        'SELECT 1 FROM SanPham WHERE MaSP = :productId AND COALESCE(TrangThai, 1) = 1'
    );
    $productQuery->execute(['productId' => $productId]);
    if (!$productQuery->fetchColumn()) {
        throw new InvalidArgumentException('Sản phẩm không tồn tại hoặc không còn kinh doanh.');
    }

    $insert = $conn->prepare(
        "INSERT INTO PhanHoi (MaKH, MaSP, NoiDung, DanhGia, NgayGui, TrangThaiXL)
         VALUES (:customerId, :productId, :message, :rating, NOW(), 'Chờ xử lý')"
    );
    $insert->execute([
        'customerId' => $customer['MaKH'],
        'productId' => $productId,
        'message' => $message,
        'rating' => $rating,
    ]);
    $redirectWithNotice('Cảm ơn bạn! Đánh giá đã được gửi.');
} catch (InvalidArgumentException $exception) {
    $redirectWithNotice($exception->getMessage());
} catch (Throwable $exception) {
    error_log('Product review submission failed: ' . $exception->getMessage());
    $redirectWithNotice('Không thể gửi đánh giá lúc này. Vui lòng thử lại sau.');
}

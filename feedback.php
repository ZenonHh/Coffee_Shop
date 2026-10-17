<?php
$title = 'Gửi ý kiến - Coffee Blend';
$activePage = 'feedback';
require_once __DIR__ . '/includes/customer_portal.php';
$customer = customer_portal_require_login($conn);

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    try {
        customer_portal_validate_csrf();
        $productId = $_POST['product_id'] ?? '';
        $productId = $productId === '' ? null : filter_var($productId, FILTER_VALIDATE_INT);
        $rating = filter_var($_POST['rating'] ?? null, FILTER_VALIDATE_INT);
        $message = is_string($_POST['message'] ?? null) ? trim($_POST['message']) : '';
        if (($productId !== null && (!$productId || $productId < 1))
            || !is_int($rating) || $rating < 1 || $rating > 5
            || $message === '' || mb_strlen($message, 'UTF-8') > 5000) {
            throw new InvalidArgumentException('Chọn điểm đánh giá từ 1–5 sao và nhập ý kiến (tối đa 5.000 ký tự).');
        }
        if ($productId !== null) {
            $productQuery = $conn->prepare('SELECT 1 FROM SanPham WHERE MaSP = :productId AND COALESCE(TrangThai, 1) = 1');
            $productQuery->execute(['productId' => $productId]);
            if (!$productQuery->fetchColumn()) {
                throw new InvalidArgumentException('Sản phẩm đã chọn không tồn tại hoặc không còn kinh doanh.');
            }
        }
        $insert = $conn->prepare(
            "INSERT INTO PhanHoi (MaKH, MaSP, NoiDung, DanhGia, NgayGui, TrangThaiXL)
             VALUES (:customerId, :productId, :message, :rating, NOW(), 'Đã tiếp nhận')"
        );
        $insert->execute([
            'customerId' => $customer['MaKH'],
            'productId' => $productId,
            'message' => $message,
            'rating' => $rating,
        ]);
        $_SESSION['success'] = 'Cảm ơn bạn. Phản hồi đã được gửi đến cửa hàng.';
        customer_portal_redirect('feedback.php');
    } catch (InvalidArgumentException $exception) {
        $_SESSION['error'] = $exception->getMessage();
        customer_portal_redirect('feedback.php');
    } catch (Throwable $exception) {
        error_log('Customer feedback submission failed: ' . $exception->getMessage());
        $_SESSION['error'] = 'Không thể gửi ý kiến lúc này. Vui lòng thử lại sau.';
        customer_portal_redirect('feedback.php');
    }
}

$productQuery = $conn->prepare('SELECT MaSP, TenSP FROM SanPham WHERE COALESCE(TrangThai, 1) = 1 ORDER BY TenSP');
$productQuery->execute();
$products = $productQuery->fetchAll();
$feedbackQuery = $conn->prepare(
    'SELECT ph.NoiDung, ph.DanhGia, ph.NgayGui, ph.TrangThaiXL, sp.TenSP
     FROM PhanHoi AS ph LEFT JOIN SanPham AS sp ON sp.MaSP = ph.MaSP
     WHERE ph.MaKH = :customerId ORDER BY ph.NgayGui DESC, ph.MaPhanHoi DESC'
);
$feedbackQuery->execute(['customerId' => $customer['MaKH']]);
$myFeedback = $feedbackQuery->fetchAll();
include __DIR__ . '/src/header.php';
?>
<section class="ftco-section contact-section">
  <div class="container mt-5">
    <div class="row justify-content-center"><div class="col-md-8">
      <h2 class="mb-2">Gửi phản hồi</h2><p>Chia sẻ trải nghiệm sản phẩm để chúng tôi phục vụ tốt hơn.</p>
      <?php foreach (['success' => 'alert-success', 'error' => 'alert-danger'] as $notice => $class): ?>
        <?php if (isset($_SESSION[$notice])): ?><div class="alert <?= $class ?>"><?= customer_portal_escape($_SESSION[$notice]) ?></div><?php unset($_SESSION[$notice]); endif; ?>
      <?php endforeach; ?>
      <form method="post" action="feedback.php" class="contact-form">
        <input type="hidden" name="csrf_token" value="<?= customer_portal_escape($_SESSION['customer_csrf']) ?>">
        <div class="form-group"><label for="feedbackProduct">Sản phẩm (không bắt buộc)</label><select class="form-control" id="feedbackProduct" name="product_id"><option value="">Góp ý chung về cửa hàng</option><?php foreach ($products as $product): ?><option value="<?= (int) $product['MaSP'] ?>"><?= customer_portal_escape($product['TenSP']) ?></option><?php endforeach; ?></select></div>
        <div class="form-group"><label for="feedbackRating">Mức độ hài lòng</label><select class="form-control" id="feedbackRating" name="rating" required><option value="">Chọn số sao</option><?php for ($stars = 5; $stars >= 1; $stars--): ?><option value="<?= $stars ?>"><?= $stars ?> sao</option><?php endfor; ?></select></div>
        <div class="form-group"><label for="feedbackMessage">Ý kiến của bạn</label><textarea class="form-control" id="feedbackMessage" name="message" rows="5" maxlength="5000" required></textarea></div>
        <button type="submit" class="btn btn-primary py-3 px-5">Gửi phản hồi</button>
      </form>
    </div></div>
    <div class="row mt-5"><div class="col-12"><h3>Phản hồi đã gửi</h3>
      <?php if ($myFeedback === []): ?><p>Bạn chưa gửi phản hồi nào.</p><?php endif; ?>
      <?php foreach ($myFeedback as $entry): ?><article class="p-3 mb-3" style="background:#fff;border-radius:8px"><strong><?= customer_portal_escape($entry['TenSP'] ?? 'Góp ý chung') ?></strong> · <?= (int) $entry['DanhGia'] ?>/5 sao <span class="float-right"><?= customer_portal_escape($entry['TrangThaiXL'] ?? 'Đã tiếp nhận') ?></span><p class="mt-2 mb-1"><?= nl2br(customer_portal_escape($entry['NoiDung'])) ?></p><small><?= !empty($entry['NgayGui']) ? date('d/m/Y H:i', strtotime((string) $entry['NgayGui'])) : '' ?></small></article><?php endforeach; ?>
    </div></div>
  </div>
</section>
<?php include __DIR__ . '/src/footer.php'; ?>

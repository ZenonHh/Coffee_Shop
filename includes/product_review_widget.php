<?php
require_once __DIR__ . '/../connect.php';
require_once __DIR__ . '/session_helpers.php';
app_start_session();

$reviewProductId = (int) ($reviewProductId ?? 0);
if ($reviewProductId < 1) {
    return;
}

if (!isset($reviewSummary) || !is_array($reviewSummary)) {
    require_once __DIR__ . '/product_review_data.php';
    $reviewSummaries = app_product_review_summaries($conn, [$reviewProductId]);
    $reviewSummary = $reviewSummaries[$reviewProductId] ?? ['ReviewCount' => 0, 'AverageRating' => 0];
}

$reviewEntries = [];
if (!empty($reviewShowRecent)) {
    $reviewEntriesQuery = $conn->prepare(
        'SELECT ph.NoiDung, ph.DanhGia, ph.NgayGui, kh.HoTen
         FROM PhanHoi AS ph
         INNER JOIN KhachHang AS kh ON kh.MaKH = ph.MaKH
         WHERE ph.MaSP = :productId AND ph.DanhGia BETWEEN 1 AND 5
         ORDER BY ph.NgayGui DESC, ph.MaPhanHoi DESC
         LIMIT 3'
    );
    $reviewEntriesQuery->execute(['productId' => $reviewProductId]);
    $reviewEntries = $reviewEntriesQuery->fetchAll(PDO::FETCH_ASSOC);
}

if (!isset($_SESSION['customer_csrf']) || !is_string($_SESSION['customer_csrf'])) {
    $_SESSION['customer_csrf'] = bin2hex(random_bytes(32));
}

$reviewReturnTo = 'product-single.php?id=' . $reviewProductId;
$reviewPage = basename((string) ($_SERVER['SCRIPT_NAME'] ?? 'shop.php'));
if ($reviewPage === 'product-single.php') {
    $pageProductId = filter_var($_GET['id'] ?? null, FILTER_VALIDATE_INT);
    if ($pageProductId && $pageProductId > 0) {
        $reviewReturnTo = 'product-single.php?id=' . $pageProductId;
    }
}

$reviewNotice = null;
if (
    (int) ($_SESSION['product_review_notice_product_id'] ?? 0) === $reviewProductId
    && is_string($_SESSION['product_review_notice'] ?? null)
) {
    $reviewNotice = $_SESSION['product_review_notice'];
    unset($_SESSION['product_review_notice'], $_SESSION['product_review_notice_product_id']);
}

$reviewEscape = static fn (mixed $value): string => htmlspecialchars(
    (string) $value,
    ENT_QUOTES | ENT_SUBSTITUTE,
    'UTF-8'
);
?>
<section class="product-review" id="product-reviews-<?= $reviewProductId ?>" aria-label="Đánh giá sản phẩm">
    <div class="product-review__summary">
        <?php $averageRating = (float) $reviewSummary['AverageRating']; ?>
        <span class="product-review__stars" aria-label="<?= number_format($averageRating, 1) ?> trên 5 sao">
            <?= str_repeat('★', (int) round($averageRating)) ?><?= str_repeat('☆', 5 - (int) round($averageRating)) ?>
        </span>
        <strong><?= number_format($averageRating, 1) ?>/5</strong>
        <span>(<?= (int) $reviewSummary['ReviewCount'] ?> đánh giá)</span>
    </div>

    <?php if ($reviewNotice !== null): ?>
        <p class="product-review__notice" role="status"><?= $reviewEscape($reviewNotice) ?></p>
    <?php endif; ?>

    <?php if (isset($_SESSION['user_id'])): ?>
        <form class="product-review__form" method="post" action="product-review.php">
            <input type="hidden" name="csrf_token" value="<?= $reviewEscape($_SESSION['customer_csrf']) ?>">
            <input type="hidden" name="product_id" value="<?= $reviewProductId ?>">
            <input type="hidden" name="return_to" value="<?= $reviewEscape($reviewReturnTo) ?>">
            <fieldset class="product-review__rating">
                <legend>Đánh giá món này</legend>
                <div class="product-review__choices">
                    <?php for ($rating = 1; $rating <= 5; $rating++): ?>
                        <label>
                            <input type="radio" name="rating" value="<?= $rating ?>" required>
                            <span aria-hidden="true">★</span>
                            <span class="sr-only"><?= $rating ?> sao</span>
                        </label>
                    <?php endfor; ?>
                </div>
            </fieldset>
            <label class="sr-only" for="review-message-<?= $reviewProductId ?>">Nhận xét (không bắt buộc)</label>
            <textarea id="review-message-<?= $reviewProductId ?>" name="message" maxlength="1000" rows="2" placeholder="Chia sẻ thêm cảm nhận (không bắt buộc)"></textarea>
            <button class="product-review__submit" type="submit">Gửi đánh giá</button>
        </form>
    <?php else: ?>
        <p class="product-review__login">Vui lòng <a href="login.php">đăng nhập</a> để đánh giá món này.</p>
    <?php endif; ?>

    <?php if ($reviewEntries !== []): ?>
        <details class="product-review__recent">
            <summary>Xem nhận xét gần đây</summary>
            <?php foreach ($reviewEntries as $entry): ?>
                <article class="product-review__entry">
                    <strong><?= $reviewEscape($entry['HoTen']) ?></strong>
                    <span class="product-review__stars"><?= str_repeat('★', (int) $entry['DanhGia']) ?><?= str_repeat('☆', 5 - (int) $entry['DanhGia']) ?></span>
                    <?php if (trim((string) $entry['NoiDung']) !== ''): ?>
                        <p><?= nl2br($reviewEscape($entry['NoiDung'])) ?></p>
                    <?php endif; ?>
                </article>
            <?php endforeach; ?>
        </details>
    <?php endif; ?>
</section>

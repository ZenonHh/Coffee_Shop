<section class="ftco-section" id="product-catalog">
    <div class="container">
        <div class="row justify-content-center mb-5">
            <div class="col-md-8 heading-section text-center">
                <span class="subheading">Coffee Blend</span>
                <h2 class="mb-4">Thực đơn</h2>
                <p>Chọn món yêu thích và kích cỡ phù hợp.</p>
            </div>
        </div>
        <?php if ($catalogGroups === []): ?>
            <p class="text-center">Hiện chưa có sản phẩm đang kinh doanh.</p>
        <?php endif; ?>
        <?php foreach ($catalogGroups as $categoryId => $category): ?>
            <div class="mb-5" id="category-<?= (int) $categoryId ?>">
                <div class="row justify-content-center mb-3">
                    <div class="col-md-8 heading-section text-center">
                        <h3><?= app_escape($category['name']) ?></h3>
                    </div>
                </div>
                <div class="row">
                    <?php
                    $cardProducts = $category['products'];
                    include __DIR__ . '/product_cards.php';
                    ?>
                </div>
            </div>
        <?php endforeach; ?>
    </div>
</section>

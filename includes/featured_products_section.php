<section class="ftco-section">
    <div class="container">
        <div class="row justify-content-center mb-5">
            <div class="col-md-8 heading-section text-center">
                <span class="subheading">Được yêu thích</span>
                <h2 class="mb-4">Món nổi bật</h2>
                <p>Một vài lựa chọn nổi bật trong thực đơn Coffee Blend.</p>
            </div>
        </div>
        <div class="row">
            <?php
            $cardProducts = $homeProducts;
            include __DIR__ . '/product_cards.php';
            ?>
        </div>
    </div>
</section>

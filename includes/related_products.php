<section class="ftco-section">
    <div class="container">
        <div class="row justify-content-center mb-5">
            <div class="col-md-8 heading-section text-center">
                <span class="subheading">Khám phá thêm</span>
                <h2 class="mb-4">Sản phẩm liên quan</h2>
            </div>
        </div>
        <div class="row">
            <?php
            $cardProducts = $relatedProducts;
            include __DIR__ . '/product_cards.php';
            ?>
        </div>
    </div>
</section>

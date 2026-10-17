<?php
$title = 'Thực đơn - Coffee Blend';
$activePage = 'shop';

require_once __DIR__ . '/includes/catalog_data.php';
include __DIR__ . '/src/header.php';
?>

<section class="home-slider owl-carousel">
    <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
        <div class="overlay"></div>
        <div class="container">
            <div class="row slider-text justify-content-center align-items-center">
                <div class="col-md-7 col-sm-12 text-center ftco-animate">
                    <span class="subheading">Coffee Blend</span>
                    <h1 class="mb-3 mt-2">Thực đơn của chúng tôi</h1>
                    <p class="breadcrumbs">
                        <span class="mr-2"><a href="index.php">Trang chủ</a></span>
                        <span>Thực đơn</span>
                    </p>
                    <p>Chọn món yêu thích, xem kích cỡ và giá bán để đặt hàng.</p>
                </div>
            </div>
        </div>
    </div>
</section>

<?php include __DIR__ . '/includes/product_catalog_section.php'; ?>

<?php include __DIR__ . '/src/footer.php'; ?>

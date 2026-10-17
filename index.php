<?php
$title = 'Trang chủ - Coffee Blend';
$activePage = 'home';

require_once __DIR__ . '/includes/catalog_data.php';

$homeProducts = [];
foreach ($catalogGroups as $category) {
    foreach ($category['products'] as $product) {
        $homeProducts[] = $product;
        if (count($homeProducts) === 4) {
            break 2;
        }
    }
}

include __DIR__ . '/src/header.php';
?>

<section class="home-slider owl-carousel">
    <div class="slider-item" style="background-image: url(images/bg_1.jpg);">
        <div class="overlay"></div>
        <div class="container">
            <div class="row slider-text justify-content-center align-items-center" data-scrollax-parent="true">
                <div class="col-md-8 col-sm-12 text-center ftco-animate">
                    <span class="subheading">Coffee Blend</span>
                    <h1 class="mb-4">Cà phê đậm vị, khởi đầu ngày mới</h1>
                    <p class="mb-4 mb-md-5">Khám phá các món cà phê pha phin và espresso trong thực đơn.</p>
                    <p>
                        <a href="shop.php" class="btn btn-primary p-3 px-xl-4 py-xl-3">Đặt món ngay</a>
                        <a href="shop.php" class="btn btn-white btn-outline-white p-3 px-xl-4 py-xl-3">Xem thực đơn</a>
                    </p>
                </div>
            </div>
        </div>
    </div>
    <div class="slider-item" style="background-image: url(images/bg_2.jpg);">
        <div class="overlay"></div>
        <div class="container">
            <div class="row slider-text justify-content-center align-items-center" data-scrollax-parent="true">
                <div class="col-md-8 col-sm-12 text-center ftco-animate">
                    <span class="subheading">Coffee Blend</span>
                    <h1 class="mb-4">Trà thơm, vị trái cây tươi mát</h1>
                    <p class="mb-4 mb-md-5">Thử các món trà trái cây, trà sữa và matcha đang có tại cửa hàng.</p>
                    <p><a href="shop.php" class="btn btn-primary p-3 px-xl-4 py-xl-3">Chọn món yêu thích</a></p>
                </div>
            </div>
        </div>
    </div>
    <div class="slider-item" style="background-image: url(images/bg_3.jpg);">
        <div class="overlay"></div>
        <div class="container">
            <div class="row slider-text justify-content-center align-items-center" data-scrollax-parent="true">
                <div class="col-md-8 col-sm-12 text-center ftco-animate">
                    <span class="subheading">Coffee Blend</span>
                    <h1 class="mb-4">Thức uống ngon, thêm chút bánh</h1>
                    <p class="mb-4 mb-md-5">Chọn món uống và bánh ăn kèm từ danh sách đang kinh doanh.</p>
                    <p><a href="shop.php" class="btn btn-primary p-3 px-xl-4 py-xl-3">Xem thực đơn</a></p>
                </div>
            </div>
        </div>
    </div>
</section>

<section class="ftco-about d-md-flex">
    <div class="one-half img" style="background-image: url(images/about.jpg);"></div>
    <div class="one-half ftco-animate">
        <div class="overlap">
            <div class="heading-section ftco-animate">
                <span class="subheading">Coffee Blend</span>
                <h2 class="mb-4">Một khoảng dừng cho ngày thêm vui</h2>
            </div>
            <div>
                <p>Từ ly cà phê đậm đà đến trà trái cây mát lành, thực đơn Coffee Blend có nhiều lựa chọn để bạn thưởng thức theo cách riêng.</p>
                <p><a href="shop.php" class="btn btn-primary btn-outline-primary px-4 py-3">Khám phá thực đơn</a></p>
            </div>
        </div>
    </div>
</section>

<?php include __DIR__ . '/includes/featured_products_section.php'; ?>

<section class="ftco-section">
    <div class="container">
        <div class="row justify-content-center mb-5">
            <div class="col-md-8 heading-section text-center">
                <span class="subheading">Chọn theo sở thích</span>
                <h2 class="mb-4">Các nhóm món trong thực đơn</h2>
                <p>Tìm nhanh món yêu thích trong danh sách sản phẩm đang kinh doanh.</p>
            </div>
        </div>
        <div class="row justify-content-center">
            <?php foreach ($catalogGroups as $categoryId => $category): ?>
                <div class="col-md-4 col-lg-3 mb-3 text-center">
                    <a class="btn btn-primary btn-outline-primary btn-block" href="shop.php#category-<?= (int) $categoryId ?>">
                        <?= app_escape($category['name']) ?>
                    </a>
                </div>
            <?php endforeach; ?>
        </div>
        <div class="text-center mt-4">
            <a href="shop.php" class="btn btn-primary px-4 py-3">Xem toàn bộ thực đơn</a>
        </div>
    </div>
</section>

<?php include __DIR__ . '/src/footer.php'; ?>

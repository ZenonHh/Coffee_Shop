<?php
$title = 'Bài viết - Katinat Coffee';
$activePage = 'blog';

require_once __DIR__ . '/includes/catalog_data.php';
include __DIR__ . '/src/header.php';

$descriptionTemplates = [
    1 => 'Đậm đà, thơm nồng và phù hợp cho những ai yêu hương vị cà phê nguyên bản.',
    2 => 'Cà phê pha phin mang hương vị truyền thống, cân bằng và đặc trưng của quán.',
    3 => 'Cà phê ủ lạnh êm dịu, thanh mát, thích hợp để thưởng thức trong ngày nóng.',
    4 => 'Matcha thơm dịu, béo nhẹ và mang đến trải nghiệm mới mẻ cho người yêu đồ uống xanh.',
    5 => 'Trà sữa thơm béo, dễ uống, là lựa chọn lý tưởng cho những phút thư giãn.',
    6 => 'Trà trái cây thanh mát, kết hợp vị trà nhẹ cùng hương trái cây tự nhiên.',
    7 => 'Thức uống trái cây tươi mát, nổi bật với màu sắc bắt mắt và vị chua ngọt hài hòa.',
    8 => 'Món bánh thơm ngon, phù hợp dùng kèm cà phê trong những buổi gặp gỡ và thư giãn.',
];
?>

<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-8 col-sm-12 text-center ftco-animate">
          <span class="subheading">Katinat Coffee</span>
          <h1 class="mb-3 mt-5 bread">Khám phá các món của chúng tôi</h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span>Bài viết</span>
          </p>
          <p>Thưởng thức những món đồ uống và bánh ngon được phục vụ mỗi ngày.</p>
        </div>
      </div>
    </div>
  </div>
</section>

<section class="ftco-section">
  <div class="container">
    <div class="row justify-content-center mb-5">
      <div class="col-md-8 heading-section text-center ftco-animate">
        <span class="subheading">Gợi ý hôm nay</span>
        <h2 class="mb-4">Món ngon tại Coffee Blend</h2>
        <p>Khám phá menu và chọn món yêu thích của bạn.</p>
      </div>
    </div>

    <?php if ($catalogGroups === []): ?>
      <p class="text-center">Hiện chưa có món nào đang được phục vụ.</p>
    <?php endif; ?>

    <?php foreach ($catalogGroups as $categoryId => $category): ?>
      <?php if ($category['products'] === []) continue; ?>
      <div class="mb-5">
        <div class="row justify-content-center mb-3">
          <div class="col-md-8 heading-section text-center">
            <h3><?= app_escape($category['name']) ?></h3>
          </div>
        </div>
        <div class="row d-flex">
          <?php foreach ($category['products'] as $product): ?>
            <?php $description = $descriptionTemplates[(int) $categoryId] ?? 'Một lựa chọn thơm ngon dành cho bạn tại Coffee Blend.'; ?>
            <div class="col-md-6 col-lg-4 d-flex ftco-animate">
              <div class="blog-entry align-self-stretch">
                <a href="product-single.php?id=<?= (int) $product['MaSP'] ?>" class="block-20" style="background-image: url('<?= app_escape(app_product_image($product['HinhAnh'] ?? null)) ?>');"></a>
                <div class="text py-4 d-block">
                  <!--<div class="meta">
                    <div><a href="shop.php#category-<?= (int) $categoryId ?>"><?= app_escape($category['name']) ?></a></div>
                  </div>--> 
                  <h3 class="heading mt-2">
                    <a href="product-single.php?id=<?= (int) $product['MaSP'] ?>"><?= app_escape($product['TenSP']) ?></a>
                  </h3>
                  <p><?= app_escape($description) ?></p>
                  <p><a href="product-single.php?id=<?= (int) $product['MaSP'] ?>" class="btn btn-primary btn-outline-primary">Xem món</a></p>
                </div>
              </div>
            </div>
          <?php endforeach; ?>
        </div>
      </div>
    <?php endforeach; ?>
  </div>
</section>

<?php include __DIR__ . '/src/footer.php'; ?>

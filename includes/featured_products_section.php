<?php
require_once __DIR__ . '/catalog_data.php';
$featuredProducts = array_slice(array_merge(...array_map(
    static fn (array $category): array => $category['products'],
    array_values($catalogGroups)
)), 0, 8);
?>
<section class="ftco-section">
  <div class="container">
    <div class="row justify-content-center mb-5 pb-3">
      <div class="col-md-7 heading-section text-center ftco-animate">
        <span class="subheading">Khám phá</span>
        <h2 class="mb-4">Món nổi bật</h2>
      </div>
    </div>
    <div class="row">
      <?php if ($featuredProducts === []): ?>
        <div class="col-12 text-center"><p>Thực đơn hiện chưa có sản phẩm. Vui lòng quay lại sau.</p></div>
      <?php endif; ?>
      <?php foreach ($featuredProducts as $product): ?>
        <div class="col-lg-3 col-md-4 col-sm-6 mb-4">
          <article class="menu-entry catalog-entry">
            <a href="product-single.php?id=<?= (int) $product['MaSP'] ?>" class="catalog-image-link">
              <img class="catalog-image" src="<?= app_escape(app_product_image($product['HinhAnh'] ?? null)) ?>" alt="<?= app_escape($product['TenSP']) ?>">
            </a>
            <div class="text text-center pt-3">
              <h3><a href="product-single.php?id=<?= (int) $product['MaSP'] ?>"><?= app_escape($product['TenSP']) ?></a></h3>
              <p class="price"><span><?= app_money($product['GiaBan']) ?></span></p>
              <form action="cart.php" method="post">
                <input type="hidden" name="csrf_token" value="<?= app_escape(app_csrf_token()) ?>">
                <input type="hidden" name="action" value="add">
                <input type="hidden" name="product_id" value="<?= (int) $product['MaSP'] ?>">
                <button type="submit" class="btn btn-primary btn-outline-primary">Thêm vào giỏ</button>
              </form>
            </div>
          </article>
        </div>
      <?php endforeach; ?>
    </div>
  </div>
</section>

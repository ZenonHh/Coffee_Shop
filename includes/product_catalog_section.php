<?php
require_once __DIR__ . '/catalog_data.php';
?>
<section class="ftco-section product-catalog" id="product-catalog">
  <div class="container">
    <?php if ($catalogGroups === []): ?>
      <div class="row"><div class="col-12 text-center"><p>Thực đơn hiện chưa có sản phẩm. Vui lòng quay lại sau.</p></div></div>
    <?php endif; ?>
    <?php foreach ($catalogGroups as $category): ?>
      <?php if ($category['products'] === []) continue; ?>
      <div class="row justify-content-center mb-4">
        <div class="col-md-8 heading-section text-center ftco-animate">
          <span class="subheading">Thực đơn Katinat</span>
          <h2 class="mb-4"><?= app_escape($category['name']) ?></h2>
        </div>
      </div>
      <div class="row mb-5">
        <?php foreach ($category['products'] as $product): ?>
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
    <?php endforeach; ?>
  </div>
</section>

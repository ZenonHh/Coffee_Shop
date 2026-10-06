<section class="ftco-section">
  <div class="container">
    <div class="row justify-content-center mb-5 pb-3">
      <div class="col-md-7 heading-section ftco-animate text-center">
        <span class="subheading">Khám phá</span>
        <h2 class="mb-4">Sản phẩm liên quan</h2>
      </div>
    </div>
    <div class="row">
      <?php foreach ($relatedProducts as $product): ?>
        <div class="col-lg-3 col-md-4 col-sm-6 mb-4">
          <article class="menu-entry catalog-entry">
            <a href="product-single.php?id=<?= (int) $product['MaSP'] ?>" class="catalog-image-link">
              <img class="catalog-image" src="<?= app_escape(app_product_image($product['HinhAnh'] ?? null)) ?>" alt="<?= app_escape($product['TenSP']) ?>">
            </a>
            <div class="text text-center pt-3">
              <h3><a href="product-single.php?id=<?= (int) $product['MaSP'] ?>"><?= app_escape($product['TenSP']) ?></a></h3>
              <p class="price"><span><?= app_money($product['GiaBan']) ?></span></p>
            </div>
          </article>
        </div>
      <?php endforeach; ?>
    </div>
  </div>
</section>

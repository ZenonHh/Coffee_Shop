<?php foreach ($cardProducts as $product): ?>
    <div class="col-md-6 col-lg-3 ftco-animate">
        <div class="menu-entry">
            <a href="product-single.php?id=<?= (int) $product['MaSP'] ?>" class="img" style="background-image: url('<?= app_escape(app_product_image($product['HinhAnh'] ?? null)) ?>');"></a>
            <div class="text text-center pt-4">
                <h3><a href="product-single.php?id=<?= (int) $product['MaSP'] ?>"><?= app_escape($product['TenSP']) ?></a></h3>
                <?php if (!empty($product['TenLoaiSP'])): ?><p><?= app_escape($product['TenLoaiSP']) ?></p><?php endif; ?>
                <p class="price"><span><?= app_money($product['GiaBan']) ?></span></p>
                <p><a href="product-single.php?id=<?= (int) $product['MaSP'] ?>" class="btn btn-primary btn-outline-primary">Chọn món</a></p>
            </div>
        </div>
    </div>
<?php endforeach; ?>

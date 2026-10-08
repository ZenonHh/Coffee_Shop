<?php
$title = 'Sản phẩm - Katinat Coffee';
$activePage = 'shop';
$extraScriptsFile = 'src/quantity-script.php';
include "database/connect.php";
include "src/header.php";
include "src/products.php";

$maSP = (int)($_GET['id'] ?? 0);

$productModel = new Product();
$sp = $maSP > 0 ? $productModel->getById($maSP) : null;
$bienThe = $sp ? $productModel->getVariants($maSP) : [];

// Không có sản phẩm hoặc chưa có size nào đang bán -> về trang cửa hàng
if (!$sp || empty($bienThe)) {
    header('Location: shop.php');
    exit;
}

// Topping chỉ lấy khi loại sản phẩm này cho phép
$toppingList = [];
if (in_array((int)$sp['MaLoaiSP'], MA_LOAI_CO_TOPPING, true)) {
    $toppingList = (new Topping())->getAll();
}

// Sản phẩm liên quan: cùng loại, bỏ chính nó, lấy tối đa 4
$lienQuan = array_filter(
    $productModel->getByCategory($sp['MaLoaiSP']),
    fn($r) => (int)$r['MaSP'] !== $maSP
);
$lienQuan = array_slice($lienQuan, 0, 4);

$hinh = htmlspecialchars($sp['HinhAnh'] ?? 'menu-1.jpg');
?>

<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-7 col-sm-12 text-center ftco-animate">
          <h1 class="mb-3 mt-5 bread"><?= htmlspecialchars($sp['TenSP']) ?></h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span class="mr-2"><a href="shop.php">Cửa hàng</a></span>
            <span><?= htmlspecialchars($sp['TenLoaiSP']) ?></span>
          </p>
        </div>
      </div>
    </div>
  </div>
</section>

<section class="ftco-section">
  <div class="container">
    <div class="row">
      <div class="col-lg-6 mb-5 ftco-animate">
        <a href="picture/<?= $hinh ?>" class="image-popup"><img src="picture/<?= $hinh ?>" class="img-fluid" alt="<?= htmlspecialchars($sp['TenSP']) ?>"></a>
      </div>

      <div class="col-lg-6 product-details pl-md-5 ftco-animate">
        <h3><?= htmlspecialchars($sp['TenSP']) ?></h3>
        <p class="price"><span id="gia-hien-thi"></span></p>

        <form action="cart.php" method="post" id="form-them-gio">
          <input type="hidden" name="MaSP" value="<?= $maSP ?>">

          <!-- Chọn size -->
          <div class="row mt-4">
            <div class="col-md-6">
              <label class="font-weight-bold">Kích cỡ</label>
              <div class="form-group d-flex">
                <div class="select-wrap">
                  <div class="icon"><span class="ion-ios-arrow-down"></span></div>
                  <select name="MaBienThe" id="chon-size" class="form-control">
                    <?php foreach ($bienThe as $bt): ?>
                      <option value="<?= (int)$bt['MaBienThe'] ?>" data-price="<?= (int)$bt['GiaBan'] ?>">
                        <?= htmlspecialchars($bt['KichCo']) ?> - <?= number_format($bt['GiaBan'], 0, ',', '.') ?>đ
                      </option>
                    <?php endforeach; ?>
                  </select>
                </div>
              </div>
            </div>
          </div>

          <!-- Chọn topping (chỉ hiện nếu loại này có topping) -->
          <?php if (!empty($toppingList)): ?>
            <div class="mb-3">
              <label class="font-weight-bold d-block">Thêm topping</label>
              <?php foreach ($toppingList as $tp): ?>
                <div class="custom-control custom-checkbox">
                  <input type="checkbox" class="custom-control-input chon-topping"
                         name="topping[]" value="<?= (int)$tp['MaTopping'] ?>"
                         id="tp-<?= (int)$tp['MaTopping'] ?>" data-price="<?= (int)$tp['GiaTopping'] ?>">
                  <label class="custom-control-label" for="tp-<?= (int)$tp['MaTopping'] ?>">
                    <?= htmlspecialchars($tp['TenTopping']) ?> (+<?= number_format($tp['GiaTopping'], 0, ',', '.') ?>đ)
                  </label>
                </div>
              <?php endforeach; ?>
            </div>
          <?php endif; ?>

          <!-- Số lượng -->
          <div class="row">
            <div class="input-group col-md-6 d-flex mb-3">
              <span class="input-group-btn mr-2">
                <button type="button" class="quantity-left-minus btn" data-type="minus" data-field="">
                  <i class="icon-minus"></i>
                </button>
              </span>
              <input type="text" id="quantity" name="quantity" class="form-control input-number" value="1" min="1" max="100">
              <span class="input-group-btn ml-2">
                <button type="button" class="quantity-right-plus btn" data-type="plus" data-field="">
                  <i class="icon-plus"></i>
                </button>
              </span>
            </div>
          </div>

          <p><button type="submit" class="btn btn-primary py-3 px-5" style="color:#fff !important;">Thêm vào giỏ</button></p>
        </form>
      </div>
    </div>
  </div>
</section>

<?php if (!empty($lienQuan)): ?>
<section class="ftco-section">
  <div class="container">
    <div class="row justify-content-center mb-5 pb-3">
      <div class="col-md-7 heading-section ftco-animate text-center">
        <span class="subheading">Khám phá</span>
        <h2 class="mb-4">Sản phẩm liên quan</h2>
      </div>
    </div>
    <div class="row">
      <?php foreach ($lienQuan as $r): ?>
        <div class="col-md-3">
          <div class="menu-entry">
            <a href="product-single.php?id=<?= (int)$r['MaSP'] ?>" class="img" style="background-image: url(picture/<?= htmlspecialchars($r['HinhAnh'] ?? 'menu-1.jpg') ?>);"></a>
            <div class="text text-center pt-4">
              <h3><a href="product-single.php?id=<?= (int)$r['MaSP'] ?>"><?= htmlspecialchars($r['TenSP']) ?></a></h3>
              <p class="price"><span>Từ <?= number_format($r['GiaThapNhat'], 0, ',', '.') ?>đ</span></p>
              <p><a href="product-single.php?id=<?= (int)$r['MaSP'] ?>" class="btn btn-primary btn-outline-primary">Chọn món</a></p>
            </div>
          </div>
        </div>
      <?php endforeach; ?>
    </div>
  </div>
</section>
<?php endif; ?>

<script>
// Tính lại giá = (giá size + tổng topping) x số lượng
(function () {
  var size = document.getElementById('chon-size');
  var qty = document.getElementById('quantity');
  var out = document.getElementById('gia-hien-thi');

  function tinhGia() {
    var gia = parseInt(size.options[size.selectedIndex].dataset.price, 10) || 0;
    document.querySelectorAll('.chon-topping:checked').forEach(function (cb) {
      gia += parseInt(cb.dataset.price, 10) || 0;
    });
    var sl = parseInt(qty.value, 10) || 1;
    out.textContent = (gia * sl).toLocaleString('vi-VN') + 'đ';
  }

  size.addEventListener('change', tinhGia);
  qty.addEventListener('input', tinhGia);
  document.querySelectorAll('.chon-topping').forEach(function (cb) {
    cb.addEventListener('change', tinhGia);
  });
  // nút +/- đổi số lượng bằng script riêng nên đợi nó chạy xong rồi tính lại
  document.querySelectorAll('.quantity-left-minus, .quantity-right-plus').forEach(function (b) {
    b.addEventListener('click', function () { setTimeout(tinhGia, 0); });
  });
  tinhGia();
})();
</script>

<?php include "src/footer.php"; ?>
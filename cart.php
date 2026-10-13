<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}
include "database/connect.php";
include_once "src/products.php";
include_once "src/cart.php";
//include_once "src/cart-count.php";

// Giỏ hàng lưu trong CSDL theo tài khoản -> phải đăng nhập mới dùng được
if (!isset($_SESSION['user_id'])) {
    $_SESSION['error'] = 'Vui lòng đăng nhập để sử dụng giỏ hàng.';
    header('Location: login.php');
    exit;
}

$cart = new Cart((int)$_SESSION['user_id']);

// ===== Xử lý form TRƯỚC khi in HTML (để còn redirect được) =====
if ($_SERVER['REQUEST_METHOD'] === 'POST') {

    if (isset($_POST['MaBienThe'])) {
        // Form "Thêm vào giỏ" từ product-single.php
        $ok = $cart->add(
            (int)$_POST['MaBienThe'],
            (array)($_POST['topping'] ?? []),
            (int)($_POST['quantity'] ?? 1)
        );
        $_SESSION['cart_flash'] = $ok
            ? ['type' => 'success', 'text' => 'Đã thêm món vào giỏ hàng.']
            : ['type' => 'danger',  'text' => 'Không thể thêm món này vào giỏ (món không tồn tại hoặc đã ngừng bán).'];
    } else {
        // Form cập nhật / xóa ngay trên trang giỏ hàng
        if (isset($_POST['qty']) && is_array($_POST['qty'])) {
            $cart->update($_POST['qty']);
        }
        if (!empty($_POST['remove'])) {
            $cart->remove((string)$_POST['remove']);
        }
    }

    header('Location: cart.php'); // tránh bấm F5 bị gửi lại form
    exit;
}

$title = 'Giỏ hàng - Katinat Coffee';
$activePage = 'shop';
include "src/header.php";

$lines = $cart->getLines();
$tong  = $cart->summary($lines);

$flash = $_SESSION['cart_flash'] ?? null;
unset($_SESSION['cart_flash']);
?>

<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-7 col-sm-12 text-center ftco-animate">
          <h1 class="mb-3 mt-5 bread">Giỏ hàng</h1>
          <p class="breadcrumbs"><span class="mr-2"><a href="index.php">Trang chủ</a></span> <span>Giỏ hàng</span></p>
        </div>
      </div>
    </div>
  </div>
</section>

<section class="ftco-section ftco-cart">
  <div class="container">

    <?php if ($flash): ?>
      <div class="alert alert-<?= htmlspecialchars($flash['type']) ?>"><?= htmlspecialchars($flash['text']) ?></div>
    <?php endif; ?>

    <?php if (empty($lines)): ?>

      <div class="text-center py-5">
        <h3 class="mb-4">Giỏ hàng của bạn đang trống</h3>
        <a href="shop.php" class="btn btn-primary py-3 px-5">Xem thực đơn</a>
      </div>

    <?php else: ?>

      <form action="cart.php" method="post">
        <!-- Nút mặc định khi bấm Enter trong ô số lượng (phải nằm TRƯỚC các nút xóa, nếu không Enter sẽ xóa dòng đầu tiên) -->
        <button type="submit" name="cap_nhat" value="1" tabindex="-1" aria-hidden="true"
                style="position:absolute;left:-9999px;"></button>

        <div class="row">
          <div class="col-md-12 ftco-animate">
            <div class="cart-list">
              <table class="table">
                <thead class="thead-primary">
                  <tr class="text-center">
                    <th>&nbsp;</th>
                    <th>&nbsp;</th>
                    <th>Sản phẩm</th>
                    <th>Đơn giá</th>
                    <th>Số lượng</th>
                    <th>Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                <?php foreach ($lines as $l): ?>
                  <tr class="text-center">
                    <td class="product-remove">
                      <button type="submit" name="remove" value="<?= htmlspecialchars($l['Key']) ?>"
                              class="btn btn-link p-0" style="color:inherit;" title="Xóa món này">
                        <span class="icon-close"></span>
                      </button>
                    </td>

                    <td class="image-prod">
                      <div class="img" style="background-image:url(picture/<?= htmlspecialchars($l['HinhAnh'] ?? 'menu-1.jpg') ?>);"></div>
                    </td>

                    <td class="product-name">
                      <h3><a href="product-single.php?id=<?= (int)$l['MaSP'] ?>"><?= htmlspecialchars($l['TenSP']) ?></a></h3>
                      <p>
                        Size <?= htmlspecialchars($l['KichCo']) ?>
                        <?php if (!empty($l['Topping'])): ?>
                          <br>Topping:
                          <?= htmlspecialchars(implode(', ', array_column($l['Topping'], 'TenTopping'))) ?>
                        <?php endif; ?>
                      </p>
                    </td>

                    <td class="price"><?= dinh_dang_tien($l['DonGia']) ?></td>

                    <td class="quantity">
                      <div class="input-group mb-3">
                        <input type="number" name="qty[<?= htmlspecialchars($l['Key']) ?>]"
                               class="quantity form-control input-number"
                               value="<?= (int)$l['SoLuong'] ?>" min="1" max="100">
                      </div>
                    </td>

                    <td class="total"><?= dinh_dang_tien($l['ThanhTien']) ?></td>
                  </tr>
                <?php endforeach; ?>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="row mb-4">
          <div class="col-md-12 d-flex justify-content-between">
            <a href="shop.php" class="btn btn-primary btn-outline-primary py-2 px-4">Tiếp tục chọn món</a>
            <button type="submit" name="cap_nhat" value="1" class="btn btn-primary btn-outline-primary py-2 px-4">Cập nhật giỏ hàng</button>
          </div>
        </div>
      </form>

      <div class="row justify-content-end">
        <div class="col col-lg-4 col-md-6 mt-3 cart-wrap ftco-animate">
          <div class="cart-total mb-3">
            <h3>Tổng giỏ hàng</h3>
            <p class="d-flex">
              <span>Tạm tính</span>
              <span><?= dinh_dang_tien($tong['TamTinh']) ?></span>
            </p>
            <p class="d-flex">
              <span>Giao hàng</span>
              <span><?= dinh_dang_tien($tong['PhiGiao']) ?></span>
            </p>
            <p class="d-flex">
              <span>Giảm giá</span>
              <span><?= dinh_dang_tien($tong['GiamGia']) ?></span>
            </p>
            <hr>
            <p class="d-flex total-price">
              <span>Tổng cộng</span>
              <span><?= dinh_dang_tien($tong['Tong']) ?></span>
            </p>
          </div>
          <p class="text-center"><a href="checkout.php" class="btn btn-primary py-3 px-4">Tiến hành thanh toán</a></p>
        </div>
      </div>

    <?php endif; ?>

  </div>
</section>

<?php include "src/footer.php"; ?>
<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}
include "database/connect.php";
include_once "src/products.php";
include_once "src/cart.php";
include_once "src/orders.php";

// Đặt false khi nộp bài để không lộ chi tiết lỗi SQL cho người dùng
define('APP_DEBUG', true);

// ===== 1. Phải đăng nhập mới được thanh toán =====
if (!isset($_SESSION['user_id'])) {
    $_SESSION['error'] = 'Vui lòng đăng nhập để thanh toán.';
    header('Location: login.php');
    exit;
}
$maKH = (int)$_SESSION['user_id'];

$cart       = new Cart($maKH);
$orderModel = new Order();

// ===== 2. Vừa đặt xong -> hiện trang thành công =====
$donThanhCong = null;
if (isset($_GET['thanhcong'])) {
    $donThanhCong = $orderModel->getById((int)$_GET['thanhcong'], $maKH);
}

$lines = $cart->getLines();
$tong  = $cart->summary($lines);

// Giỏ trống mà cũng không phải trang thành công -> về giỏ hàng
if (!$donThanhCong && empty($lines)) {
    header('Location: cart.php');
    exit;
}

// ===== 3. Dữ liệu form (điền sẵn từ thông tin khách) =====
$kh = $orderModel->getThongTinKhach($maKH);
$form = [
    'ho_ten'  => $kh['HoTen'] ?? '',
    'sdt'     => $kh['Sdt'] ?? '',
    'dia_chi' => '',
    'ghi_chu' => '',
    'pttt'    => 'COD',
];
$errors = [];

// ===== 4. Bấm "Đặt hàng" =====
if (!$donThanhCong && $_SERVER['REQUEST_METHOD'] === 'POST' && ($_POST['action'] ?? '') === 'dat_hang') {
    $form['ho_ten']  = trim((string)($_POST['ho_ten'] ?? ''));
    $form['sdt']     = trim((string)($_POST['sdt'] ?? ''));
    $form['dia_chi'] = trim((string)($_POST['dia_chi'] ?? ''));
    $form['ghi_chu'] = trim((string)($_POST['ghi_chu'] ?? ''));
    $form['pttt']    = (string)($_POST['pttt'] ?? '');

    $sdtSo = preg_replace('/\D+/', '', $form['sdt']);

    if ($form['ho_ten'] === '' || mb_strlen($form['ho_ten']) > 100) {
        $errors[] = 'Vui lòng nhập họ tên người nhận (tối đa 100 ký tự).';
    }
    if (!preg_match('/^0\d{9}$/', $sdtSo)) {
        $errors[] = 'Số điện thoại không hợp lệ (gồm 10 số, bắt đầu bằng 0).';
    }
    if ($form['dia_chi'] === '' || mb_strlen($form['dia_chi']) > 255) {
        $errors[] = 'Vui lòng nhập địa chỉ giao hàng (tối đa 255 ký tự).';
    }
    if (mb_strlen($form['ghi_chu']) > 255) {
        $errors[] = 'Ghi chú tối đa 255 ký tự.';
    }
    if (!in_array($form['pttt'], ['COD', 'CK'], true)) {
        $errors[] = 'Vui lòng chọn phương thức thanh toán.';
    }

    if (!$errors) {
        $maHD = $orderModel->create($maKH, [
            'HoTen'        => $form['ho_ten'],
            'Sdt'          => $sdtSo,
            'DiaChi'       => $form['dia_chi'],
            'GhiChu'       => $form['ghi_chu'],
            'PhuongThucTT' => $form['pttt'],
        ], $lines, $tong);

        if ($maHD) {
            $cart->clear();
            header('Location: checkout.php?thanhcong=' . $maHD);
            exit;
        }

        $errors[] = 'Đặt hàng thất bại, vui lòng thử lại.';
        if (APP_DEBUG) {
            $errors[] = 'Chi tiết lỗi: ' . $orderModel->loi;
        }
    }
}

$title = 'Thanh toán - Katinat Coffee';
$activePage = 'shop';
include "src/header.php";
?>

<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-7 col-sm-12 text-center ftco-animate">
          <h1 class="mb-3 mt-5 bread">Thanh toán</h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span class="mr-2"><a href="cart.php">Giỏ hàng</a></span>
            <span>Thanh toán</span>
          </p>
        </div>
      </div>
    </div>
  </div>
</section>

<section class="ftco-section">
  <div class="container">

  <?php if ($donThanhCong): ?>

    <!-- ===== ĐẶT HÀNG THÀNH CÔNG ===== -->
    <div class="text-center py-5 ftco-animate">
      <h2 class="mb-3">Đặt hàng thành công!</h2>
      <p>Mã đơn hàng của bạn: <strong>#<?= (int)$donThanhCong['MaHD'] ?></strong></p>
      <p>Người nhận: <?= htmlspecialchars($donThanhCong['HoTenNguoiNhan']) ?> - <?= htmlspecialchars($donThanhCong['SdtNguoiNhan']) ?></p>
      <p>Giao đến: <?= htmlspecialchars($donThanhCong['DiaChiGiao']) ?></p>
      <p>Tổng thanh toán: <strong><?= dinh_dang_tien($donThanhCong['TongTien']) ?></strong></p>
      <?php if ($donThanhCong['PhuongThuc'] === 'CK'): ?>
        <p>Quán sẽ liên hệ qua số điện thoại trên để hướng dẫn chuyển khoản cho đơn này.</p>
      <?php else: ?>
        <p>Bạn sẽ thanh toán bằng tiền mặt khi nhận hàng.</p>
      <?php endif; ?>
      <a href="shop.php" class="btn btn-primary py-3 px-5 mt-3">Tiếp tục chọn món</a>
    </div>

  <?php else: ?>

    <!-- ===== FORM THANH TOÁN ===== -->
    <form action="checkout.php" method="post">
      <input type="hidden" name="action" value="dat_hang">

      <div class="row">

        <!-- Cột trái: thông tin giao hàng -->
        <div class="col-xl-7 ftco-animate">
          <div class="billing-form ftco-bg-dark p-3 p-md-5">
            <h3 class="mb-4 billing-heading">Thông tin giao hàng</h3>

            <?php if ($errors): ?>
              <div class="alert alert-danger">
                <ul class="mb-0">
                  <?php foreach ($errors as $e): ?>
                    <li><?= htmlspecialchars($e) ?></li>
                  <?php endforeach; ?>
                </ul>
              </div>
            <?php endif; ?>

            <div class="row align-items-end">
              <div class="col-md-6">
                <div class="form-group">
                  <label for="ho_ten">Họ và tên người nhận</label>
                  <input type="text" id="ho_ten" name="ho_ten" class="form-control" maxlength="100" required
                         value="<?= htmlspecialchars($form['ho_ten']) ?>">
                </div>
              </div>
              <div class="col-md-6">
                <div class="form-group">
                  <label for="sdt">Số điện thoại</label>
                  <input type="text" id="sdt" name="sdt" class="form-control" maxlength="15" required
                         value="<?= htmlspecialchars($form['sdt']) ?>">
                </div>
              </div>
              <div class="w-100"></div>
              <div class="col-md-12">
                <div class="form-group">
                  <label for="dia_chi">Địa chỉ giao hàng</label>
                  <input type="text" id="dia_chi" name="dia_chi" class="form-control" maxlength="255" required
                         placeholder="Số nhà, tên đường, phường/xã, quận/huyện"
                         value="<?= htmlspecialchars($form['dia_chi']) ?>">
                </div>
              </div>
              <div class="w-100"></div>
              <div class="col-md-12">
                <div class="form-group">
                  <label for="ghi_chu">Ghi chú (không bắt buộc)</label>
                  <input type="text" id="ghi_chu" name="ghi_chu" class="form-control" maxlength="255"
                         placeholder="Ví dụ: ít đường, giao giờ hành chính..."
                         value="<?= htmlspecialchars($form['ghi_chu']) ?>">
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Cột phải: đơn hàng + thanh toán -->
        <div class="col-xl-5 ftco-animate">

          <div class="cart-detail cart-total ftco-bg-dark p-3 p-md-4 mb-4">
            <h3 class="billing-heading mb-4">Đơn hàng của bạn</h3>
            <?php foreach ($lines as $l): ?>
              <p class="d-flex">
                <span>
                  <?= (int)$l['SoLuong'] ?> × <?= htmlspecialchars($l['TenSP']) ?> (<?= htmlspecialchars($l['KichCo']) ?>)
                  <?php if (!empty($l['Topping'])): ?>
                    <br><small>+ <?= htmlspecialchars(implode(', ', array_column($l['Topping'], 'TenTopping'))) ?></small>
                  <?php endif; ?>
                </span>
                <span><?= dinh_dang_tien($l['ThanhTien']) ?></span>
              </p>
            <?php endforeach; ?>
            <hr>
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

          <div class="cart-detail ftco-bg-dark p-3 p-md-4">
            <h3 class="billing-heading mb-4">Phương thức thanh toán</h3>
            <div class="form-group">
              <div class="col-md-12">
                <div class="radio">
                  <label><input type="radio" name="pttt" value="COD" class="mr-2" <?= $form['pttt'] === 'COD' ? 'checked' : '' ?>> Thanh toán khi nhận hàng (COD)</label>
                </div>
              </div>
            </div>
            <div class="form-group">
              <div class="col-md-12">
                <div class="radio">
                  <label><input type="radio" name="pttt" value="CK" class="mr-2" <?= $form['pttt'] === 'CK' ? 'checked' : '' ?>> Chuyển khoản ngân hàng</label>
                </div>
              </div>
            </div>
            <div class="form-group">
              <div class="col-md-12">
                <div class="checkbox">
                  <label><input type="checkbox" class="mr-2" required> Tôi đồng ý với điều khoản và điều kiện của cửa hàng</label>
                </div>
              </div>
            </div>
            <p><button type="submit" class="btn btn-primary py-3 px-4" style="color:#fff !important;">Đặt hàng</button></p>
          </div>

        </div>
      </div>
    </form>

  <?php endif; ?>

  </div>
</section>

<?php include "src/footer.php"; ?>
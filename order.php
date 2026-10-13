<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}
include "database/connect.php";
include_once "src/products.php";
include_once "src/cart.php";     // dùng hàm dinh_dang_tien()
include_once "src/orders.php";

// Phải đăng nhập mới xem được đơn hàng
if (!isset($_SESSION['user_id'])) {
    $_SESSION['error'] = 'Vui lòng đăng nhập để xem đơn hàng.';
    header('Location: login.php');
    exit;
}
$maKH = (int)$_SESSION['user_id'];

$orderModel = new Order();

// Nhãn + màu cho TrangThai của hóa đơn (sửa lại cho khớp quy ước của bạn)
$nhanTrangThai = [
    '0' => ['Chờ xác nhận', 'warning'],
    '1' => ['Đang pha chế',  'info'],
    '2' => ['Đang giao',     'primary'],
    '3' => ['Hoàn thành',    'success'],
    '4' => ['Đã hủy',        'danger'],
];
$nhanPTTT = ['COD' => 'Thanh toán khi nhận hàng', 'CK' => 'Chuyển khoản'];

$donXem = null;
$chiTiet = [];
if (isset($_GET['id'])) {
    // getById đã kiểm tra đơn phải thuộc về khách này
    $donXem = $orderModel->getById((int)$_GET['id'], $maKH);
    if (!$donXem) {
        header('Location: order.php');
        exit;
    }
    $chiTiet = $orderModel->getChiTiet((int)$donXem['MaHD']);
} else {
    $danhSach = $orderModel->getByKhach($maKH);
}

$title = 'Đơn hàng của tôi - Katinat Coffee';
$activePage = 'shop';
include "src/header.php";
?>

<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-7 col-sm-12 text-center ftco-animate">
          <h1 class="mb-3 mt-5 bread">Đơn hàng của tôi</h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span>Đơn hàng</span>
          </p>
        </div>
      </div>
    </div>
  </div>
</section>

<section class="ftco-section">
  <div class="container">

  <?php if ($donXem): ?>
    <?php
      $tt = $nhanTrangThai[(string)$donXem['TrangThai']] ?? ['Không rõ', 'secondary'];
    ?>
    <!-- ===== CHI TIẾT 1 ĐƠN ===== -->
    <div class="mb-4">
      <a href="order.php" class="btn btn-primary btn-outline-primary py-2 px-4">&larr; Tất cả đơn hàng</a>
    </div>

    <div class="row">
      <div class="col-lg-5 mb-4">
        <h3 class="mb-3">Đơn #<?= (int)$donXem['MaHD'] ?></h3>
        <p>Trạng thái: <span class="badge badge-<?= $tt[1] ?> p-2"><?= $tt[0] ?></span></p>
        <p>Ngày đặt: <?= date('d/m/Y H:i', strtotime($donXem['NgayTao'])) ?></p>
        <p>Người nhận: <?= htmlspecialchars($donXem['HoTenNguoiNhan']) ?> - <?= htmlspecialchars($donXem['SdtNguoiNhan']) ?></p>
        <p>Giao đến: <?= htmlspecialchars($donXem['DiaChiGiao']) ?></p>
        <?php if (!empty($donXem['GhiChu'])): ?>
          <p>Ghi chú: <?= htmlspecialchars($donXem['GhiChu']) ?></p>
        <?php endif; ?>
        <p>Thanh toán: <?= htmlspecialchars($nhanPTTT[$donXem['PhuongThuc']] ?? (string)$donXem['PhuongThuc']) ?></p>
      </div>

      <div class="col-lg-7">
        <div class="cart-list">
          <table class="table">
            <thead class="thead-primary">
              <tr class="text-center">
                <th>Sản phẩm</th>
                <th>Đơn giá</th>
                <th>SL</th>
                <th>Thành tiền</th>
              </tr>
            </thead>
            <tbody>
            <?php foreach ($chiTiet as $ct): ?>
              <tr class="text-center">
                
                <td class="text-left">
                  <?= htmlspecialchars($ct['TenSP']) ?>
                  <?php if (!empty($ct['KichCo'])): ?>(<?= htmlspecialchars($ct['KichCo']) ?>)<?php endif; ?>
                  <?php if (!empty($ct['Topping'])): ?>
                    <br><small>+ <?= htmlspecialchars(implode(', ', $ct['Topping'])) ?></small>
                  <?php endif; ?>
                </td>
                <td><?= dinh_dang_tien($ct['GiaBan'] + $ct['TienTopping']) ?></td>
                <td><?= (int)$ct['SoLuong'] ?></td>
                <td><?= dinh_dang_tien($ct['ThanhTien']) ?></td>
              </tr>
            <?php endforeach; ?>
            </tbody>
          </table>
        </div>

        <div class="cart-total mb-3">
          <p class="d-flex"><span>Tạm tính</span><span><?= dinh_dang_tien($donXem['TamTinh']) ?></span></p>
          <p class="d-flex"><span>Giao hàng</span><span><?= dinh_dang_tien($donXem['PhiGiao']) ?></span></p>
          <p class="d-flex"><span>Giảm giá</span><span><?= dinh_dang_tien($donXem['TienGiamGia']) ?></span></p>
          <hr>
          <p class="d-flex total-price"><span>Tổng cộng</span><span><?= dinh_dang_tien($donXem['TongTien']) ?></span></p>
        </div>
      </div>
    </div>

  <?php elseif (empty($danhSach)): ?>

    <!-- ===== CHƯA CÓ ĐƠN ===== -->
    <div class="text-center py-5">
      <h3 class="mb-4">Bạn chưa có đơn hàng nào</h3>
      <a href="shop.php" class="btn btn-primary py-3 px-5">Xem thực đơn</a>
    </div>

  <?php else: ?>

    <!-- ===== DANH SÁCH ĐƠN ===== -->
    <div class="cart-list">
      <table class="table">
        <thead class="thead-primary">
          <tr class="text-center">
            <th>Mã đơn</th>
            <th>Ngày đặt</th>
            <th>Thanh toán</th>
            <th>Tổng tiền</th>
            <th>Trạng thái</th>
            <th>&nbsp;</th>
          </tr>
        </thead>
        <tbody>
        <?php foreach ($danhSach as $d): ?>
          <?php $tt = $nhanTrangThai[(string)$d['TrangThai']] ?? ['Không rõ', 'secondary']; ?>
          <tr class="text-center">
            <td>#<?= (int)$d['MaHD'] ?></td>
            <td><?= date('d/m/Y H:i', strtotime($d['NgayTao'])) ?></td>
            <td><?= htmlspecialchars($d['PhuongThuc'] ?? '-') ?></td>
            <td><?= dinh_dang_tien($d['TongTien']) ?></td>
            <td><span class="badge badge-<?= $tt[1] ?> p-2"><?= $tt[0] ?></span></td>
            <td><a href="order.php?id=<?= (int)$d['MaHD'] ?>" class="btn btn-primary btn-outline-primary py-1 px-3">Xem</a></td>
          </tr>
        <?php endforeach; ?>
        </tbody>
      </table>
    </div>

  <?php endif; ?>

  </div>
</section>

<?php include "src/footer.php"; ?>
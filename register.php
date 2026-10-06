<?php
$title = 'Đăng ký - Katinat Coffee';
$activePage = 'register';
include "database/connect.php";
require_once "src/user.php";

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

if (isset($_POST['dang_ky'])) {
    if ($_POST['mat_khau'] !== $_POST['xac_nhan_mat_khau']) {
        $_SESSION['error'] = 'Mật khẩu xác nhận không khớp!';
        header('Location: register.php');
        exit();
    }
    $user = new User();
    $user->dangky($_POST['tai_khoan'], $_POST['ho_ten'], $_POST['email'] ?? '', $_POST['sdt'], $_POST['mat_khau']);
}

include "src/header.php";
?>
<!--
<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-7 col-sm-12 text-center ftco-animate">
          <h1 class="mb-3 mt-5 bread">Đăng ký</h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span>Đăng ký</span>
          </p>
        </div>
      </div>
    </div>
  </div>
</section>
-->
<section class="ftco-section contact-section">
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6 ftco-animate">
        <h2 class="mb-4 text-center">Tạo tài khoản mới</h2>

        <form action="register.php" method="POST" class="contact-form">
          <div class="form-group">
            <input type="text" name="ho_ten" class="form-control" placeholder="Họ và tên" required>
          </div>
          <div class="form-group">
            <input type="text" name="tai_khoan" class="form-control" placeholder="Tên đăng nhập" required>
          </div>
          <div class="form-group">
            <input type="text" name="sdt" class="form-control" placeholder="Số điện thoại" required>
          </div>
          <div class="form-group">
            <input type="password" name="mat_khau" class="form-control" placeholder="Mật khẩu" required>
          </div>
          <div class="form-group">
            <input type="password" name="xac_nhan_mat_khau" class="form-control" placeholder="Xác nhận mật khẩu" required>
          </div>
          <div class="form-group text-center">
            <input type="submit" name="dang_ky" value="Đăng ký" class="btn btn-primary py-3 px-5">
          </div>
          <p class="text-center mt-3">
            Đã có tài khoản?
            <a href="login.php">Đăng nhập</a>
          </p>
        </form>
      </div>
    </div>
  </div>
</section>

<?php include "src/footer.php"; ?>

<?php
$title = 'Đăng nhập - Katinat Coffee';
$activePage = 'login';
include "database/connect.php";
require_once "src/user.php";   

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

if (isset($_POST['dang_nhap'])) {
    $user = new User();
    $user->dangnhap($_POST['tai_khoan'], $_POST['mat_khau']);
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
          <h1 class="mb-3 mt-5 bread">Đăng nhập</h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span>Đăng nhập</span>
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
        <h2 class="mb-4 text-center">Đăng nhập tài khoản</h2>

        <?php if (isset($_SESSION['error'])): ?>
  <div class="alert alert-danger"><?= htmlspecialchars($_SESSION['error']); unset($_SESSION['error']); ?></div>
<?php endif; ?>

        <form action="login.php" method="POST" class="contact-form">
          <div class="form-group">
            <input type="text" name="tai_khoan" class="form-control" placeholder="Tên đăng nhập" required>
          </div>
          <div class="form-group">
            <input type="password" name="mat_khau" class="form-control" placeholder="Mật khẩu" required>
          </div>
          <div class="form-group text-center">
            <input type="submit" name="dang_nhap" value="Đăng nhập" class="btn btn-primary py-3 px-5">
          </div>
          <p class="text-center mt-3">
            Chưa có tài khoản?
            <a href="register.php">Đăng ký ngay</a>
          </p>
        </form>
      </div>
    </div>
  </div>
</section>

<?php include "src/footer.php"; ?>

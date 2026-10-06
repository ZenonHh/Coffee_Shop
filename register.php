<?php
$title = 'Đăng ký - Katinat Coffee';
$activePage = 'register';
include "database/connect.php";
require_once "src/user.php";

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

$errorMessage = '';
$successMessage = '';

// Lấy thông báo lỗi từ Session nếu có
if (isset($_SESSION['error'])) {
    $errorMessage = $_SESSION['error'];
    unset($_SESSION['error']);
}

if (isset($_POST['dang_ky'])) {
    if ($_POST['mat_khau'] !== $_POST['xac_nhan_mat_khau']) {
        $errorMessage = 'Mật khẩu xác nhận không khớp!';
    } else {
        $user = new User();
        
        // Bắt lỗi thử thực thi để tránh crash trang nếu hàm trong user.php chưa chuẩn return
        try {
            $result = $user->dangky(
                $_POST['tai_khoan'], 
                $_POST['ho_ten'], 
                $_POST['email'] ?? '', 
                $_POST['sdt'], 
                $_POST['mat_khau']
            );
            
            // Nếu hàm dangky trả về true hoặc không bị lỗi ngoại lệ
            if ($result === true || $result === null) { 
                $successMessage = 'Đăng ký tài khoản thành công! Đang chuyển hướng...';
            } else {
                $errorMessage = 'Đăng ký thất bại! Tên đăng nhập hoặc số điện thoại đã tồn tại.';
            }
        } catch (Exception $e) {
            $errorMessage = 'Lỗi hệ thống: ' . $e->getMessage();
        }
    }
}

include "src/header.php";
?>

<section class="ftco-section contact-section">
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6 ftco-animate">
        <h2 class="mb-4 text-center">Tạo tài khoản mới</h2>

        <!-- Hiển thị thông báo LỖI -->
        <?php if (!empty($errorMessage)): ?>
            <div class="alert alert-danger text-center">
                <?= htmlspecialchars($errorMessage) ?>
            </div>
        <?php endif; ?>

        <!-- Hiển thị thông báo THÀNH CÔNG và tự động chuyển trang -->
        <?php if (!empty($successMessage)): ?>
            <div class="alert alert-success text-center">
                <?= htmlspecialchars($successMessage) ?>
            </div>
            <script>
                setTimeout(function() {
                    window.location.href = 'login.php';
                }, 2000); // Chờ 2 giây rồi chuyển hướng sang trang login
            </script>
        <?php endif; ?>

        <form action="register.php" method="POST" class="contact-form">
          <div class="form-group">
            <input type="text" name="ho_ten" class="form-control" placeholder="Họ và tên" value="<?= isset($_POST['ho_ten']) ? htmlspecialchars($_POST['ho_ten']) : '' ?>" required>
          </div>
          <div class="form-group">
            <input type="text" name="tai_khoan" class="form-control" placeholder="Tên đăng nhập" value="<?= isset($_POST['tai_khoan']) ? htmlspecialchars($_POST['tai_khoan']) : '' ?>" required>
          </div>
          <div class="form-group">
            <input type="text" name="sdt" class="form-control" placeholder="Số điện thoại" value="<?= isset($_POST['sdt']) ? htmlspecialchars($_POST['sdt']) : '' ?>" required>
          </div>
          <div class="form-group">
            <input type="email" name="email" class="form-control" placeholder="Email" value="<?= isset($_POST['email']) ? htmlspecialchars($_POST['email']) : '' ?>" required>
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
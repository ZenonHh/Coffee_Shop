<?php
$title = 'Hồ sơ cá nhân - Katinat Coffee';
$activePage = 'profile';

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require_once "src/user.php";
$userModel = new User();

function h($v) { return htmlspecialchars((string)$v, ENT_QUOTES, 'UTF-8'); }

// 1. Kiểm tra trạng thái đăng nhập
$userId = $_SESSION['user_id'] ?? null;
if (!$userId) {
    $_SESSION['error'] = 'Vui lòng đăng nhập để xem thông tin cá nhân!';
    header('Location: login.php');
    exit();
}

// 2. Lấy thông tin tài khoản hiện tại từ CSDL
$currentUser = $userModel->getInfoById((int)$userId);
if (!$currentUser) {
    $_SESSION['error'] = 'Không tìm thấy thông tin tài khoản!';
    header('Location: login.php');
    exit();
}

// 3. Khởi tạo CSRF token
if (empty($_SESSION['csrf'])) {
    $_SESSION['csrf'] = bin2hex(random_bytes(16));
}

$errors      = [];
$formError   = '';
$openSection = '';

$form = [
    'ho_ten'  => $currentUser['HoTen'] ?? '',
    'sdt'     => $currentUser['Sdt'] ?? '',
    'email'   => $currentUser['Email'] ?? '',
    'dia_chi' => $currentUser['DiaChi'] ?? '',
];

// 4. Xử lý cập nhật dữ liệu POST
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $section = $_POST['section'] ?? '';

    if (!hash_equals($_SESSION['csrf'], $_POST['csrf'] ?? '')) {
        $formError = 'Phiên làm việc đã hết hạn, vui lòng tải lại trang và thử lại.';
    } elseif ($section === 'info') {
        $openSection = 'info';
        $form['ho_ten'] = trim($_POST['ho_ten'] ?? '');
        $form['sdt']    = preg_replace('/[\s.\-]/', '', trim($_POST['sdt'] ?? ''));
        $form['email']  = trim($_POST['email'] ?? '');

        $oldPass     = $_POST['old_password'] ?? '';
        $newPass     = $_POST['new_password'] ?? '';
        $confirmPass = $_POST['confirm_password'] ?? '';

        if (strpos($form['sdt'], '+84') === 0) {
            $form['sdt'] = '0' . substr($form['sdt'], 3);
        }

        // Kiểm tra thông tin cá nhân
        if ($form['ho_ten'] === '') {
            $errors['ho_ten'] = 'Vui lòng nhập họ và tên.';
        } elseif (mb_strlen($form['ho_ten'], 'UTF-8') > 100) {
            $errors['ho_ten'] = 'Họ và tên tối đa 100 ký tự.';
        }

        if ($form['sdt'] === '') {
            $errors['sdt'] = 'Vui lòng nhập số điện thoại.';
        } elseif (!preg_match('/^0(3|5|7|8|9)[0-9]{8}$/', $form['sdt'])) {
            $errors['sdt'] = 'Số điện thoại phải gồm 10 số, bắt đầu bằng 03, 05, 07, 08 hoặc 09.';
        }

        if ($form['email'] !== '' && !filter_var($form['email'], FILTER_VALIDATE_EMAIL)) {
            $errors['email'] = 'Email chưa đúng định dạng.';
        }

        // Kiểm tra mật khẩu (chỉ xác thực khi người dùng nhập ít nhất một ô)
        if ($oldPass !== '' || $newPass !== '' || $confirmPass !== '') {
            if ($oldPass === '') {
                $errors['old_password'] = 'Vui lòng nhập mật khẩu hiện tại.';
            }
            if ($newPass === '') {
                $errors['new_password'] = 'Vui lòng nhập mật khẩu mới.';
            } elseif (strlen($newPass) < 6) {
                $errors['new_password'] = 'Mật khẩu mới phải có tối thiểu 6 ký tự.';
            }
            if ($newPass !== $confirmPass) {
                $errors['confirm_password'] = 'Xác nhận mật khẩu mới không khớp.';
            }
        }

        if (!$errors && $formError === '') {
            $result = $userModel->updateProfileWithPassword(
                (int)$userId, 
                $form['ho_ten'], 
                $form['sdt'], 
                $form['email'], 
                $oldPass, 
                $newPass
            );

            if ($result['status']) {
                $_SESSION['fullname'] = $form['ho_ten'];
                $_SESSION['sdt']      = $form['sdt'];
                $_SESSION['email']    = $form['email'];
                $_SESSION['success']  = $result['msg'];
                header('Location: profile.php');
                exit();
            } else {
                $formError = $result['error'];
            }
        }
    } elseif ($section === 'address') {
        $openSection = 'address';
        $form['dia_chi'] = trim($_POST['dia_chi'] ?? '');

        if (mb_strlen($form['dia_chi'], 'UTF-8') > 255) {
            $errors['dia_chi'] = 'Địa chỉ tối đa 255 ký tự.';
        } else {
            $ok = $userModel->updateAddress((int)$userId, $form['dia_chi']);
            if ($ok) {
                $_SESSION['dia_chi'] = $form['dia_chi'];
                $_SESSION['success'] = ($form['dia_chi'] === '') ? 'Đã xóa địa chỉ nhận hàng.' : 'Đã cập nhật địa chỉ nhận hàng thành công.';
                header('Location: profile.php');
                exit();
            } else {
                $formError = 'Không thể lưu địa chỉ vào CSDL, vui lòng thử lại.';
            }
        }
    }
}

// Flash messages
$successMessage = '';
if (isset($_SESSION['success'])) { $successMessage = $_SESSION['success']; unset($_SESSION['success']); }
if (isset($_SESSION['error']))   { $formError = $formError ?: $_SESSION['error']; unset($_SESSION['error']); }

$taiKhoanHienThi = $currentUser['TaiKhoan'];
$hoTenHienThi    = $currentUser['HoTen'] ?: $taiKhoanHienThi;
$sdtHienThi      = $currentUser['Sdt'] ?? '';
$emailHienThi    = $currentUser['Email'] ?? '';
$diaChiHienThi   = $currentUser['DiaChi'] ?? '';
$initial         = mb_strtoupper(mb_substr($hoTenHienThi, 0, 1, 'UTF-8'), 'UTF-8');

include "src/header.php";
?>

<link rel="stylesheet" href="css/profile.css">

<section class="ftco-section contact-section">
  <div class="container">

    <div class="row justify-content-center mb-4">
      <div class="col-md-7 text-center heading-section">
        <h2 class="mb-3">HỒ SƠ CÁ NHÂN</h2>
        <p class="text-muted">Quản lý và cập nhật thông tin tài khoản thành viên Katinat</p>
      </div>
    </div>

    <!-- Thông báo lỗi hoặc thành công -->
    <div class="row justify-content-center">
      <div class="col-md-12">
        <?php if ($formError !== ''): ?>
          <div class="alert alert-danger text-center" role="alert"><?= h($formError) ?></div>
        <?php endif; ?>
        <?php if ($successMessage !== ''): ?>
          <div class="alert alert-success text-center" role="status"><?= h($successMessage) ?></div>
        <?php endif; ?>
      </div>
    </div>

    <div class="row justify-content-center">

      <!-- CỘT TRÁI: AVATAR & MENU -->
      <div class="col-lg-4 col-md-5 mb-4">
        <div class="pf-card p-4 text-center">
          <div class="pf-avatar"><?= h($initial) ?></div>
          <h4 class="text-white mt-3 mb-1"><?= h($hoTenHienThi) ?></h4>
          <span class="badge badge-warning mb-3 px-3 py-1" style="background-color: #c49b63; color: #000; font-size: 13px;">Thành viên Katinat</span>

          <div class="pf-menu list-group list-group-flush text-left bg-transparent mt-3 border-top border-secondary pt-3">
            <a href="profile.php" class="list-group-item active">
              <span class="icon-user mr-2"></span> Thông tin cá nhân
            </a>
            <a href="logout.php" class="list-group-item text-danger">
              <span class="icon-power-off mr-2"></span> Đăng xuất
            </a>
          </div>
        </div>
      </div>

      <!-- CỘT PHẢI: CHI TIẾT VÀ SỬA THÔNG TIN -->
      <div class="col-lg-8 col-md-7">

        <!-- MỤC 1: THÔNG TIN CÁ NHÂN & ĐỔI MẬT KHẨU -->
        <div class="pf-card p-4 mb-4" data-section="info">
          <div class="pf-head">
            <h5><span class="icon-id-card mr-2" style="color:#c49b63;"></span> Thông tin tài khoản</h5>
            <button type="button" class="pf-link" data-edit="info" <?= $openSection === 'info' ? 'hidden' : '' ?>>Chỉnh sửa</button>
          </div>

          <!-- Chế độ xem -->
          <div data-view="info" <?= $openSection === 'info' ? 'hidden' : '' ?>>
            <div class="pf-row">
              <div class="pf-label">Tên đăng nhập</div>
              <div class="pf-value" style="color:#c49b63; font-weight:600;"><?= h($taiKhoanHienThi) ?></div>
            </div>
            <div class="pf-row">
              <div class="pf-label">Họ và tên</div>
              <div class="pf-value"><?= h($hoTenHienThi) ?></div>
            </div>
            <div class="pf-row">
              <div class="pf-label">Số điện thoại</div>
              <div class="pf-value"><?= !empty($sdtHienThi) ? h($sdtHienThi) : '<span class="pf-empty">Chưa cập nhật</span>' ?></div>
            </div>
            <div class="pf-row">
              <div class="pf-label">Email</div>
              <div class="pf-value"><?= !empty($emailHienThi) ? h($emailHienThi) : '<span class="pf-empty">Chưa cập nhật</span>' ?></div>
            </div>
          </div>

          <!-- Chế độ sửa -->
          <form method="POST" action="profile.php" data-form="info" novalidate <?= $openSection === 'info' ? '' : 'hidden' ?>>
            <input type="hidden" name="csrf" value="<?= h($_SESSION['csrf']) ?>">
            <input type="hidden" name="section" value="info">

            <div class="form-group mt-3">
              <label for="f-username">Tên đăng nhập</label>
              <input id="f-username" type="text" class="form-control" value="<?= h($taiKhoanHienThi) ?>" readonly style="background:rgba(255,255,255,.05) !important; color:#888 !important; cursor:not-allowed;">
              <div class="pf-hint">Tên đăng nhập cố định không thể thay đổi.</div>
            </div>

            <div class="form-group">
              <label for="f-hoten">Họ và tên <span class="text-danger">*</span></label>
              <input id="f-hoten" type="text" name="ho_ten" maxlength="100" required autocomplete="name"
                     class="form-control <?= isset($errors['ho_ten']) ? 'pf-invalid' : '' ?>" value="<?= h($form['ho_ten']) ?>">
              <?php if (isset($errors['ho_ten'])): ?><div class="pf-error"><?= h($errors['ho_ten']) ?></div><?php endif; ?>
            </div>

            <div class="form-group">
              <label for="f-sdt">Số điện thoại <span class="text-danger">*</span></label>
              <input id="f-sdt" type="tel" name="sdt" inputmode="numeric" maxlength="12" required autocomplete="tel"
                     class="form-control <?= isset($errors['sdt']) ? 'pf-invalid' : '' ?>" value="<?= h($form['sdt']) ?>">
              <?php if (isset($errors['sdt'])): ?><div class="pf-error"><?= h($errors['sdt']) ?></div><?php endif; ?>
            </div>

            <div class="form-group">
              <label for="f-email">Email</label>
              <input id="f-email" type="email" name="email" maxlength="100" autocomplete="email"
                     class="form-control <?= isset($errors['email']) ? 'pf-invalid' : '' ?>" value="<?= h($form['email']) ?>">
              <?php if (isset($errors['email'])): ?><div class="pf-error"><?= h($errors['email']) ?></div><?php endif; ?>
            </div>

            <!-- CỤM ĐỔI MẬT KHẨU TÍCH HỢP -->
            <div class="pt-3 mt-4 border-top border-secondary">
              <h6 class="text-warning mb-2" style="font-size: 14.5px; font-weight: 600;">
                <span class="icon-lock mr-1"></span> Đổi mật khẩu
              </h6>
              <p class="pf-hint mb-3">Bỏ trống các ô bên dưới nếu không muốn đổi mật khẩu.</p>

              <div class="form-group">
                <label for="f-old-pass">Mật khẩu hiện tại</label>
                <input id="f-old-pass" type="password" name="old_password" autocomplete="current-password"
                       class="form-control <?= isset($errors['old_password']) ? 'pf-invalid' : '' ?>" placeholder="Nhập mật khẩu hiện tại nếu muốn đổi">
                <?php if (isset($errors['old_password'])): ?><div class="pf-error"><?= h($errors['old_password']) ?></div><?php endif; ?>
              </div>

              <div class="form-row">
                <div class="form-group col-md-6">
                  <label for="f-new-pass">Mật khẩu mới</label>
                  <input id="f-new-pass" type="password" name="new_password" autocomplete="new-password"
                         class="form-control <?= isset($errors['new_password']) ? 'pf-invalid' : '' ?>" placeholder="Tối thiểu 6 ký tự">
                  <?php if (isset($errors['new_password'])): ?><div class="pf-error"><?= h($errors['new_password']) ?></div><?php endif; ?>
                </div>

                <div class="form-group col-md-6">
                  <label for="f-confirm-pass">Xác nhận mật khẩu mới</label>
                  <input id="f-confirm-pass" type="password" name="confirm_password" autocomplete="new-password"
                         class="form-control <?= isset($errors['confirm_password']) ? 'pf-invalid' : '' ?>" placeholder="Nhập lại mật khẩu mới">
                  <?php if (isset($errors['confirm_password'])): ?><div class="pf-error"><?= h($errors['confirm_password']) ?></div><?php endif; ?>
                </div>
              </div>
            </div>

            <div class="mt-4">
              <button type="submit" class="pf-btn" disabled>Lưu thay đổi</button>
              <button type="button" class="pf-btn pf-btn-ghost" data-cancel="info">Hủy</button>
            </div>
          </form>
        </div>

        <!-- MỤC 2: ĐỊA CHỈ NHẬN HÀNG -->
        <div class="pf-card p-4" data-section="address">
          <div class="pf-head">
            <h5><span class="icon-map-marker mr-2" style="color:#c49b63;"></span> Địa chỉ nhận hàng</h5>
            <button type="button" class="pf-link" data-edit="address" <?= $openSection === 'address' ? 'hidden' : '' ?>>
              <?= !empty($diaChiHienThi) ? 'Chỉnh sửa' : 'Thêm địa chỉ' ?>
            </button>
          </div>

          <!-- Chế độ xem -->
          <div data-view="address" <?= $openSection === 'address' ? 'hidden' : '' ?>>
            <?php if (!empty($diaChiHienThi)): ?>
              <div class="pf-address-box">
                <span class="icon-location-arrow mr-2" style="color: #c49b63;"></span>
                <?= nl2br(h($diaChiHienThi)) ?>
              </div>
            <?php else: ?>
              <div class="pf-address-box text-center py-4">
                <span class="pf-empty">Bạn chưa có địa chỉ nhận hàng. Thêm địa chỉ để đặt hàng thuận tiện hơn.</span>
              </div>
            <?php endif; ?>
          </div>

          <!-- Chế độ sửa -->
          <form method="POST" action="profile.php" data-form="address" novalidate <?= $openSection === 'address' ? '' : 'hidden' ?>>
            <input type="hidden" name="csrf" value="<?= h($_SESSION['csrf']) ?>">
            <input type="hidden" name="section" value="address">

            <div class="form-group mt-3">
              <label for="f-diachi">Địa chỉ nhận hàng</label>
              <textarea id="f-diachi" name="dia_chi" rows="3" maxlength="255" autocomplete="street-address"
                        class="form-control <?= isset($errors['dia_chi']) ? 'pf-invalid' : '' ?>"
                        placeholder="Số nhà, tên đường, phường/xã, quận/huyện, tỉnh/thành phố..."><?= h($form['dia_chi']) ?></textarea>
              <div class="pf-hint"><span data-count="dia_chi">0</span>/255 ký tự. Địa chỉ này dùng để giao hàng tận nơi.</div>
              <?php if (isset($errors['dia_chi'])): ?><div class="pf-error"><?= h($errors['dia_chi']) ?></div><?php endif; ?>
            </div>

            <div class="mt-4">
              <button type="submit" class="pf-btn" disabled>Lưu thay đổi</button>
              <button type="button" class="pf-btn pf-btn-ghost" data-cancel="address">Hủy</button>
            </div>
          </form>
        </div>

      </div>
    </div>
  </div>
</section>

<script>
(function () {
  ['info', 'address'].forEach(function (name) {
    var form   = document.querySelector('[data-form="' + name + '"]');
    var view   = document.querySelector('[data-view="' + name + '"]');
    var editBt = document.querySelector('[data-edit="' + name + '"]');
    var cancel = document.querySelector('[data-cancel="' + name + '"]');
    if (!form) return;

    var saveBt = form.querySelector('button[type="submit"]');
    var fields = form.querySelectorAll('input[name]:not([type="hidden"]), textarea[name]');
    var initial = {};
    fields.forEach(function (f) { initial[f.name] = f.defaultValue; });

    function dirty() {
      for (var i = 0; i < fields.length; i++) {
        if (fields[i].value.trim() !== initial[fields[i].name].trim()) return true;
      }
      return false;
    }
    function refresh() { saveBt.disabled = !dirty(); }

    function open() {
      form.hidden = false; view.hidden = true; editBt.hidden = true;
      var first = form.querySelector('input:not([readonly]):not([type="hidden"]), textarea');
      if (first) first.focus();
    }
    function close() {
      fields.forEach(function (f) { f.value = initial[f.name]; });
      form.querySelectorAll('.pf-error').forEach(function (e) { e.remove(); });
      form.querySelectorAll('.pf-invalid').forEach(function (e) { e.classList.remove('pf-invalid'); });
      form.hidden = true; view.hidden = false; editBt.hidden = false;
      refresh(); updateCount();
      editBt.focus();
    }

    var counter  = form.querySelector('[data-count]');
    var textarea = form.querySelector('textarea');
    function updateCount() { if (counter && textarea) counter.textContent = textarea.value.length; }

    editBt.addEventListener('click', open);
    cancel.addEventListener('click', close);
    fields.forEach(function (f) { f.addEventListener('input', function () { refresh(); updateCount(); }); });
    form.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
    form.addEventListener('submit', function (e) { if (!dirty()) e.preventDefault(); });

    refresh(); updateCount();
  });
})();
</script>

<?php include "src/footer.php"; ?>
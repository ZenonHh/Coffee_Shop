<?php
$title = 'Thông tin tài khoản - Coffee Blend';
$activePage = 'profile';
require_once __DIR__ . '/includes/customer_portal.php';
$customer = customer_portal_require_login($conn);

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    try {
        customer_portal_validate_csrf();
        $name = is_string($_POST['full_name'] ?? null) ? trim($_POST['full_name']) : '';
        $phone = is_string($_POST['phone'] ?? null) ? trim($_POST['phone']) : '';
        $email = is_string($_POST['email'] ?? null) ? trim($_POST['email']) : '';
        $birthDate = is_string($_POST['birth_date'] ?? null) ? trim($_POST['birth_date']) : '';
        $interests = is_string($_POST['interests'] ?? null) ? trim($_POST['interests']) : '';
        $birth = $birthDate === '' ? null : DateTimeImmutable::createFromFormat('!Y-m-d', $birthDate);
        if ($name === '' || mb_strlen($name, 'UTF-8') > 100 || !preg_match('/^[0-9+\s().-]{7,20}$/', $phone)) {
            throw new InvalidArgumentException('Vui lòng nhập họ tên và số điện thoại hợp lệ.');
        }
        if ($email !== '' && (filter_var($email, FILTER_VALIDATE_EMAIL) === false || mb_strlen($email, 'UTF-8') > 100)) {
            throw new InvalidArgumentException('Địa chỉ email không hợp lệ.');
        }
        if ($birthDate !== '' && (!$birth || $birth->format('Y-m-d') !== $birthDate || $birth > new DateTimeImmutable('today'))) {
            throw new InvalidArgumentException('Ngày sinh không hợp lệ.');
        }
        if (mb_strlen($interests, 'UTF-8') > 500) {
            throw new InvalidArgumentException('Sở thích tối đa 500 ký tự.');
        }
        if ($email !== '') {
            $emailQuery = $conn->prepare('SELECT 1 FROM KhachHang WHERE Email = :email AND MaKH <> :customerId LIMIT 1');
            $emailQuery->execute(['email' => $email, 'customerId' => $customer['MaKH']]);
            if ($emailQuery->fetchColumn()) {
                throw new InvalidArgumentException('Email này đã được sử dụng bởi tài khoản khác.');
            }
        }
        $update = $conn->prepare(
            'UPDATE KhachHang SET HoTen = :name, Sdt = :phone, Email = :email,
                NgaySinh = :birthDate, SoThich = :interests
             WHERE MaKH = :customerId AND TrangThai = 1'
        );
        $update->execute([
            'name' => $name,
            'phone' => $phone,
            'email' => $email !== '' ? $email : null,
            'birthDate' => $birth?->format('Y-m-d'),
            'interests' => $interests !== '' ? $interests : null,
            'customerId' => $customer['MaKH'],
        ]);
        $_SESSION['fullname'] = $name;
        $_SESSION['success'] = 'Đã cập nhật hồ sơ. Thông tin ngày sinh và sở thích giúp cửa hàng gửi khảo sát phù hợp hơn.';
        customer_portal_redirect('profile.php');
    } catch (InvalidArgumentException $exception) {
        $_SESSION['error'] = $exception->getMessage();
        customer_portal_redirect('profile.php');
    } catch (Throwable $exception) {
        error_log('Customer profile update failed: ' . $exception->getMessage());
        $_SESSION['error'] = 'Không thể lưu hồ sơ lúc này. Vui lòng thử lại sau.';
        customer_portal_redirect('profile.php');
    }
}

include __DIR__ . '/src/header.php';
?>
<section class="ftco-section contact-section">
  <div class="container mt-5">
    <div class="row justify-content-center"><div class="col-md-8">
      <h2 class="mb-2">Thông tin tài khoản</h2>
      <p>Cập nhật thông tin và sở thích để nhận khảo sát phù hợp.</p>
      <?php foreach (['success' => 'alert-success', 'error' => 'alert-danger'] as $notice => $class): ?>
        <?php if (isset($_SESSION[$notice])): ?><div class="alert <?= $class ?>"><?= customer_portal_escape($_SESSION[$notice]) ?></div><?php unset($_SESSION[$notice]); endif; ?>
      <?php endforeach; ?>
      <form method="post" action="profile.php" class="contact-form">
        <input type="hidden" name="csrf_token" value="<?= customer_portal_escape($_SESSION['customer_csrf']) ?>">
        <div class="form-group"><label for="profileName">Họ và tên</label><input id="profileName" class="form-control" name="full_name" maxlength="100" value="<?= customer_portal_escape($customer['HoTen']) ?>" required></div>
        <div class="form-group"><label for="profilePhone">Số điện thoại</label><input id="profilePhone" class="form-control" name="phone" maxlength="20" value="<?= customer_portal_escape($customer['Sdt']) ?>" required></div>
        <div class="form-group"><label for="profileEmail">Email</label><input id="profileEmail" class="form-control" name="email" type="email" maxlength="100" value="<?= customer_portal_escape($customer['Email'] ?? '') ?>"></div>
        <div class="form-group"><label for="profileBirthDate">Ngày sinh</label><input id="profileBirthDate" class="form-control" name="birth_date" type="date" max="<?= date('Y-m-d') ?>" value="<?= customer_portal_escape($customer['NgaySinh'] ?? '') ?>"></div>
        <div class="form-group"><label for="profileInterests">Sở thích</label><input id="profileInterests" class="form-control" name="interests" maxlength="500" value="<?= customer_portal_escape($customer['SoThich'] ?? '') ?>" placeholder="Ví dụ: cà phê, trà, bánh (cách nhau bằng dấu phẩy)"></div>
        <button type="submit" class="btn btn-primary py-3 px-5">Lưu thông tin</button>
      </form>
    </div></div>
  </div>
</section>
<?php include __DIR__ . '/src/footer.php'; ?>

<?php
require_once __DIR__ . '/session_helpers.php';
app_start_session();
?>
<?php if (isset($_SESSION['user_id'])): ?>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" id="dropdownCustomerAccount" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
            <?= htmlspecialchars((string) ($_SESSION['fullname'] ?? $_SESSION['username'] ?? 'Tài khoản'), ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8') ?>
        </a>
        <div class="dropdown-menu dropdown-menu-right" aria-labelledby="dropdownCustomerAccount">
            <a class="dropdown-item" href="profile.php">Thông tin tài khoản</a>
            <a class="dropdown-item" href="surveys.php">Khảo sát của tôi</a>
            <a class="dropdown-item" href="feedback.php">Gửi phản hồi</a>
            <a class="dropdown-item" href="logout.php">Đăng xuất</a>
        </div>
    </li>
<?php else: ?>
    <li class="nav-item <?= in_array($activePage ?? '', ['login', 'register'], true) ? 'active' : '' ?>">
        <a href="login.php" class="nav-link">Đăng nhập</a>
    </li>
<?php endif; ?>

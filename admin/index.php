<?php
require_once __DIR__ . '/AdminController.php';

$viewData = (new AdminController())->handle();
extract($viewData, EXTR_SKIP);
function money(int|float|string $amount): string
{
    return number_format((float) $amount, 0, ',', '.') . 'đ';
}

function status_class(string $status): string
{
    if (in_array($status, ['Hoàn thành', 'Hoàn tất', 'Đang bán', 'Thân thiết', 'Đang diễn ra', 'Đang mở'], true)) {
        return 'success';
    }
    if (in_array($status, ['Đang giao', 'Đang chuẩn bị', 'Thành viên'], true)) {
        return 'info';
    }
    if (in_array($status, ['Chờ xác nhận', 'Sắp hết', 'Sắp diễn ra'], true)) {
        return 'warning';
    }
    if (in_array($status, ['Đã hủy', 'Hết hàng', 'Đã kết thúc'], true)) {
        return 'muted';
    }
    return 'muted';
}

function nav_link(string $key, string $label, string $icon, string $activePage): void
{
    $active = $key === $activePage ? ' active' : '';
    echo '<a class="side-link' . $active . '" href="?page=' . htmlspecialchars($key, ENT_QUOTES, 'UTF-8') . '">'
        . '<span class="side-icon" aria-hidden="true">' . $icon . '</span>'
        . '<span>' . htmlspecialchars($label, ENT_QUOTES, 'UTF-8') . '</span></a>';
}
?>
<?php if (!$isAdminAuthenticated): ?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Đăng nhập | Coffee Blend Admin</title>
    <link rel="stylesheet" href="../css/open-iconic-bootstrap.min.css">
    <link rel="stylesheet" href="admin.css?v=2">
</head>
<body>
<main class="admin-shell"><div class="main-content">
    <section class="panel" style="max-width:460px;margin:10vh auto;padding:32px">
        <div class="eyebrow">COFFEE BLEND · ADMIN</div>
        <h1>Đăng nhập quản trị</h1>
        <p>Đăng nhập để quản lý sản phẩm và đơn hàng.</p>
        <?php if ($adminError !== ''): ?><div class="alert alert-danger" role="alert"><?= htmlspecialchars($adminError, ENT_QUOTES, 'UTF-8') ?></div><?php endif; ?>
        <form method="post" action="index.php" class="dialog-form">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($adminCsrf, ENT_QUOTES, 'UTF-8') ?>">
            <input type="hidden" name="action" value="login">
            <label>Tên đăng nhập<input name="username" type="text" autocomplete="username" required></label>
            <label>Mật khẩu<input name="password" type="password" autocomplete="current-password" required></label>
            <button class="button button-primary" type="submit">Đăng nhập</button>
        </form>
        <p class="dialog-note">Tài khoản phải có trạng thái hoạt động và vai trò Admin trong bảng taikhoan.</p>
    </section>
</div></main>
</body>
</html>
<?php exit; endif; ?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="Khu vực quản trị Coffee Blend">
    <title><?= htmlspecialchars($title, ENT_QUOTES, 'UTF-8') ?> | Coffee Blend Admin</title>
    <link rel="stylesheet" href="../css/open-iconic-bootstrap.min.css">
    <link rel="stylesheet" href="admin.css?v=2">
</head>
<body>
<div class="admin-shell">
    <aside class="sidebar" id="sidebar">
        <a class="brand" href="index.php" aria-label="Coffee Blend Admin">
            <span class="brand-mark">C</span>
            <span class="brand-copy">Coffee<small>Blend · ADMIN</small></span>
        </a>

        <div class="side-caption">MENU CHÍNH</div>
        <nav class="side-nav" aria-label="Điều hướng quản trị">
            <?php
            nav_link('dashboard', 'Tổng quan', '◫', $page);
            nav_link('products', 'Sản phẩm', '▤', $page);
            nav_link('orders', 'Đơn hàng', '▣', $page);
            nav_link('customers', 'Khách hàng', '♙', $page);
            nav_link('surveys', 'Khảo sát ý kiến', '▧', $page);
            ?>
        </nav>

        <div class="side-caption side-caption-spaced">HỆ THỐNG</div>
        <form method="post" action="index.php">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($adminCsrf, ENT_QUOTES, 'UTF-8') ?>">
            <input type="hidden" name="action" value="logout">
            <button class="side-link" type="submit"><span class="side-icon" aria-hidden="true">↪</span><span>Đăng xuất</span></button>
        </form>

        <div class="sidebar-bottom">
            <div class="help-card">
                <span class="help-mark">?</span>
                <strong>Cần hỗ trợ?</strong>
                <span>Liên hệ quản trị viên hệ thống.</span>
            </div>
            <a class="store-link" href="../index.php"><span aria-hidden="true">↗</span> Xem website cửa hàng</a>
            <div class="admin-profile">
                <div class="avatar avatar-admin">AD</div>
                <div class="profile-copy"><strong><?= htmlspecialchars($adminFullName, ENT_QUOTES, 'UTF-8') ?></strong><span><?= htmlspecialchars($adminUsername, ENT_QUOTES, 'UTF-8') ?> · <?= htmlspecialchars($adminRoleName, ENT_QUOTES, 'UTF-8') ?></span></div>
                <span class="profile-dots" aria-hidden="true">···</span>
            </div>
        </div>
    </aside>

    <main class="main-area">
        <?php if ($adminError !== ''): ?><div class="alert alert-danger" role="alert"><?= htmlspecialchars($adminError, ENT_QUOTES, 'UTF-8') ?></div><?php endif; ?>
        <?php if ($adminMessage !== ''): ?><div class="alert alert-<?= htmlspecialchars($adminMessageType, ENT_QUOTES, 'UTF-8') ?>" role="status"><?= htmlspecialchars($adminMessage, ENT_QUOTES, 'UTF-8') ?></div><?php endif; ?>
        <header class="topbar">
            <button class="icon-button menu-toggle" id="menuToggle" type="button" aria-label="Mở menu" aria-expanded="false">☰</button>
            <div class="breadcrumb"><span>Admin</span><span class="crumb-separator">/</span><strong><?= htmlspecialchars($title, ENT_QUOTES, 'UTF-8') ?></strong></div>
            <div class="topbar-actions">
                <label class="global-search">
                    <span aria-hidden="true">⌕</span>
                    <input type="search" id="tableSearch" placeholder="Tìm kiếm..." aria-label="Tìm kiếm trong bảng">
                    <kbd>⌘ K</kbd>
                </label>
                <button class="icon-button notification-button" type="button" aria-label="Thông báo">♧<i></i></button>
                <div class="topbar-divider"></div>
                <div class="topbar-user"><div class="avatar avatar-admin">AD</div><span><?= htmlspecialchars($adminFullName, ENT_QUOTES, 'UTF-8') ?></span><span class="chevron">⌄</span></div>
            </div>
        </header>

        <div class="page-content">
            <?php if ($page === 'dashboard'): ?>
                <?php
                $revenueChartMaximum = max(1.0, ...array_map(
                    static fn (array $day): float => $day['revenue'],
                    $dashboard['revenue_days']
                ));
                $revenueAxisMaximum = $revenueChartMaximum > 0
                    ? max(1000000, (int) (ceil($revenueChartMaximum / 4 / 1000000) * 4000000))
                    : 0;
                $revenueChange = $dashboard['revenue_previous_week'] > 0
                    ? ($dashboard['revenue_week'] - $dashboard['revenue_previous_week']) / $dashboard['revenue_previous_week'] * 100
                    : null;
                $categoryMaximum = max(1, ...array_map(
                    static fn (array $category): int => (int) $category['SoLuongBan'],
                    $dashboard['category_sales']
                ));
                ?>
                <section class="welcome-row">
                    <div>
                        <div class="eyebrow">TỔNG QUAN</div>
                        <h1>Tổng quan kinh doanh <span aria-hidden="true">☀</span></h1>
                        <p class="page-subtitle">Đây là tình hình kinh doanh của cửa hàng hôm nay.</p>
                    </div>
                    <div class="button button-outline"><span aria-hidden="true">▦</span> <?= (new DateTimeImmutable('today'))->format('d/m/Y') ?></div>
                </section>

                <section class="stats-grid" aria-label="Chỉ số kinh doanh">
                    <article class="stat-card">
                        <div class="stat-top"><span>Doanh thu hôm nay</span><span class="stat-icon revenue-icon">₫</span></div>
                        <div class="stat-value"><?= money($dashboard['revenue_today']) ?></div>
                        <div class="stat-foot"><span>Doanh thu hóa đơn trong ngày</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Đơn hàng</span><span class="stat-icon order-icon">▣</span></div>
                        <div class="stat-value"><?= $dashboard['orders_today'] ?></div>
                        <div class="stat-foot"><span>Đơn được tạo hôm nay</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Khách hàng mới</span><span class="stat-icon customer-icon">♙</span></div>
                        <div class="stat-value"><?= $dashboard['new_customers'] ?></div>
                        <div class="stat-foot"><span>Khách đăng ký trong 30 ngày gần nhất</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Giá trị đơn trung bình</span><span class="stat-icon average-icon">⌁</span></div>
                        <div class="stat-value"><?= money($dashboard['average_order']) ?></div>
                        <div class="stat-foot"><span>Giá trị trung bình đơn hôm nay</span></div>
                    </article>
                </section>

                <section class="dashboard-grid">
                    <article class="panel revenue-panel">
                        <div class="panel-heading">
                            <div><h2>Doanh thu</h2><p>Biến động doanh thu trong tuần này</p></div>
                            <button class="select-button" type="button">Tuần này <span aria-hidden="true">⌄</span></button>
                        </div>
                        <div class="chart-summary"><strong><?= money($dashboard['revenue_week']) ?></strong>
                            <?php if ($revenueChange !== null): ?><span class="<?= $revenueChange >= 0 ? 'trend-up' : 'trend-down' ?>"><?= $revenueChange >= 0 ? '↗' : '↘' ?> <?= number_format(abs($revenueChange), 1, ',', '.') ?>%</span><small>so với 7 ngày trước đó</small><?php else: ?><small>Chưa có dữ liệu 7 ngày trước đó để so sánh.</small><?php endif; ?>
                        </div>
                        <div class="chart" role="img" aria-label="Biểu đồ doanh thu trong tuần: thứ Hai đến Chủ nhật">
                            <div class="chart-y-axis"><?php for ($tick = 4; $tick >= 0; $tick--): ?><span><?= number_format($revenueAxisMaximum * $tick / 4 / 1000000, 1, ',', '.') ?>tr</span><?php endfor; ?></div>
                            <div class="chart-plot">
                                <div class="chart-grid-lines"><i></i><i></i><i></i><i></i><i></i></div>
                                <div class="chart-bars">
                                    <?php foreach ($dashboard['revenue_days'] as $day): $height = $day['revenue'] > 0 ? max(2, (int) round($day['revenue'] / $revenueAxisMaximum * 100)) : 0; ?>
                                        <div class="chart-column"><span class="bar-value"><?= number_format($day['revenue'] / 1000000, 1, ',', '.') ?>tr</span><div class="bar" style="height: <?= $height ?>%"></div><span class="bar-label"><?= $day['label'] ?></span></div>
                                    <?php endforeach; ?>
                                </div>
                            </div>
                        </div>
                    </article>

                    <article class="panel category-panel">
                        <div class="panel-heading"><div><h2>Số món theo danh mục</h2><p>Tổng số lượng trong chi tiết hóa đơn</p></div><a class="text-link" href="?page=products">Chi tiết <span aria-hidden="true">→</span></a></div>
                        <div class="category-total"><strong><?= $dashboard['products_sold'] ?></strong><span>món đã bán</span></div>
                        <div class="category-list">
                            <?php foreach ($dashboard['category_sales'] as $categorySale): $soldCount = (int) $categorySale['SoLuongBan']; $share = $dashboard['products_sold'] > 0 ? (int) round($soldCount / $dashboard['products_sold'] * 100) : 0; $barWidth = (int) round($soldCount / $categoryMaximum * 100); ?>
                            <div class="category-row"><span class="category-dot dot-other"></span><span><?= htmlspecialchars($categorySale['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></span><strong><?= $soldCount ?></strong><div class="mini-track"><i style="width: <?= $barWidth ?>%"></i></div><small><?= $share ?>%</small></div>
                            <?php endforeach; ?>
                        </div>
                        <?php if ($dashboard['products_sold'] === 0): ?><div class="category-note">Chưa có sản phẩm nào trong chi tiết hóa đơn.</div><?php endif; ?>
                    </article>
                </section>

                <section class="panel table-panel">
                    <div class="panel-heading table-heading">
                        <div><h2>Đơn hàng gần đây</h2><p>Theo dõi các đơn hàng mới nhất của cửa hàng.</p></div>
                        <a class="button button-outline button-small" href="?page=orders">Xem tất cả <span aria-hidden="true">→</span></a>
                    </div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>MÃ ĐƠN</th><th>KHÁCH HÀNG</th><th>THỜI GIAN</th><th>SẢN PHẨM</th><th>TỔNG TIỀN</th><th>TRẠNG THÁI</th><th></th></tr></thead>
                            <tbody>
                            <?php foreach (array_slice($orders, 0, 4) as $order): ?>
                                <?php $orderStatus = AdminController::orderStatus((string) ($order['TrangThai'] ?? '')); ?>
                                <tr data-search-row>
                                    <td class="order-id">#<?= (int) $order['MaHD'] ?></td>
                                    <td><?= htmlspecialchars($order['TenNguoiNhan'] ?? 'Khách hàng', ENT_QUOTES, 'UTF-8') ?></td>
                                    <td class="muted-cell"><?= htmlspecialchars((string) ($order['NgayTao'] ?? ''), ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><?= (int) $order['SoSanPham'] ?> món</td><td class="money-cell"><?= money($order['TongTien'] ?? 0) ?></td>
                                    <td><span class="status status-<?= status_class($orderStatus) ?>"><i></i><?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td></td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($orders === []): ?><tr><td colspan="7" class="muted-cell">Chưa có đơn hàng trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                </section>

            <?php elseif ($page === 'products'): ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Sản phẩm</h1><p class="page-subtitle">Quản lý danh mục và tình trạng sản phẩm của cửa hàng.</p></div>
                    <button class="button button-primary" type="button" id="openProductDialog"><span aria-hidden="true">＋</span> Thêm sản phẩm</button>
                </section>
                <section class="stats-grid compact-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng sản phẩm</span><span class="stat-icon revenue-icon">▤</span></div><div class="stat-value"><?= count($products) ?></div><div class="stat-foot"><span>Sản phẩm trong cơ sở dữ liệu</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đang kinh doanh</span><span class="stat-icon customer-icon">✓</span></div><div class="stat-value"><?= count(array_filter($products, static fn (array $product): bool => (bool) ($product['TrangThai'] ?? 1))) ?></div><div class="stat-foot"><span>Sản phẩm đang hiển thị</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Danh mục</span><span class="stat-icon average-icon">▧</span></div><div class="stat-value"><?= count($categories) ?></div><div class="stat-foot"><span>Danh mục hiện có</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đang ẩn</span><span class="stat-icon order-icon">×</span></div><div class="stat-value"><?= count(array_filter($products, static fn (array $product): bool => !(bool) ($product['TrangThai'] ?? 1))) ?></div><div class="stat-foot"><span>Không hiển thị trên cửa hàng</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách sản phẩm</h2><p>Dữ liệu lấy trực tiếp từ cơ sở dữ liệu.</p></div><div class="table-tools"><select class="filter-select" aria-label="Lọc danh mục"><option>Tất cả danh mục</option><?php foreach ($categories as $category): ?><option><?= htmlspecialchars($category['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></option><?php endforeach; ?></select><button class="button button-outline button-small" type="button" id="productSearchFocus">⌕ <span class="hide-mobile">Tìm sản phẩm</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>SẢN PHẨM</th><th>DANH MỤC</th><th>GIÁ BÁN</th><th>KÍCH CỠ</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($products as $product): ?>
                                <?php $productStatus = (bool) ($product['TrangThai'] ?? 1) ? 'Đang bán' : 'Ngừng bán'; ?>
                                <tr data-search-row>
                                    <td><div class="product-cell"><img src="../images/<?= rawurlencode((string) ($product['HinhAnh'] ?? 'menu-1.jpg')) ?>" alt="<?= htmlspecialchars($product['TenSP'], ENT_QUOTES, 'UTF-8') ?>"><div><strong><?= htmlspecialchars($product['TenSP'], ENT_QUOTES, 'UTF-8') ?></strong><small>Mã món: <?= (int) $product['MaSP'] ?></small></div></div></td>
                                    <td><?= htmlspecialchars($product['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></td><td class="money-cell"><?= money($product['GiaBan']) ?></td>
                                    <td><?= htmlspecialchars($product['KichCo'] ?? '—', ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><span class="status status-<?= status_class($productStatus) ?>"><i></i><?= $productStatus ?></span></td>
                                    <td><form method="post" action="index.php?page=products" onsubmit="return confirm('Xóa hoặc ẩn sản phẩm này?')"><input type="hidden" name="csrf_token" value="<?= htmlspecialchars($adminCsrf, ENT_QUOTES, 'UTF-8') ?>"><input type="hidden" name="action" value="delete_product"><input type="hidden" name="product_id" value="<?= (int) $product['MaSP'] ?>"><button class="row-more" type="submit" aria-label="Xóa <?= htmlspecialchars($product['TenSP'], ENT_QUOTES, 'UTF-8') ?>">×</button></form></td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($products === []): ?><tr><td colspan="6" class="muted-cell">Chưa có sản phẩm. Hãy thêm món mới để hiển thị trên cửa hàng.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($products) ?> sản phẩm</span></div>
                </section>

            <?php elseif ($page === 'orders'): ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Đơn hàng</h1><p class="page-subtitle">Theo dõi và cập nhật tình trạng các đơn hàng.</p></div>
                    <button class="button button-outline" type="button" id="exportButton"><span aria-hidden="true">↓</span> Xuất báo cáo</button>
                </section>
                <section class="stats-grid compact-stats three-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng đơn hàng</span><span class="stat-icon revenue-icon">▣</span></div><div class="stat-value"><?= count($orders) ?></div><div class="stat-foot"><span>Đơn trong cơ sở dữ liệu</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Chờ xác nhận</span><span class="stat-icon average-icon">◷</span></div><div class="stat-value"><?= count(array_filter($orders, static fn (array $order): bool => AdminController::orderStatus((string) $order['TrangThai']) === 'Chờ xác nhận')) ?></div><div class="stat-foot"><span>Cần xử lý sớm</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đã hoàn tất</span><span class="stat-icon customer-icon">✓</span></div><div class="stat-value"><?= count(array_filter($orders, static fn (array $order): bool => in_array(AdminController::orderStatus((string) $order['TrangThai']), ['Hoàn tất', 'Hoàn thành'], true))) ?></div><div class="stat-foot"><span>Đơn đã xử lý</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Tất cả đơn hàng</h2><p>Dữ liệu lấy trực tiếp từ cơ sở dữ liệu.</p></div><div class="table-tools"><select class="filter-select" aria-label="Lọc trạng thái"><option>Tất cả trạng thái</option><option>Chờ xác nhận</option><option>Đang giao</option><option>Hoàn tất</option><option>Hoàn thành</option><option>Đã hủy</option></select><button class="button button-outline button-small" type="button" id="orderSearchFocus">⌕ <span class="hide-mobile">Tìm đơn hàng</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>MÃ ĐƠN</th><th>KHÁCH HÀNG</th><th>THỜI GIAN</th><th>SẢN PHẨM</th><th>TỔNG TIỀN</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($orders as $order): ?>
                                <?php $rawOrderStatus = (string) ($order['TrangThai'] ?? ''); $orderStatus = AdminController::orderStatus($rawOrderStatus); ?>
                                <tr data-search-row data-status="<?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?>">
                                    <td class="order-id">#<?= (int) $order['MaHD'] ?></td>
                                    <td><div class="customer-info"><strong><?= htmlspecialchars($order['TenNguoiNhan'] ?? 'Khách hàng', ENT_QUOTES, 'UTF-8') ?></strong><small><?= htmlspecialchars($order['SdtNguoiNhan'] ?? '', ENT_QUOTES, 'UTF-8') ?></small></div></td>
                                    <td class="muted-cell"><?= htmlspecialchars((string) ($order['NgayTao'] ?? ''), ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><?= (int) $order['SoSanPham'] ?> món</td>
                                    <td class="money-cell"><?= money($order['TongTien'] ?? 0) ?></td>
                                    <td><span class="status status-<?= status_class($orderStatus) ?>"><i></i><?= htmlspecialchars($orderStatus, ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td>
                                        <?php if (in_array($orderStatus, ['Chờ xác nhận', 'Đang giao'], true)): ?>
                                        <form method="post" action="index.php?page=orders">
                                            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($adminCsrf, ENT_QUOTES, 'UTF-8') ?>">
                                            <input type="hidden" name="action" value="advance_order">
                                            <input type="hidden" name="order_id" value="<?= (int) $order['MaHD'] ?>">
                                            <input type="hidden" name="current_status" value="<?= htmlspecialchars($rawOrderStatus, ENT_QUOTES, 'UTF-8') ?>">
                                            <button class="button button-outline button-small" type="submit"><?= $orderStatus === 'Chờ xác nhận' ? 'Bắt đầu giao' : 'Hoàn tất' ?></button>
                                        </form>
                                        <?php endif; ?>
                                    </td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($orders === []): ?><tr><td colspan="7" class="muted-cell">Chưa có đơn hàng trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($orders) ?> đơn hàng</span></div>
                </section>

            <?php elseif ($page === 'surveys'): ?>
                <section class="welcome-row page-title-row survey-welcome">
                    <div><div class="eyebrow">THẤU HIỂU KHÁCH HÀNG</div><h1>Khảo sát ý kiến</h1><p class="page-subtitle">Lắng nghe khách hàng để cải thiện trải nghiệm tại Coffee Blend.</p></div>
                    <a class="button button-primary" href="surveys/index.php"><span aria-hidden="true">＋</span> Tạo khảo sát</a>
                </section>
                <section class="survey-callout">
                    <span class="survey-callout-icon" aria-hidden="true">✦</span>
                    <div><strong>Mỗi ý kiến đều giúp cửa hàng tốt hơn</strong><p>Theo dõi phản hồi, mức độ hài lòng và những góp ý cần ưu tiên xử lý.</p></div>
                    <span class="callout-decoration" aria-hidden="true">☕</span>
                </section>
                <section class="stats-grid compact-stats survey-stats" aria-label="Chỉ số khảo sát">
                    <article class="stat-card"><div class="stat-top"><span>Khảo sát đang mở</span><span class="stat-icon revenue-icon">▧</span></div><div class="stat-value"><?= $dashboard['survey_open'] ?></div><div class="stat-foot"><span>Trong <?= count($surveys) ?> khảo sát trong CSDL</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Tổng lượt mời</span><span class="stat-icon order-icon">➤</span></div><div class="stat-value"><?= $dashboard['survey_invitations'] ?></div><div class="stat-foot"><span>Tổng lượt mời đã ghi nhận</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Phản hồi khảo sát</span><span class="stat-icon customer-icon">▤</span></div><div class="stat-value"><?= $dashboard['survey_responses'] ?></div><div class="stat-foot"><span>Lượt mời có ít nhất một câu trả lời</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đánh giá sản phẩm</span><span class="stat-icon average-icon">★</span></div><div class="stat-value"><?= number_format($dashboard['average_rating'], 1, ',', '.') ?> <small>/ 5</small></div><div class="stat-foot"><span><?= array_sum($dashboard['rating_counts']) ?> lượt đánh giá trong PhanHoi</span></div></article>
                </section>
                <section class="survey-insights">
                    <article class="panel satisfaction-panel">
                        <div class="panel-heading"><div><h2>Đánh giá sản phẩm</h2><p>Phân bố điểm trong bảng PhanHoi</p></div><span class="survey-period"><?= array_sum($dashboard['rating_counts']) ?> lượt</span></div>
                        <div class="satisfaction-content">
                            <div class="rating-score"><strong><?= number_format($dashboard['average_rating'], 1, ',', '.') ?></strong><span class="rating-stars" aria-label="Điểm trung bình <?= number_format($dashboard['average_rating'], 1, ',', '.') ?> trên 5"><?= str_repeat('★', (int) round($dashboard['average_rating'])) ?><?= str_repeat('☆', 5 - (int) round($dashboard['average_rating'])) ?></span><small>trên 5 điểm</small></div>
                            <div class="rating-bars" aria-label="Phân bố số sao">
                                <?php for ($rating = 5; $rating >= 1; $rating--): $ratingCount = $dashboard['rating_counts'][$rating] ?? 0; $ratingPercent = array_sum($dashboard['rating_counts']) > 0 ? (int) round($ratingCount / array_sum($dashboard['rating_counts']) * 100) : 0; ?>
                                    <div class="rating-row"><span><?= $rating ?> sao</span><div class="rating-track"><i style="width: <?= $ratingPercent ?>%"></i></div><strong><?= $ratingCount ?></strong></div>
                                <?php endfor; ?>
                            </div>
                        </div>
                        <?php if (array_sum($dashboard['rating_counts']) === 0): ?><div class="insight-note">Chưa có đánh giá nào trong bảng PhanHoi.</div><?php endif; ?>
                    </article>
                    <article class="panel survey-focus-panel">
                        <div class="panel-heading"><div><h2>Dữ liệu khảo sát</h2><p>Thống kê đọc từ các bảng khảo sát hiện có</p></div></div>
                        <div class="focus-list">
                            <div class="focus-row"><span class="focus-number">01</span><div><strong>Khảo sát</strong><small>Bản ghi trong KhaoSat</small></div><b><?= count($surveys) ?></b></div>
                            <div class="focus-row"><span class="focus-number">02</span><div><strong>Câu hỏi</strong><small>Bản ghi trong CauHoiKhaoSat</small></div><b><?= array_sum(array_map(static fn (array $survey): int => (int) $survey['SoCauHoi'], $surveys)) ?></b></div>
                            <div class="focus-row"><span class="focus-number">03</span><div><strong>Ý kiến sản phẩm</strong><small>Bản ghi trong PhanHoi</small></div><b><?= $dashboard['feedback_count'] ?></b></div>
                        </div>
                        <div class="focus-foot">Chủ đề góp ý chưa được phân loại trong các bảng hiện tại.</div>
                    </article>
                </section>
                <section class="panel table-panel survey-list-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách khảo sát</h2><p>Theo dõi thời gian, lượt mời và tiến độ phản hồi.</p></div><div class="table-tools"><select class="filter-select" id="surveyStatusFilter" aria-label="Lọc trạng thái khảo sát"><option value="">Tất cả trạng thái</option><option value="Đang diễn ra">Đang diễn ra</option><option value="Sắp diễn ra">Sắp diễn ra</option><option value="Đã kết thúc">Đã kết thúc</option><option value="Chưa thiết lập">Chưa thiết lập</option></select><button class="button button-outline button-small" type="button" id="surveySearchFocus">⌕ <span class="hide-mobile">Tìm khảo sát</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table survey-table">
                            <thead><tr><th>KHẢO SÁT</th><th>LOẠI CÂU HỎI</th><th>THỜI GIAN</th><th>LƯỢT MỜI</th><th>PHẢN HỒI</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($surveys as $survey): ?>
                                <?php $responseRate = (int) $survey['SoLuotMoi'] > 0 ? (int) round((int) $survey['SoLuotPhanHoi'] / (int) $survey['SoLuotMoi'] * 100) : 0; $surveyStatus = (string) $survey['TrangThaiTinh']; ?>
                                <tr data-search-row data-status="<?= htmlspecialchars($surveyStatus, ENT_QUOTES, 'UTF-8') ?>">
                                    <td><div class="survey-name-cell"><span class="survey-row-icon" aria-hidden="true">▧</span><div><strong><?= htmlspecialchars($survey['TieuDe'], ENT_QUOTES, 'UTF-8') ?></strong><small>#<?= (int) $survey['MaKhaoSat'] ?> · <?= (int) $survey['SoCauHoi'] ?> câu hỏi</small></div></div></td>
                                    <td><?= (int) $survey['SoCauHoi'] ?> câu hỏi</td>
                                    <td class="survey-dates"><?= !empty($survey['NgayBatDau']) ? date('d/m/Y', strtotime((string) $survey['NgayBatDau'])) : '—' ?><small>đến <?= !empty($survey['NgayKetThuc']) ? date('d/m/Y', strtotime((string) $survey['NgayKetThuc'])) : '—' ?></small></td>
                                    <td><?= (int) $survey['SoLuotMoi'] ?></td>
                                    <td><div class="response-cell"><strong><?= (int) $survey['SoLuotPhanHoi'] ?></strong><span><?= $responseRate ?>%</span><div class="response-track"><i style="width: <?= $responseRate ?>%"></i></div></div></td>
                                    <td><span class="status status-<?= status_class($surveyStatus) ?>"><i></i><?= htmlspecialchars($surveyStatus, ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td><a class="button button-outline button-small" href="surveys/index.php?survey=<?= (int) $survey['MaKhaoSat'] ?>">Xem kết quả</a></td>
                                </tr>
                            <?php endforeach; ?>
                            <?php if ($surveys === []): ?><tr><td colspan="7" class="muted-cell">Chưa có khảo sát trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($surveys) ?> khảo sát</span><a class="text-link" href="#recentFeedback">Xem phản hồi mới <span aria-hidden="true">↓</span></a></div>
                </section>
                <section class="feedback-section" id="recentFeedback">
                    <div class="feedback-heading"><div><div class="eyebrow">TIẾNG NÓI KHÁCH HÀNG</div><h2>Đánh giá sản phẩm mới nhất</h2><p class="page-subtitle">Ý kiến và điểm đánh giá được lưu trong bảng PhanHoi.</p></div><button class="button button-outline button-small" type="button" id="feedbackSearchFocus">⌕ Tìm phản hồi</button></div>
                    <div class="feedback-grid">
                        <?php foreach ($feedback as $response): ?>
                            <?php $feedbackRating = max(0, min(5, (int) $response['DanhGia'])); ?>
                            <article class="panel feedback-card" data-search-row>
                                <div class="feedback-card-top"><div class="customer-cell"><span class="avatar avatar-lavender"><?= htmlspecialchars(mb_substr((string) ($response['HoTen'] ?? 'K'), 0, 1, 'UTF-8'), ENT_QUOTES, 'UTF-8') ?></span><div class="customer-info"><strong><?= htmlspecialchars($response['HoTen'] ?? 'Khách hàng', ENT_QUOTES, 'UTF-8') ?></strong><small><?= !empty($response['NgayGui']) ? date('d/m/Y H:i', strtotime((string) $response['NgayGui'])) : '—' ?></small></div></div><span class="feedback-stars" aria-label="<?= $feedbackRating ?> trên 5 sao"><?= str_repeat('★', $feedbackRating) ?><span><?= str_repeat('★', 5 - $feedbackRating) ?></span></span></div>
                                <div class="feedback-question"><?= htmlspecialchars($response['TenSP'] ?? 'Góp ý chung', ENT_QUOTES, 'UTF-8') ?> · Mã phản hồi <?= (int) $response['MaPhanHoi'] ?></div>
                                <p class="feedback-answer">“<?= htmlspecialchars($response['NoiDung'] ?? '', ENT_QUOTES, 'UTF-8') ?>”</p>
                                <div class="feedback-card-foot"><span>Trạng thái: <?= htmlspecialchars($response['TrangThaiXL'] ?? 'Chưa cập nhật', ENT_QUOTES, 'UTF-8') ?></span></div>
                            </article>
                        <?php endforeach; ?>
                        <?php if ($feedback === []): ?><p class="muted-cell">Chưa có ý kiến khách hàng trong bảng PhanHoi.</p><?php endif; ?>
                    </div>
                </section>

            <?php else: ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Khách hàng</h1><p class="page-subtitle">Thông tin và hoạt động mua hàng của khách hàng.</p></div>
                    <button class="button button-outline" type="button" id="customerExportButton"><span aria-hidden="true">↓</span> Xuất danh sách</button>
                </section>
                <section class="stats-grid compact-stats three-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng khách hàng</span><span class="stat-icon customer-icon">♙</span></div><div class="stat-value"><?= $dashboard['customer_count'] ?></div><div class="stat-foot"><span>Bản ghi trong bảng KhachHang</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Khách hàng mới</span><span class="stat-icon revenue-icon">＋</span></div><div class="stat-value"><?= $dashboard['new_customers'] ?></div><div class="stat-foot"><span>Đăng ký trong 30 ngày gần nhất</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Có hạng thành viên</span><span class="stat-icon average-icon">✦</span></div><div class="stat-value"><?= $dashboard['loyal_customer_count'] ?></div><div class="stat-foot"><span>HạngTV có giá trị trong CSDL</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách khách hàng</h2><p>Dữ liệu lấy từ bảng KhachHang và HoaDon.</p></div><button class="button button-outline button-small" type="button" id="customerSearchFocus">⌕ <span class="hide-mobile">Tìm khách hàng</span></button></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>KHÁCH HÀNG</th><th>SỐ ĐIỆN THOẠI</th><th>ĐƠN HÀNG</th><th>TỔNG CHI TIÊU</th><th>HẠNG THÀNH VIÊN</th></tr></thead>
                            <tbody>
                            <?php foreach ($customers as $customer): ?>
                                <?php $customerTier = trim((string) ($customer['HangTV'] ?? '')) !== '' ? (string) $customer['HangTV'] : 'Chưa xếp hạng'; ?>
                                <tr data-search-row><td><div class="customer-cell"><span class="avatar avatar-lavender"><?= htmlspecialchars(mb_substr($customer['HoTen'], 0, 1, 'UTF-8'), ENT_QUOTES, 'UTF-8') ?></span><div class="customer-info"><strong><?= htmlspecialchars($customer['HoTen'], ENT_QUOTES, 'UTF-8') ?></strong><small>Mã khách hàng: <?= (int) $customer['MaKH'] ?></small></div></div></td><td><?= htmlspecialchars($customer['Sdt'], ENT_QUOTES, 'UTF-8') ?></td><td><?= (int) $customer['SoDonHang'] ?></td><td class="money-cell"><?= money($customer['TongChiTieu']) ?></td><td><span class="status status-<?= status_class($customerTier) ?>"><i></i><?= htmlspecialchars($customerTier, ENT_QUOTES, 'UTF-8') ?></span></td></tr>
                            <?php endforeach; ?>
                            <?php if ($customers === []): ?><tr><td colspan="5" class="muted-cell">Chưa có khách hàng trong cơ sở dữ liệu.</td></tr><?php endif; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($customers) ?> khách hàng</span></div>
                </section>
            <?php endif; ?>
            <footer class="page-footer">© 2026 Coffee Blend Admin <span>Số liệu quản trị được tổng hợp từ cơ sở dữ liệu coffee_shop_db</span></footer>
        </div>
    </main>
</div>

<?php if ($page === 'products'): ?>
<dialog class="product-dialog" id="productDialog">
    <form method="post" action="index.php?page=products" enctype="multipart/form-data" class="dialog-form">
        <div class="dialog-heading"><div><div class="eyebrow">SẢN PHẨM MỚI</div><h2>Thêm sản phẩm</h2></div><button class="icon-button dialog-close" type="button" aria-label="Đóng">×</button></div>
        <input type="hidden" name="csrf_token" value="<?= htmlspecialchars($adminCsrf, ENT_QUOTES, 'UTF-8') ?>">
        <input type="hidden" name="action" value="create_product">
        <label>Tên sản phẩm<input name="name" type="text" maxlength="100" placeholder="Ví dụ: Cà phê sữa đá" required></label>
        <div class="dialog-fields">
            <label>Danh mục<select name="category_id" required><option value="">Chọn danh mục</option><?php foreach ($categories as $category): ?><option value="<?= (int) $category['MaLoaiSP'] ?>"><?= htmlspecialchars($category['TenLoaiSP'], ENT_QUOTES, 'UTF-8') ?></option><?php endforeach; ?></select></label>
            <label>Giá bán (đ)<input name="price" type="number" min="0" step="0.01" placeholder="0" required></label>
        </div>
        <label>Kích cỡ (không bắt buộc)<input name="size" type="text" maxlength="10" placeholder="M, L hoặc mặc định"></label>
        <label>Ảnh sản phẩm (JPG, PNG, WEBP · tối đa 5 MB)<input name="image" type="file" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" required></label>
        <div class="dialog-actions"><button class="button button-outline dialog-cancel" type="button">Hủy</button><button class="button button-primary" type="submit">Lưu sản phẩm</button></div>
    </form>
</dialog>
<?php endif; ?>
<div class="toast" id="adminToast" role="status" aria-live="polite"></div>
<div class="sidebar-backdrop" id="sidebarBackdrop"></div>
<script src="admin.js?v=2"></script>
</body>
</html>

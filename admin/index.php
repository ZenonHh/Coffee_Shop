<?php
$pages = [
    'dashboard' => ['title' => 'Tổng quan', 'eyebrow' => 'TỔNG QUAN'],
    'products' => ['title' => 'Sản phẩm', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
    'orders' => ['title' => 'Đơn hàng', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
    'customers' => ['title' => 'Khách hàng', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
    'surveys' => ['title' => 'Khảo sát ý kiến', 'eyebrow' => 'THẤU HIỂU KHÁCH HÀNG'],
];

$page = $_GET['page'] ?? 'dashboard';
if (!is_string($page) || !isset($pages[$page])) {
    $page = 'dashboard';
}
$title = $pages[$page]['title'];

$products = [
    ['name' => 'Cà phê Cappuccino', 'category' => 'Cà phê', 'price' => 59000, 'stock' => 42, 'image' => 'menu-1.jpg', 'status' => 'Đang bán'],
    ['name' => 'Cà phê Latte', 'category' => 'Cà phê', 'price' => 49000, 'stock' => 28, 'image' => 'menu-2.jpg', 'status' => 'Đang bán'],
    ['name' => 'Trà đào cam sả', 'category' => 'Trà', 'price' => 45000, 'stock' => 3, 'image' => 'drink-3.jpg', 'status' => 'Sắp hết'],
    ['name' => 'Chocolate đá xay', 'category' => 'Đồ uống', 'price' => 55000, 'stock' => 16, 'image' => 'drink-5.jpg', 'status' => 'Đang bán'],
    ['name' => 'Tiramisu', 'category' => 'Bánh ngọt', 'price' => 39000, 'stock' => 0, 'image' => 'dessert-1.jpg', 'status' => 'Hết hàng'],
];

$orders = [
    ['id' => '#DH1024', 'customer' => 'Nguyễn Minh Anh', 'date' => '30/09/2026 · 14:32', 'items' => 3, 'total' => 163000, 'status' => 'Chờ xác nhận'],
    ['id' => '#DH1023', 'customer' => 'Trần Hoàng Nam', 'date' => '30/09/2026 · 14:18', 'items' => 2, 'total' => 108000, 'status' => 'Đang chuẩn bị'],
    ['id' => '#DH1022', 'customer' => 'Lê Thảo Vy', 'date' => '30/09/2026 · 13:56', 'items' => 4, 'total' => 221000, 'status' => 'Đang giao'],
    ['id' => '#DH1021', 'customer' => 'Phạm Gia Bảo', 'date' => '30/09/2026 · 13:21', 'items' => 1, 'total' => 59000, 'status' => 'Hoàn thành'],
    ['id' => '#DH1020', 'customer' => 'Đỗ Ngọc Hà', 'date' => '30/09/2026 · 12:47', 'items' => 2, 'total' => 94000, 'status' => 'Đã hủy'],
    ['id' => '#DH1019', 'customer' => 'Vũ Đức Thành', 'date' => '30/09/2026 · 12:12', 'items' => 3, 'total' => 145000, 'status' => 'Hoàn thành'],
];

$customers = [
    ['name' => 'Nguyễn Minh Anh', 'email' => 'minhanh@example.com', 'phone' => '090 123 4567', 'orders' => 12, 'spent' => 1845000, 'tier' => 'Thân thiết'],
    ['name' => 'Trần Hoàng Nam', 'email' => 'hoangnam@example.com', 'phone' => '091 234 5678', 'orders' => 8, 'spent' => 1260000, 'tier' => 'Thân thiết'],
    ['name' => 'Lê Thảo Vy', 'email' => 'thaovy@example.com', 'phone' => '093 345 6789', 'orders' => 5, 'spent' => 745000, 'tier' => 'Thành viên'],
    ['name' => 'Phạm Gia Bảo', 'email' => 'giabao@example.com', 'phone' => '098 456 7890', 'orders' => 3, 'spent' => 329000, 'tier' => 'Thành viên'],
    ['name' => 'Đỗ Ngọc Hà', 'email' => 'ngocha@example.com', 'phone' => '097 567 8901', 'orders' => 1, 'spent' => 94000, 'tier' => 'Mới'],
];

$surveys = [
    ['id' => 'KS-008', 'title' => 'Trải nghiệm khách hàng tháng 9', 'description' => 'Lắng nghe cảm nhận về dịch vụ và trải nghiệm tại cửa hàng.', 'type' => 'Mức độ hài lòng', 'questions' => 6, 'invitations' => 240, 'responses' => 186, 'start' => '15/09/2026', 'end' => '05/10/2026', 'status' => 'Đang diễn ra'],
    ['id' => 'KS-007', 'title' => 'Chất lượng đồ uống mới', 'description' => 'Đánh giá hương vị và gợi ý cho menu đồ uống mùa thu.', 'type' => 'Đánh giá sản phẩm', 'questions' => 5, 'invitations' => 180, 'responses' => 164, 'start' => '01/09/2026', 'end' => '20/09/2026', 'status' => 'Đã kết thúc'],
    ['id' => 'KS-006', 'title' => 'Không gian & trải nghiệm tại quán', 'description' => 'Tìm hiểu cảm nhận về không gian, âm nhạc và thời gian phục vụ.', 'type' => 'Câu hỏi hỗn hợp', 'questions' => 8, 'invitations' => 320, 'responses' => 218, 'start' => '20/08/2026', 'end' => '10/09/2026', 'status' => 'Đã kết thúc'],
    ['id' => 'KS-009', 'title' => 'Khám phá thức uống mùa lễ hội', 'description' => 'Chọn hương vị khách hàng mong chờ trong menu cuối năm.', 'type' => 'Khảo sát lựa chọn', 'questions' => 4, 'invitations' => 0, 'responses' => 0, 'start' => '08/10/2026', 'end' => '25/10/2026', 'status' => 'Sắp diễn ra'],
];

$feedback = [
    ['initials' => 'MA', 'name' => 'Nguyễn Minh Anh', 'order' => '#DH1018', 'date' => '30/09/2026 · 13:42', 'rating' => 5, 'question' => 'Bạn hài lòng thế nào về chất lượng đồ uống?', 'answer' => 'Latte thơm, vị cân bằng và được phục vụ rất nhanh. Mình sẽ quay lại!'],
    ['initials' => 'TV', 'name' => 'Lê Thảo Vy', 'order' => '#DH1012', 'date' => '30/09/2026 · 11:18', 'rating' => 4, 'question' => 'Bạn đánh giá không gian cửa hàng ra sao?', 'answer' => 'Không gian thoải mái, nhạc dễ chịu. Nếu có thêm ổ cắm ở khu vực cửa sổ thì sẽ tuyệt hơn.'],
    ['initials' => 'NB', 'name' => 'Ngọc Bảo (ẩn danh)', 'order' => 'Khảo sát trải nghiệm', 'date' => '29/09/2026 · 19:05', 'rating' => 3, 'question' => 'Bạn muốn cửa hàng cải thiện điều gì?', 'answer' => 'Giờ cao điểm phải chờ hơi lâu. Mong quán có thêm nhân viên vào buổi tối.'],
];

function money(int $amount): string
{
    return number_format($amount, 0, ',', '.') . 'đ';
}

function status_class(string $status): string
{
    if (in_array($status, ['Hoàn thành', 'Đang bán', 'Thân thiết', 'Đang diễn ra', 'Đang mở'], true)) {
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
        <a class="side-link side-disabled" href="#" aria-disabled="true"><span class="side-icon" aria-hidden="true">⚙</span><span>Cài đặt</span><span class="soon-tag">Sắp có</span></a>

        <div class="sidebar-bottom">
            <div class="help-card">
                <span class="help-mark">?</span>
                <strong>Cần hỗ trợ?</strong>
                <span>Liên hệ quản trị viên hệ thống.</span>
            </div>
            <a class="store-link" href="../index.php"><span aria-hidden="true">↗</span> Xem website cửa hàng</a>
            <div class="admin-profile">
                <div class="avatar avatar-admin">AD</div>
                <div class="profile-copy"><strong>Quản trị viên</strong><span>admin@coffeeblend.vn</span></div>
                <span class="profile-dots" aria-hidden="true">···</span>
            </div>
        </div>
    </aside>

    <main class="main-area">
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
                <div class="topbar-user"><div class="avatar avatar-admin">AD</div><span>Admin</span><span class="chevron">⌄</span></div>
            </div>
        </header>

        <div class="page-content">
            <?php if ($page === 'dashboard'): ?>
                <section class="welcome-row">
                    <div>
                        <div class="eyebrow">TỔNG QUAN</div>
                        <h1>Chào buổi chiều, Admin <span aria-hidden="true">☀</span></h1>
                        <p class="page-subtitle">Đây là tình hình kinh doanh của cửa hàng hôm nay.</p>
                    </div>
                    <button class="button button-outline" type="button" id="dateButton"><span aria-hidden="true">▦</span> 30 tháng 9, 2026 <span aria-hidden="true">⌄</span></button>
                </section>

                <section class="stats-grid" aria-label="Chỉ số kinh doanh">
                    <article class="stat-card">
                        <div class="stat-top"><span>Doanh thu hôm nay</span><span class="stat-icon revenue-icon">₫</span></div>
                        <div class="stat-value">8.450.000đ</div>
                        <div class="stat-foot"><span class="trend-up">↗ 12,8%</span><span>so với hôm qua</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Đơn hàng</span><span class="stat-icon order-icon">▣</span></div>
                        <div class="stat-value">126</div>
                        <div class="stat-foot"><span class="trend-up">↗ 8,2%</span><span>so với hôm qua</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Khách hàng mới</span><span class="stat-icon customer-icon">♙</span></div>
                        <div class="stat-value">38</div>
                        <div class="stat-foot"><span class="trend-up">↗ 5,4%</span><span>so với hôm qua</span></div>
                    </article>
                    <article class="stat-card">
                        <div class="stat-top"><span>Giá trị đơn trung bình</span><span class="stat-icon average-icon">⌁</span></div>
                        <div class="stat-value">67.063đ</div>
                        <div class="stat-foot"><span class="trend-down">↘ 2,1%</span><span>so với hôm qua</span></div>
                    </article>
                </section>

                <section class="dashboard-grid">
                    <article class="panel revenue-panel">
                        <div class="panel-heading">
                            <div><h2>Doanh thu</h2><p>Biến động doanh thu trong tuần này</p></div>
                            <button class="select-button" type="button">Tuần này <span aria-hidden="true">⌄</span></button>
                        </div>
                        <div class="chart-summary"><strong>42.680.000đ</strong><span class="trend-up">↗ 10,4%</span><small>so với tuần trước</small></div>
                        <div class="chart" role="img" aria-label="Biểu đồ doanh thu trong tuần: thứ Hai đến Chủ nhật">
                            <div class="chart-y-axis"><span>10tr</span><span>7.5tr</span><span>5tr</span><span>2.5tr</span><span>0</span></div>
                            <div class="chart-plot">
                                <div class="chart-grid-lines"><i></i><i></i><i></i><i></i><i></i></div>
                                <div class="chart-bars">
                                    <?php foreach ([48, 67, 55, 82, 61, 93, 72] as $index => $height): ?>
                                        <div class="chart-column"><span class="bar-value"><?= [5.1, 6.8, 5.9, 8.2, 6.4, 9.1, 7.3][$index] ?>tr</span><div class="bar<?= $index === 5 ? ' bar-highlight' : '' ?>" style="height: <?= $height ?>%"></div><span class="bar-label"><?= ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'][$index] ?></span></div>
                                    <?php endforeach; ?>
                                </div>
                            </div>
                        </div>
                    </article>

                    <article class="panel category-panel">
                        <div class="panel-heading"><div><h2>Sản phẩm bán chạy</h2><p>Theo số lượng bán ra</p></div><a class="text-link" href="?page=products">Chi tiết <span aria-hidden="true">→</span></a></div>
                        <div class="category-total"><strong>1.284</strong><span>sản phẩm</span><span class="trend-up">↗ 6,3%</span></div>
                        <div class="category-list">
                            <div class="category-row"><span class="category-dot dot-coffee"></span><span>Cà phê</span><strong>548</strong><div class="mini-track"><i style="width:43%"></i></div><small>43%</small></div>
                            <div class="category-row"><span class="category-dot dot-drink"></span><span>Trà & đồ uống</span><strong>372</strong><div class="mini-track"><i style="width:29%"></i></div><small>29%</small></div>
                            <div class="category-row"><span class="category-dot dot-dessert"></span><span>Bánh ngọt</span><strong>231</strong><div class="mini-track"><i style="width:18%"></i></div><small>18%</small></div>
                            <div class="category-row"><span class="category-dot dot-other"></span><span>Khác</span><strong>133</strong><div class="mini-track"><i style="width:10%"></i></div><small>10%</small></div>
                        </div>
                        <div class="category-note"><span aria-hidden="true">✦</span> Cà phê chiếm phần lớn doanh số tuần này.</div>
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
                                <tr data-search-row>
                                    <td class="order-id"><?= htmlspecialchars($order['id'], ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><div class="customer-cell"><span class="avatar avatar-<?= $order === $orders[0] ? 'peach' : 'lavender' ?>"><?= htmlspecialchars(mb_substr($order['customer'], 0, 1, 'UTF-8'), ENT_QUOTES, 'UTF-8') ?></span><?= htmlspecialchars($order['customer'], ENT_QUOTES, 'UTF-8') ?></div></td>
                                    <td class="muted-cell"><?= htmlspecialchars($order['date'], ENT_QUOTES, 'UTF-8') ?></td>
                                    <td><?= (int) $order['items'] ?> món</td><td class="money-cell"><?= money($order['total']) ?></td>
                                    <td><span class="status status-<?= status_class($order['status']) ?>"><i></i><?= htmlspecialchars($order['status'], ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td><button class="row-more" type="button" aria-label="Thao tác với <?= htmlspecialchars($order['id'], ENT_QUOTES, 'UTF-8') ?>">···</button></td>
                                </tr>
                            <?php endforeach; ?>
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
                    <article class="stat-card"><div class="stat-top"><span>Tổng sản phẩm</span><span class="stat-icon revenue-icon">▤</span></div><div class="stat-value">48</div><div class="stat-foot"><span>Trong 5 danh mục</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đang kinh doanh</span><span class="stat-icon customer-icon">✓</span></div><div class="stat-value">42</div><div class="stat-foot"><span class="trend-up">87,5%</span><span> tổng sản phẩm</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Sắp hết hàng</span><span class="stat-icon average-icon">!</span></div><div class="stat-value">4</div><div class="stat-foot"><span>Cần kiểm tra tồn kho</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Hết hàng</span><span class="stat-icon order-icon">×</span></div><div class="stat-value">2</div><div class="stat-foot"><span>Đang ẩn khỏi cửa hàng</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách sản phẩm</h2><p>Dữ liệu minh họa · Chưa kết nối cơ sở dữ liệu</p></div><div class="table-tools"><select class="filter-select" aria-label="Lọc danh mục"><option>Tất cả danh mục</option><option>Cà phê</option><option>Trà</option><option>Đồ uống</option><option>Bánh ngọt</option></select><button class="button button-outline button-small" type="button" id="productSearchFocus">⌕ <span class="hide-mobile">Tìm sản phẩm</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>SẢN PHẨM</th><th>DANH MỤC</th><th>GIÁ BÁN</th><th>TỒN KHO</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($products as $product): ?>
                                <tr data-search-row>
                                    <td><div class="product-cell"><img src="../images/<?= htmlspecialchars($product['image'], ENT_QUOTES, 'UTF-8') ?>" alt=""><div><strong><?= htmlspecialchars($product['name'], ENT_QUOTES, 'UTF-8') ?></strong><small>SKU: CF-<?= str_pad((string) (array_search($product, $products, true) + 1), 3, '0', STR_PAD_LEFT) ?></small></div></div></td>
                                    <td><?= htmlspecialchars($product['category'], ENT_QUOTES, 'UTF-8') ?></td><td class="money-cell"><?= money($product['price']) ?></td>
                                    <td class="<?= $product['stock'] < 5 ? 'stock-low' : '' ?>"><?= (int) $product['stock'] ?> sản phẩm</td>
                                    <td><span class="status status-<?= status_class($product['status']) ?>"><i></i><?= htmlspecialchars($product['status'], ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td><button class="row-more" type="button" aria-label="Thao tác với <?= htmlspecialchars($product['name'], ENT_QUOTES, 'UTF-8') ?>">···</button></td>
                                </tr>
                            <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Hiển thị <?= count($products) ?> trên 48 sản phẩm mẫu</span><div class="pagination"><button type="button" disabled aria-label="Trang trước">‹</button><button class="current" type="button" aria-current="page">1</button><button type="button">2</button><button type="button">3</button><span>...</span><button type="button">10</button><button type="button" aria-label="Trang sau">›</button></div></div>
                </section>

            <?php elseif ($page === 'orders'): ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Đơn hàng</h1><p class="page-subtitle">Theo dõi và cập nhật tình trạng các đơn hàng.</p></div>
                    <button class="button button-outline" type="button" id="exportButton"><span aria-hidden="true">↓</span> Xuất báo cáo</button>
                </section>
                <section class="stats-grid compact-stats three-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng đơn hôm nay</span><span class="stat-icon revenue-icon">▣</span></div><div class="stat-value">126</div><div class="stat-foot"><span class="trend-up">↗ 8,2%</span><span> so với hôm qua</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Chờ xác nhận</span><span class="stat-icon average-icon">◷</span></div><div class="stat-value">8</div><div class="stat-foot"><span>Cần xử lý sớm</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Đã hoàn thành</span><span class="stat-icon customer-icon">✓</span></div><div class="stat-value">94</div><div class="stat-foot"><span>74,6% tổng đơn hôm nay</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Tất cả đơn hàng</h2><p>Dữ liệu minh họa · Chưa kết nối cơ sở dữ liệu</p></div><div class="table-tools"><select class="filter-select" aria-label="Lọc trạng thái"><option>Tất cả trạng thái</option><option>Chờ xác nhận</option><option>Đang chuẩn bị</option><option>Đang giao</option><option>Hoàn thành</option></select><button class="button button-outline button-small" type="button" id="orderSearchFocus">⌕ <span class="hide-mobile">Tìm đơn hàng</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>MÃ ĐƠN</th><th>KHÁCH HÀNG</th><th>THỜI GIAN</th><th>SẢN PHẨM</th><th>TỔNG TIỀN</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($orders as $order): ?>
                                <tr data-search-row><td class="order-id"><?= htmlspecialchars($order['id'], ENT_QUOTES, 'UTF-8') ?></td><td><?= htmlspecialchars($order['customer'], ENT_QUOTES, 'UTF-8') ?></td><td class="muted-cell"><?= htmlspecialchars($order['date'], ENT_QUOTES, 'UTF-8') ?></td><td><?= (int) $order['items'] ?> món</td><td class="money-cell"><?= money($order['total']) ?></td><td><span class="status status-<?= status_class($order['status']) ?>"><i></i><?= htmlspecialchars($order['status'], ENT_QUOTES, 'UTF-8') ?></span></td><td><button class="row-more" type="button" aria-label="Thao tác với <?= htmlspecialchars($order['id'], ENT_QUOTES, 'UTF-8') ?>">···</button></td></tr>
                            <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($orders) ?> đơn hàng mẫu</span><div class="pagination"><button type="button" disabled aria-label="Trang trước">‹</button><button class="current" type="button" aria-current="page">1</button><button type="button">2</button><button type="button">3</button><span>...</span><button type="button">21</button><button type="button" aria-label="Trang sau">›</button></div></div>
                </section>

            <?php elseif ($page === 'surveys'): ?>
                <section class="welcome-row page-title-row survey-welcome">
                    <div><div class="eyebrow">THẤU HIỂU KHÁCH HÀNG</div><h1>Khảo sát ý kiến</h1><p class="page-subtitle">Lắng nghe khách hàng để cải thiện trải nghiệm tại Coffee Blend.</p></div>
                    <button class="button button-primary" type="button" id="openSurveyDialog"><span aria-hidden="true">＋</span> Tạo khảo sát</button>
                </section>
                <section class="survey-callout">
                    <span class="survey-callout-icon" aria-hidden="true">✦</span>
                    <div><strong>Mỗi ý kiến đều giúp cửa hàng tốt hơn</strong><p>Theo dõi phản hồi, mức độ hài lòng và những góp ý cần ưu tiên xử lý.</p></div>
                    <span class="callout-decoration" aria-hidden="true">☕</span>
                </section>
                <section class="stats-grid compact-stats survey-stats" aria-label="Chỉ số khảo sát">
                    <article class="stat-card"><div class="stat-top"><span>Khảo sát đang mở</span><span class="stat-icon revenue-icon">▧</span></div><div class="stat-value">1</div><div class="stat-foot"><span>KS-008 · kết thúc 05/10</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Tổng lượt mời</span><span class="stat-icon order-icon">➤</span></div><div class="stat-value">740</div><div class="stat-foot"><span>Trong 3 khảo sát gần nhất</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Phản hồi nhận được</span><span class="stat-icon customer-icon">▤</span></div><div class="stat-value">568</div><div class="stat-foot"><span class="trend-up">↗ 12,4%</span><span> so với tháng trước</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Điểm hài lòng trung bình</span><span class="stat-icon average-icon">★</span></div><div class="stat-value">4,6 <small>/ 5</small></div><div class="stat-foot"><span class="star-inline">★★★★★</span><span> từ 568 phản hồi</span></div></article>
                </section>
                <section class="survey-insights">
                    <article class="panel satisfaction-panel">
                        <div class="panel-heading"><div><h2>Mức độ hài lòng</h2><p>Kết quả khảo sát trải nghiệm tháng 9</p></div><span class="survey-period">Tháng 9, 2026</span></div>
                        <div class="satisfaction-content">
                            <div class="rating-score"><strong>4,6</strong><span class="rating-stars" aria-label="4,6 trên 5 sao">★★★★★</span><small>trên 5 điểm</small><span class="trend-up">↗ 0,3 điểm</span></div>
                            <div class="rating-bars" aria-label="Phân bố số sao">
                                <?php foreach ([['5 sao', 72, 134], ['4 sao', 20, 37], ['3 sao', 6, 11], ['2 sao', 2, 3], ['1 sao', 0, 1]] as $rating): ?>
                                    <div class="rating-row"><span><?= $rating[0] ?></span><div class="rating-track"><i style="width: <?= $rating[1] ?>%"></i></div><strong><?= $rating[2] ?></strong></div>
                                <?php endforeach; ?>
                            </div>
                        </div>
                        <div class="insight-note"><span aria-hidden="true">✦</span> 92% khách hàng đánh giá trải nghiệm từ 4 sao trở lên.</div>
                    </article>
                    <article class="panel survey-focus-panel">
                        <div class="panel-heading"><div><h2>Điều khách hàng quan tâm</h2><p>Chủ đề được nhắc đến nhiều nhất</p></div></div>
                        <div class="focus-list">
                            <div class="focus-row"><span class="focus-number">01</span><div><strong>Chất lượng đồ uống</strong><small>Hương vị, nhiệt độ, cách trình bày</small></div><b>38%</b></div>
                            <div class="focus-row"><span class="focus-number">02</span><div><strong>Thời gian phục vụ</strong><small>Tốc độ pha chế vào giờ cao điểm</small></div><b>27%</b></div>
                            <div class="focus-row"><span class="focus-number">03</span><div><strong>Không gian cửa hàng</strong><small>Chỗ ngồi, âm nhạc và ổ cắm</small></div><b>21%</b></div>
                        </div>
                        <div class="focus-foot">Dựa trên nội dung phản hồi khảo sát mẫu.</div>
                    </article>
                </section>
                <section class="panel table-panel survey-list-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách khảo sát</h2><p>Theo dõi thời gian, lượt mời và tiến độ phản hồi.</p></div><div class="table-tools"><select class="filter-select" id="surveyStatusFilter" aria-label="Lọc trạng thái khảo sát"><option value="">Tất cả trạng thái</option><option value="Đang diễn ra">Đang diễn ra</option><option value="Sắp diễn ra">Sắp diễn ra</option><option value="Đã kết thúc">Đã kết thúc</option></select><button class="button button-outline button-small" type="button" id="surveySearchFocus">⌕ <span class="hide-mobile">Tìm khảo sát</span></button></div></div>
                    <div class="table-scroll">
                        <table class="data-table survey-table">
                            <thead><tr><th>KHẢO SÁT</th><th>LOẠI CÂU HỎI</th><th>THỜI GIAN</th><th>LƯỢT MỜI</th><th>PHẢN HỒI</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($surveys as $survey): ?>
                                <?php $responseRate = $survey['invitations'] > 0 ? (int) round($survey['responses'] / $survey['invitations'] * 100) : 0; ?>
                                <tr data-search-row data-status="<?= htmlspecialchars($survey['status'], ENT_QUOTES, 'UTF-8') ?>">
                                    <td><div class="survey-name-cell"><span class="survey-row-icon" aria-hidden="true">▧</span><div><strong><?= htmlspecialchars($survey['title'], ENT_QUOTES, 'UTF-8') ?></strong><small><?= htmlspecialchars($survey['id'], ENT_QUOTES, 'UTF-8') ?> · <?= (int) $survey['questions'] ?> câu hỏi</small></div></div></td>
                                    <td><?= htmlspecialchars($survey['type'], ENT_QUOTES, 'UTF-8') ?></td>
                                    <td class="survey-dates"><?= htmlspecialchars($survey['start'], ENT_QUOTES, 'UTF-8') ?><small>đến <?= htmlspecialchars($survey['end'], ENT_QUOTES, 'UTF-8') ?></small></td>
                                    <td><?= (int) $survey['invitations'] ?></td>
                                    <td><div class="response-cell"><strong><?= (int) $survey['responses'] ?></strong><span><?= $responseRate ?>%</span><div class="response-track"><i style="width: <?= $responseRate ?>%"></i></div></div></td>
                                    <td><span class="status status-<?= status_class($survey['status']) ?>"><i></i><?= htmlspecialchars($survey['status'], ENT_QUOTES, 'UTF-8') ?></span></td>
                                    <td><button class="row-more survey-details-button" type="button" aria-label="Xem phản hồi khảo sát <?= htmlspecialchars($survey['id'], ENT_QUOTES, 'UTF-8') ?>">···</button></td>
                                </tr>
                            <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($surveys) ?> khảo sát mẫu</span><a class="text-link" href="#recentFeedback">Xem phản hồi mới <span aria-hidden="true">↓</span></a></div>
                </section>
                <section class="feedback-section" id="recentFeedback">
                    <div class="feedback-heading"><div><div class="eyebrow">TIẾNG NÓI KHÁCH HÀNG</div><h2>Phản hồi mới nhất</h2><p class="page-subtitle">Một vài ý kiến gần đây từ các khảo sát của cửa hàng.</p></div><button class="button button-outline button-small" type="button" id="feedbackSearchFocus">⌕ Tìm phản hồi</button></div>
                    <div class="feedback-grid">
                        <?php foreach ($feedback as $response): ?>
                            <article class="panel feedback-card" data-search-row>
                                <div class="feedback-card-top"><div class="customer-cell"><span class="avatar avatar-lavender"><?= htmlspecialchars($response['initials'], ENT_QUOTES, 'UTF-8') ?></span><div class="customer-info"><strong><?= htmlspecialchars($response['name'], ENT_QUOTES, 'UTF-8') ?></strong><small><?= htmlspecialchars($response['date'], ENT_QUOTES, 'UTF-8') ?></small></div></div><span class="feedback-stars" aria-label="<?= (int) $response['rating'] ?> trên 5 sao"><?= str_repeat('★', $response['rating']) ?><span><?= str_repeat('★', 5 - $response['rating']) ?></span></span></div>
                                <div class="feedback-question"><?= htmlspecialchars($response['question'], ENT_QUOTES, 'UTF-8') ?></div>
                                <p class="feedback-answer">“<?= htmlspecialchars($response['answer'], ENT_QUOTES, 'UTF-8') ?>”</p>
                                <div class="feedback-card-foot"><span><?= htmlspecialchars($response['order'], ENT_QUOTES, 'UTF-8') ?></span><button class="feedback-action" type="button">Đánh dấu đã xem <span aria-hidden="true">→</span></button></div>
                            </article>
                        <?php endforeach; ?>
                    </div>
                </section>

            <?php else: ?>
                <section class="welcome-row page-title-row">
                    <div><div class="eyebrow">QUẢN LÝ CỬA HÀNG</div><h1>Khách hàng</h1><p class="page-subtitle">Thông tin và hoạt động mua hàng của khách hàng.</p></div>
                    <button class="button button-outline" type="button" id="customerExportButton"><span aria-hidden="true">↓</span> Xuất danh sách</button>
                </section>
                <section class="stats-grid compact-stats three-stats">
                    <article class="stat-card"><div class="stat-top"><span>Tổng khách hàng</span><span class="stat-icon customer-icon">♙</span></div><div class="stat-value">1.284</div><div class="stat-foot"><span class="trend-up">↗ 6,8%</span><span> tháng này</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Khách hàng mới</span><span class="stat-icon revenue-icon">＋</span></div><div class="stat-value">38</div><div class="stat-foot"><span>Trong 30 ngày gần nhất</span></div></article>
                    <article class="stat-card"><div class="stat-top"><span>Khách thân thiết</span><span class="stat-icon average-icon">✦</span></div><div class="stat-value">216</div><div class="stat-foot"><span>16,8% tổng khách hàng</span></div></article>
                </section>
                <section class="panel table-panel">
                    <div class="panel-heading table-heading"><div><h2>Danh sách khách hàng</h2><p>Dữ liệu minh họa · Chưa kết nối cơ sở dữ liệu</p></div><button class="button button-outline button-small" type="button" id="customerSearchFocus">⌕ <span class="hide-mobile">Tìm khách hàng</span></button></div>
                    <div class="table-scroll">
                        <table class="data-table">
                            <thead><tr><th>KHÁCH HÀNG</th><th>SỐ ĐIỆN THOẠI</th><th>ĐƠN HÀNG</th><th>TỔNG CHI TIÊU</th><th>HẠNG THÀNH VIÊN</th><th>THAO TÁC</th></tr></thead>
                            <tbody>
                            <?php foreach ($customers as $customer): ?>
                                <tr data-search-row><td><div class="customer-cell"><span class="avatar avatar-lavender"><?= htmlspecialchars(mb_substr($customer['name'], 0, 1, 'UTF-8'), ENT_QUOTES, 'UTF-8') ?></span><div class="customer-info"><strong><?= htmlspecialchars($customer['name'], ENT_QUOTES, 'UTF-8') ?></strong><small><?= htmlspecialchars($customer['email'], ENT_QUOTES, 'UTF-8') ?></small></div></div></td><td><?= htmlspecialchars($customer['phone'], ENT_QUOTES, 'UTF-8') ?></td><td><?= (int) $customer['orders'] ?></td><td class="money-cell"><?= money($customer['spent']) ?></td><td><span class="status status-<?= status_class($customer['tier']) ?>"><i></i><?= htmlspecialchars($customer['tier'], ENT_QUOTES, 'UTF-8') ?></span></td><td><button class="row-more" type="button" aria-label="Thao tác với <?= htmlspecialchars($customer['name'], ENT_QUOTES, 'UTF-8') ?>">···</button></td></tr>
                            <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                    <div class="table-footer"><span>Đang hiển thị <?= count($customers) ?> khách hàng mẫu</span><div class="pagination"><button type="button" disabled aria-label="Trang trước">‹</button><button class="current" type="button" aria-current="page">1</button><button type="button">2</button><button type="button">3</button><span>...</span><button type="button">26</button><button type="button" aria-label="Trang sau">›</button></div></div>
                </section>
            <?php endif; ?>
            <footer class="page-footer">© 2026 Coffee Blend Admin <span>Giao diện quản trị · Dữ liệu hiện chỉ mang tính minh họa</span></footer>
        </div>
    </main>
</div>

<?php if ($page === 'products'): ?>
<dialog class="product-dialog" id="productDialog">
    <form method="dialog" class="dialog-form">
        <div class="dialog-heading"><div><div class="eyebrow">SẢN PHẨM MỚI</div><h2>Thêm sản phẩm</h2></div><button class="icon-button dialog-close" value="cancel" aria-label="Đóng">×</button></div>
        <p class="dialog-note">Đây là bản xem trước giao diện. Dữ liệu chưa được lưu vào hệ thống.</p>
        <label>Tên sản phẩm<input type="text" placeholder="Ví dụ: Cà phê sữa đá"></label>
        <div class="dialog-fields"><label>Danh mục<select><option>Chọn danh mục</option><option>Cà phê</option><option>Trà</option><option>Đồ uống</option><option>Bánh ngọt</option></select></label><label>Giá bán<input type="number" min="0" placeholder="0"></label></div>
        <div class="dialog-actions"><button class="button button-outline" value="cancel">Hủy</button><button class="button button-primary" id="saveProductPreview" value="cancel">Lưu sản phẩm</button></div>
    </form>
</dialog>
<?php endif; ?>
<?php if ($page === 'surveys'): ?>
<dialog class="survey-dialog product-dialog" id="surveyDialog">
    <form method="dialog" class="dialog-form">
        <div class="dialog-heading"><div><div class="eyebrow">KHẢO SÁT MỚI</div><h2>Tạo khảo sát khách hàng</h2></div><button class="icon-button dialog-close" value="cancel" aria-label="Đóng">×</button></div>
        <p class="dialog-note">Tạo khảo sát để thu thập ý kiến và cải thiện trải nghiệm khách hàng.</p>
        <label>Tiêu đề khảo sát<input type="text" maxlength="255" placeholder="Ví dụ: Đánh giá trải nghiệm tháng 10"></label>
        <label>Mô tả<textarea rows="3" placeholder="Chia sẻ mục đích khảo sát với khách hàng"></textarea></label>
        <div class="dialog-fields">
            <label>Ngày bắt đầu<input type="date"></label>
            <label>Ngày kết thúc<input type="date"></label>
        </div>
        <div class="survey-question-preview"><strong>Câu hỏi khảo sát</strong>Phần quản lý câu hỏi sẽ được bổ sung cùng chức năng lưu khảo sát.</div>
        <div class="dialog-actions"><button class="button button-outline" value="cancel">Hủy</button><button class="button button-primary" id="saveSurveyPreview" value="cancel">Tạo khảo sát</button></div>
    </form>
</dialog>
<?php endif; ?>
<div class="toast" id="adminToast" role="status" aria-live="polite"></div>
<div class="sidebar-backdrop" id="sidebarBackdrop"></div>
<script src="admin.js?v=2"></script>
</body>
</html>

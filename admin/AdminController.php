<?php

require_once __DIR__ . '/AdminModel.php';

class AdminController
{
    private string $error = '';
    private string $message = '';
    private string $messageType = 'success';
    private bool $authenticated;
    private string $page;
    private array $pages = [
        'dashboard' => ['title' => 'Tổng quan', 'eyebrow' => 'TỔNG QUAN'],
        'products'  => ['title' => 'Sản phẩm', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
        'orders'    => ['title' => 'Đơn hàng', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
        'customers' => ['title' => 'Khách hàng', 'eyebrow' => 'QUẢN LÝ CỬA HÀNG'],
        'reviews'   => ['title' => 'Đánh giá món uống', 'eyebrow' => 'Ý KIẾN KHÁCH HÀNG'],
        'surveys'   => ['title' => 'Khảo sát ý kiến', 'eyebrow' => 'THẤU HIỂU KHÁCH HÀNG'],
        'accounts'  => ['title' => 'Quản lý tài khoản', 'eyebrow' => 'QUẢN TRỊ HỆ THỐNG'],
    ];

    public function __construct()
    {
        if (session_status() !== PHP_SESSION_ACTIVE) {
            session_set_cookie_params([
                'httponly' => true,
                'secure'   => !empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off',
                'samesite' => 'Lax',
            ]);
            session_start();
        }
        $_SESSION['admin_csrf'] ??= bin2hex(random_bytes(32));
        $this->message = (string) ($_SESSION['admin_message'] ?? '');
        $this->messageType = (string) ($_SESSION['admin_message_type'] ?? 'success');
        unset($_SESSION['admin_message'], $_SESSION['admin_message_type']);
        $this->authenticated = ($_SESSION['admin_authenticated'] ?? false) === true;
        $requestedPage = $_GET['page'] ?? 'dashboard';
        $this->page = is_string($requestedPage) && isset($this->pages[$requestedPage])
            ? $requestedPage
            : 'dashboard';
    }

    public function handle(): array
    {
        $logoutRequested = ($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'POST'
            && ($_POST['action'] ?? '') === 'logout';
        if ($this->authenticated && !$logoutRequested) {
            $this->refreshAuthenticatedAccount();
        }
        if (($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'POST') {
            $this->handlePost();
        }

        $data = [
            'adminError'           => $this->error,
            'adminMessage'         => $this->message,
            'adminMessageType'     => $this->messageType,
            'isAdminAuthenticated' => $this->authenticated,
            'adminCsrf'            => $_SESSION['admin_csrf'],
            'adminFullName'        => (string) ($_SESSION['admin_fullname'] ?? 'Quản trị viên'),
            'adminRoleName'        => (string) ($_SESSION['admin_role_name'] ?? ''),
            'adminUsername'        => (string) ($_SESSION['admin_username'] ?? ''),
            'pages'                => $this->pages,
            'page'                 => $this->page,
            'title'                => $this->pages[$this->page]['title'],
            'products'             => [],
            'orders'               => [],
            'categories'           => [],
            'customers'            => [],
            'surveys'              => [],
            'feedback'             => [],
            'accounts'             => [],
            'roles'                => [],
            'dashboard'            => [
                'revenue_today'         => 0.0,
                'orders_today'          => 0,
                'new_customers'         => 0,
                'average_order'         => 0.0,
                'revenue_week'          => 0.0,
                'revenue_previous_week' => 0.0,
                'products_sold'         => 0,
                'revenue_days'          => [],
                'category_sales'        => [],
                'customer_count'        => 0,
                'loyal_customer_count'  => 0,
                'survey_open'           => 0,
                'survey_invitations'    => 0,
                'survey_responses'      => 0,
                'average_rating'        => 0.0,
                'rating_counts'         => [1 => 0, 2 => 0, 3 => 0, 4 => 0, 5 => 0],
                'feedback_count'        => 0,
            ],
        ];

        if (!$this->authenticated) {
            return $data;
        }
        
        // Chặn người dùng không phải Admin truy cập trang Quản lý tài khoản
        $currentRole = (string) ($_SESSION['admin_role_name'] ?? '');
        if ($this->page === 'accounts' && strcasecmp($currentRole, 'Admin') !== 0) {
            $_SESSION['admin_message'] = 'Bạn không có quyền truy cập vào khu vực Quản lý tài khoản!';
            $_SESSION['admin_message_type'] = 'danger';
            $this->redirect('index.php?page=dashboard');
        }

        try {
            $model = new AdminModel();
            $db = @mysqli_connect('localhost', 'root', '', 'coffee_shop_db') 
                  ?: @mysqli_connect('localhost', 'root', '', 'coffee_shop');
            if ($db) {
                mysqli_set_charset($db, 'utf8mb4');
            }

            if ($this->page === 'accounts') {
                if ($db) {
                    $accQuery = "SELECT tk.MaTaiKhoan, tk.TenDangNhap, tk.HoTen, tk.TrangThai, tk.MaVaiTro, vt.TenVaiTro 
                                 FROM taikhoan tk
                                 LEFT JOIN vaitro vt ON tk.MaVaiTro = vt.MaVaiTro
                                 ORDER BY tk.MaTaiKhoan ASC";
                    $resAcc = $db->query($accQuery);
                    if ($resAcc) {
                        while ($row = $resAcc->fetch_assoc()) {
                            $data['accounts'][] = $row;
                        }
                    }

                    $resRoles = $db->query("SELECT * FROM vaitro ORDER BY MaVaiTro ASC");
                    if ($resRoles) {
                        while ($r = $resRoles->fetch_assoc()) {
                            $data['roles'][] = $r;
                        }
                    }
                }
            } elseif ($this->page === 'reviews') {
                $surveyData  = $model->loadPage('surveys');
                $productData = $model->loadPage('products');

                $data['feedback']  = $surveyData['feedback'] ?? [];
                $data['products']  = $productData['products'] ?? [];
                $data['dashboard']['rating_counts']  = $surveyData['dashboard']['rating_counts'] ?? [1 => 0, 2 => 0, 3 => 0, 4 => 0, 5 => 0];
                $data['dashboard']['average_rating'] = $surveyData['dashboard']['average_rating'] ?? 0.0;
                $data['dashboard']['feedback_count'] = count($data['feedback']);

                if (empty($data['feedback']) && $db) {
                    $sql = "SELECT ph.*, sp.TenSP, COALESCE(kh.HoTen, 'Khách vãng lai') AS HoTen, COALESCE(kh.Sdt, '') AS Sdt 
                            FROM PhanHoi ph
                            LEFT JOIN SanPham sp ON ph.MaSP = sp.MaSP
                            LEFT JOIN KhachHang kh ON ph.MaKH = kh.MaKH
                            ORDER BY ph.NgayGui DESC";
                    $res = $db->query($sql);
                    if ($res) {
                        $fbs = [];
                        $rCounts = [1 => 0, 2 => 0, 3 => 0, 4 => 0, 5 => 0];
                        $rSum = 0;
                        while ($r = $res->fetch_assoc()) {
                            $fbs[] = $r;
                            $star = max(1, min(5, (int)($r['DanhGia'] ?? 5)));
                            $rCounts[$star]++;
                            $rSum += $star;
                        }
                        $data['feedback'] = $fbs;
                        $data['dashboard']['rating_counts'] = $rCounts;
                        $data['dashboard']['average_rating'] = count($fbs) > 0 ? round($rSum / count($fbs), 1) : 0.0;
                        $data['dashboard']['feedback_count'] = count($fbs);
                    }
                }
            } else {
                $data = array_merge($data, $model->loadPage($this->page));
            }
        } catch (Throwable $exception) {
            error_log('Admin data load failed: ' . $exception->getMessage());
            $data['adminError'] = 'Không thể tải dữ liệu quản trị.';
        }

        return $data;
    }

    private function handlePost(): void
    {
        $postedToken = $_POST['csrf_token'] ?? '';
        if (!is_string($postedToken) || !hash_equals($_SESSION['admin_csrf'], $postedToken)) {
            $this->error = 'Phiên làm việc không hợp lệ. Vui lòng tải lại trang.';
            return;
        }

        $action = is_string($_POST['action'] ?? null) ? $_POST['action'] : '';
        if ($action === 'login') {
            $this->login();
            return;
        }
        if ($action === 'logout') {
            $this->clearAdminSession();
            $this->authenticated = false;
            session_regenerate_id(true);
            $_SESSION['admin_csrf'] = bin2hex(random_bytes(32));
            $this->redirect('index.php');
        }
        if (!$this->authenticated) {
            http_response_code(403);
            $this->error = 'Vui lòng đăng nhập với tài khoản quản trị.';
            return;
        }

        try {
            $model = new AdminModel();
            $db = @mysqli_connect('localhost', 'root', '', 'coffee_shop_db') 
                  ?: @mysqli_connect('localhost', 'root', '', 'coffee_shop');
            if ($db) {
                mysqli_set_charset($db, 'utf8mb4');
            }

            if ($action === 'create_product') {
                $this->createProduct($model);
                $this->message = 'Đã thêm sản phẩm.';
            } elseif ($action === 'delete_product') {
                $productId = filter_var($_POST['product_id'] ?? null, FILTER_VALIDATE_INT);
                if (!$productId || $productId < 1) {
                    throw new InvalidArgumentException('Mã sản phẩm không hợp lệ.');
                }
                $model->hideProduct($productId);
                $this->message = 'Đã ẩn sản phẩm khỏi menu; dữ liệu đơn hàng được giữ nguyên.';
            } elseif ($action === 'advance_order') {
                $orderId = filter_var($_POST['order_id'] ?? null, FILTER_VALIDATE_INT);
                $currentStatus = is_string($_POST['current_status'] ?? null) ? $_POST['current_status'] : '';
                if (!$orderId || $orderId < 1) {
                    throw new InvalidArgumentException('Mã đơn hàng không hợp lệ.');
                }
                $nextStatus = $model->advanceOrder($orderId, $currentStatus);
                $this->message = 'Đã chuyển đơn hàng sang trạng thái ' . self::orderStatus($nextStatus) . '.';
            } elseif ($action === 'update_feedback_status') {
                $feedbackId = filter_var($_POST['feedback_id'] ?? null, FILTER_VALIDATE_INT);
                $newStatus   = trim((string)($_POST['status'] ?? 'Đã xử lý'));
                if ($db && $feedbackId) {
                    $stmt = $db->prepare("UPDATE PhanHoi SET TrangThaiXL = ? WHERE MaPhanHoi = ?");
                    if ($stmt) {
                        $stmt->bind_param("si", $newStatus, $feedbackId);
                        $stmt->execute();
                        $stmt->close();
                    }
                }
                $this->message = 'Đã cập nhật trạng thái phản hồi.';
            } elseif (in_array($action, ['create_account', 'update_account_role', 'toggle_account_status'], true)) {
                $currentRole = (string) ($_SESSION['admin_role_name'] ?? '');
                if (strcasecmp($currentRole, 'Admin') !== 0) {
                    throw new InvalidArgumentException('Bạn không có quyền thực hiện thao tác quản trị tài khoản.');
                }

                if ($action === 'create_account') {
                    $username = trim((string)($_POST['username'] ?? ''));
                    $fullname = trim((string)($_POST['fullname'] ?? ''));
                    $password = trim((string)($_POST['password'] ?? ''));
                    $roleId   = filter_var($_POST['role_id'] ?? null, FILTER_VALIDATE_INT);

                    if ($username === '' || $fullname === '' || $password === '' || !$roleId) {
                        throw new InvalidArgumentException('Vui lòng điền đầy đủ thông tin tài khoản.');
                    }

                    if ($db) {
                        $chk = $db->prepare("SELECT MaTaiKhoan FROM taikhoan WHERE TenDangNhap = ? LIMIT 1");
                        $chk->bind_param("s", $username);
                        $chk->execute();
                        if ($chk->get_result()->fetch_assoc()) {
                            $chk->close();
                            throw new InvalidArgumentException('Tên đăng nhập này đã được sử dụng.');
                        }
                        $chk->close();

                        $hash = password_hash($password, PASSWORD_DEFAULT);
                        $ins = $db->prepare("INSERT INTO taikhoan (MaVaiTro, TenDangNhap, MatKhau, HoTen, TrangThai) VALUES (?, ?, ?, ?, 1)");
                        $ins->bind_param("isss", $roleId, $username, $hash, $fullname);
                        $ins->execute();
                        $ins->close();
                    }
                    $this->message = 'Đã thêm tài khoản mới thành công.';
                } elseif ($action === 'update_account_role') {
                    $accId  = filter_var($_POST['account_id'] ?? null, FILTER_VALIDATE_INT);
                    $roleId = filter_var($_POST['role_id'] ?? null, FILTER_VALIDATE_INT);

                    if (!$accId || !$roleId) {
                        throw new InvalidArgumentException('Dữ liệu phân quyền không hợp lệ.');
                    }
                    if ($db) {
                        $stmt = $db->prepare("UPDATE taikhoan SET MaVaiTro = ? WHERE MaTaiKhoan = ?");
                        $stmt->bind_param("ii", $roleId, $accId);
                        $stmt->execute();
                        $stmt->close();
                    }
                    $this->message = 'Đã cập nhật vai trò phân quyền cho tài khoản.';
                } elseif ($action === 'toggle_account_status') {
                    $accId     = filter_var($_POST['account_id'] ?? null, FILTER_VALIDATE_INT);
                    $newStatus = filter_var($_POST['new_status'] ?? 0, FILTER_VALIDATE_INT);

                    if ($accId === (int)$_SESSION['admin_account_id']) {
                        throw new InvalidArgumentException('Bạn không thể tự khóa tài khoản của chính mình.');
                    }

                    if ($db && $accId) {
                        $stmt = $db->prepare("UPDATE taikhoan SET TrangThai = ? WHERE MaTaiKhoan = ?");
                        $stmt->bind_param("ii", $newStatus, $accId);
                        $stmt->execute();
                        $stmt->close();
                    }
                    $this->message = $newStatus === 1 ? 'Đã kích hoạt lại tài khoản.' : 'Đã khóa tài khoản thành công.';
                }
            } else {
                throw new InvalidArgumentException('Thao tác không hợp lệ.');
            }

            $_SESSION['admin_message'] = $this->message;
            $_SESSION['admin_message_type'] = 'success';
            $this->redirect('index.php?page=' . rawurlencode($this->page));
        } catch (InvalidArgumentException $exception) {
            $this->error = $exception->getMessage();
        } catch (Throwable $exception) {
            error_log('Admin operation failed: ' . $exception->getMessage());
            $this->error = 'Không thể lưu thay đổi do lỗi hệ thống.';
        }
    }

    private function login(): void
    {
        $enteredUsername = is_string($_POST['username'] ?? null) ? $_POST['username'] : '';
        $enteredPassword = is_string($_POST['password'] ?? null) ? $_POST['password'] : '';
        try {
            $account = (new AdminModel())->authenticateAdmin($enteredUsername);
        } catch (Throwable $exception) {
            error_log('Admin login failed: ' . $exception->getMessage());
            $this->error = 'Không thể kiểm tra tài khoản quản trị.';
            return;
        }
        if ($account !== null && password_verify($enteredPassword, (string) $account['MatKhau'])) {
            session_regenerate_id(true);
            $_SESSION['admin_authenticated'] = true;
            $_SESSION['admin_account_id']    = (int) $account['MaTaiKhoan'];
            $_SESSION['admin_role_id']       = (int) $account['MaVaiTro'];
            $_SESSION['admin_role_name']     = (string) $account['TenVaiTro'];
            $_SESSION['admin_username']      = (string) $account['TenDangNhap'];
            $_SESSION['admin_fullname']      = (string) $account['HoTen'];
            $_SESSION['admin_csrf']          = bin2hex(random_bytes(32));
            $this->authenticated = true;
            $this->redirect('index.php');
        } else {
            $this->error = 'Tên đăng nhập hoặc mật khẩu không chính xác.';
        }
    }

    private function refreshAuthenticatedAccount(): void
    {
        $accountId = filter_var($_SESSION['admin_account_id'] ?? null, FILTER_VALIDATE_INT);
        if (!$accountId || $accountId < 1) {
            $this->clearAdminSession();
            $this->authenticated = false;
            return;
        }
        try {
            $account = (new AdminModel())->findActiveAdmin($accountId);
        } catch (Throwable $exception) {
            error_log('Admin session validation failed: ' . $exception->getMessage());
            $this->clearAdminSession();
            $this->authenticated = false;
            $this->error = 'Không thể xác minh quyền quản trị.';
            return;
        }
        if ($account === null) {
            $this->clearAdminSession();
            $this->authenticated = false;
            $this->error = 'Tài khoản không còn quyền truy cập khu vực quản trị.';
            return;
        }
        $_SESSION['admin_account_id'] = (int) $account['MaTaiKhoan'];
        $_SESSION['admin_role_id']    = (int) $account['MaVaiTro'];
        $_SESSION['admin_role_name']  = (string) $account['TenVaiTro'];
        $_SESSION['admin_username']   = (string) $account['TenDangNhap'];
        $_SESSION['admin_fullname']   = (string) $account['HoTen'];
    }

    private function clearAdminSession(): void
    {
        unset(
            $_SESSION['admin_authenticated'],
            $_SESSION['admin_account_id'],
            $_SESSION['admin_role_id'],
            $_SESSION['admin_role_name'],
            $_SESSION['admin_username'],
            $_SESSION['admin_fullname'],
            $_SESSION['admin_user']
        );
    }

    private function createProduct(AdminModel $model): void
    {
        $name = is_string($_POST['name'] ?? null) ? trim($_POST['name']) : '';
        $categoryId = filter_var($_POST['category_id'] ?? null, FILTER_VALIDATE_INT);
        $price = filter_var($_POST['price'] ?? null, FILTER_VALIDATE_FLOAT);
        $size = is_string($_POST['size'] ?? null) ? trim($_POST['size']) : '';
        $size = $size !== '' ? $size : 'Mặc định';
        $image = $_FILES['image'] ?? null;

        if ($name === '' || mb_strlen($name, 'UTF-8') > 100 || !$categoryId || $price === false || $price < 0 || mb_strlen($size, 'UTF-8') > 10) {
            throw new InvalidArgumentException('Vui lòng nhập tên, danh mục và giá bán hợp lệ; kích cỡ tối đa 10 ký tự.');
        }
        if (!$model->categoryExists($categoryId)) {
            throw new InvalidArgumentException('Danh mục sản phẩm không hợp lệ.');
        }
        if (!is_array($image) || ($image['error'] ?? UPLOAD_ERR_NO_FILE) !== UPLOAD_ERR_OK) {
            throw new InvalidArgumentException('Vui lòng chọn ảnh sản phẩm.');
        }
        if (!is_string($image['tmp_name'] ?? null) || !is_int($image['size'] ?? null) || $image['size'] < 1 || $image['size'] > 5 * 1024 * 1024 || !is_uploaded_file($image['tmp_name'])) {
            throw new InvalidArgumentException('Ảnh phải có dung lượng tối đa 5 MB.');
        }
        $mimeType = (new finfo(FILEINFO_MIME_TYPE))->file($image['tmp_name']);
        $extensions = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/webp' => 'webp'];
        if (!isset($extensions[$mimeType]) || getimagesize($image['tmp_name']) === false) {
            throw new InvalidArgumentException('Ảnh chỉ được dùng định dạng JPG, PNG hoặc WEBP hợp lệ.');
        }

        $imageName = bin2hex(random_bytes(16)) . '.' . $extensions[$mimeType];
        $imagePath = dirname(__DIR__) . '/images/' . $imageName;
        if (!move_uploaded_file($image['tmp_name'], $imagePath)) {
            throw new RuntimeException('Không thể lưu ảnh sản phẩm vào thư mục images.');
        }
        try {
            $model->createProduct($categoryId, $name, $size, (float) $price, $imageName);
        } catch (Throwable $exception) {
            if (is_file($imagePath) && !unlink($imagePath)) {
                error_log('Unable to remove product image after database failure: ' . $imagePath);
            }
            throw $exception;
        }
    }

    public static function orderStatus(string $status): string
    {
        return trim($status) === '0' || trim($status) === ''
            ? 'Chờ xác nhận'
            : $status;
    }

    private function redirect(string $location): never
    {
        header('Location: ' . $location, true, 303);
        exit;
    }
}